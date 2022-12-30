using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Remotely.Server.Components;
using Remotely.Server.Components.ModalContents;
using Remotely.Server.Services;
using Remotely.Shared.Models;
using Remotely.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Remotely.Server.Pages
{
    public partial class ManageOrganization : AuthComponentBase
    {
        private readonly List<DeviceGroup> _deviceGroups = new();
        private readonly List<InviteLink> _invites = new();
        private readonly List<RemotelyUser> _orgUsers = new();
        private bool _inviteAsAdmin;
        private string _inviteEmail;
        private string _newDeviceGroupName;
        private Organization _organization;
        private string _selectedDeviceGroupId;

        [Inject]
        private IDataService DataService { get; set; }

        [Inject]
        private IEmailSenderEx EmailSender { get; set; }

        [Inject]
        private IJsInterop JsInterop { get; set; }

        [Inject]
        private IModalService ModalService { get; set; }

        [Inject]
        private NavigationManager NavManager { get; set; }

        [Inject]
        private IToastService ToastService { get; set; }
        [Inject]
        private UserManager<RemotelyUser> UserManager { get; set; }


        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            await RefreshData();
        }

        private void CreateNewDeviceGroup()
        {
            if (!User.IsAdministrator)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_newDeviceGroupName))
            {
                return;
            }

            var deviceGroup = new DeviceGroup()
            {
                Name = _newDeviceGroupName
            };

            var result = DataService.AddDeviceGroup(User.OrganizationID, deviceGroup, out _, out var errorMessage);
            if (!result)
            {
                ToastService.ShowToast(errorMessage, classString: "bg-danger");
                return;
            }

            ToastService.ShowToast("Groupe d'appareils créé.");
            _deviceGroups.Add(deviceGroup);
            _newDeviceGroupName = string.Empty;
        }

        private void DefaultOrgCheckChanged(ChangeEventArgs args)
        {
            if (!User.IsServerAdmin)
            {
                return;
            }

            var isDefault = (bool)args.Value;
            DataService.SetIsDefaultOrganization(_organization.ID, isDefault);
            ToastService.ShowToast("Entreprise par défaut enregistrée.");
        }

        private async Task DeleteInvite(InviteLink invite)
        {
            if (!User.IsAdministrator)
            {
                return;
            }

            var result = await JsInterop.Confirm("Etes-vous sûr de vouloir retirer cette invitation ?");
            if (!result)
            {
                return;
            }

            DataService.DeleteInvite(User.OrganizationID, invite.ID);
            _invites.RemoveAll(x => x.ID == invite.ID);
            ToastService.ShowToast("Invitation retirée.");
        }

        private async Task DeleteSelectedDeviceGroup()
        {
            if (!User.IsAdministrator)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_selectedDeviceGroupId))
            {
                return;
            }

            var result = await JsInterop.Confirm("Etes-vous sûr de vouloir supprimer ce groupe ?");
            if (!result)
            {
                return;
            }

            DataService.DeleteDeviceGroup(User.OrganizationID, _selectedDeviceGroupId);
            _deviceGroups.RemoveAll(x => x.ID == _selectedDeviceGroupId);
            _selectedDeviceGroupId = string.Empty;
        }

        private async Task DeleteUser(RemotelyUser user)
        {
            if (!User.IsAdministrator)
            {
                return;
            }

            if (User.Id == user.Id)
            {
                ToastService.ShowToast("You can't delete yourself.", classString: "bg-warning");
                return;
            }

            var result = await JsInterop.Confirm("Etes-vous sûr de vouloir supprimer cet utilisateur ?");
            if (!result)
            {
                return;
            }

            await DataService.DeleteUser(User.OrganizationID, user.Id);
            _orgUsers.RemoveAll(x => x.Id == user.Id);
            ToastService.ShowToast("Utilisateur supprimé.");
        }

        private async Task EditDeviceGroups(RemotelyUser user)
        {
            void editDeviceGroupsModal(RenderTreeBuilder builder)
            {
                var deviceGroups = DataService.GetDeviceGroupsForOrganization(user.OrganizationID);

                builder.OpenComponent<EditDeviceGroup>(0);
                builder.AddAttribute(1, EditDeviceGroup.EditUserPropName, user);
                builder.AddAttribute(2, EditDeviceGroup.DeviceGroupsPropName, deviceGroups);
                builder.CloseComponent();
            }
            await ModalService.ShowModal("Groupes d'appareils", editDeviceGroupsModal);
        }

        private async Task EvaluateInviteInputKeypress(KeyboardEventArgs args)
        {
            if (args.Key.Equals("Enter", StringComparison.OrdinalIgnoreCase))
            {
                await SendInvite();
            }
        }

        private void EvaluateNewDeviceGroupKeyPress(KeyboardEventArgs args)
        {
            if (args.Key.Equals("Enter", StringComparison.OrdinalIgnoreCase))
            {
                CreateNewDeviceGroup();
            }
        }
        private void OrganizationNameChanged(ChangeEventArgs args)
        {
            if (!User.IsAdministrator)
            {
                return;
            }

            var newName = (string)args.Value;
            if (string.IsNullOrWhiteSpace(newName))
            {
                return;
            }

            if (newName.Length > 25)
            {
                ToastService.ShowToast("Maxi 25 caractères.",
                    classString: "bg-warning");
                return;
            }

            DataService.UpdateOrganizationName(_organization.ID, newName);
            _organization.OrganizationName = newName;
            ToastService.ShowToast("Le nom de l'entreprise a été changé.");
        }

        private async Task RefreshData()
        {
            _organization = await DataService.GetOrganizationByUserName(Username);

            _orgUsers.Clear();
            _invites.Clear();
            _deviceGroups.Clear();

            _invites.AddRange(DataService.GetAllInviteLinks(User.OrganizationID).OrderBy(x => x.InvitedUser));
            _deviceGroups.AddRange(DataService.GetDeviceGroups(Username).OrderBy(x => x.Name));
            var orgUsers = await DataService.GetAllUsersInOrganization(User.OrganizationID);
            _orgUsers.AddRange(orgUsers.OrderBy(x => x.UserName));
        }
        private async Task ResetPassword(RemotelyUser user)
        {
            if (!User.IsAdministrator)
            {
                return;
            }

            var code = await UserManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var resetUrl = $"{NavManager.BaseUri}Identity/Account/ResetPassword?code={code}";

            await ModalService.ShowModal("Password Reset", builder =>
            {
                builder.AddMarkupContent(0, $@"<div class=""mb-3"">Password Reset URL:</div>
                    <input readonly value=""{resetUrl}"" class=""form-control"" /> 
                    <div class=""mt-3"">NOTE: Give this URL to the user.  They must be logged out completely for it to work.</div>");
            });
        }

        private async Task SendInvite()
        {
            if (!User.IsAdministrator)
            {
                return;
            }

            if (!DataService.DoesUserExist(_inviteEmail))
            {
                var result = await DataService.CreateUser(_inviteEmail, _inviteAsAdmin, User.OrganizationID);
                if (result)
                {
                    var user = await DataService.GetUserAsync(_inviteEmail);

                    await UserManager.ConfirmEmailAsync(user, await UserManager.GenerateEmailConfirmationTokenAsync(user));

                    _orgUsers.Add(user);

                    _inviteAsAdmin = false;
                    _inviteEmail = string.Empty;
                    ToastService.ShowToast("Compte créé.");
                    return;
                }
                else
                {
                    ToastService.ShowToast("La création de l'utilisateur a échoué.", classString: "bg-danger");
                    return;
                }
            }
            else
            {

                var invite = new InviteViewModel()
                {
                    InvitedUser = _inviteEmail,
                    IsAdmin = _inviteAsAdmin
                };
                var newInvite = DataService.AddInvite(User.OrganizationID, invite);

                var inviteURL = $"{NavManager.BaseUri}Invite?id={newInvite.ID}";
                var emailResult = await EmailSender.SendEmailAsync(invite.InvitedUser, "Invitation to Organization in Remotely",
                        $@"<img src='{NavManager.BaseUri}images/Remotely_Logo.png'/>
                            <br><br>
                            Hello!
                            <br><br>
                            You've been invited to join an organization in Remotely.
                            <br><br>
                            You can join the organization by <a href='{HtmlEncoder.Default.Encode(inviteURL)}'>clicking here</a>.",
                        User.OrganizationID);
                if (emailResult)
                {
                    ToastService.ShowToast("Invitation envoyée.");
                    
                    _inviteAsAdmin = false;
                    _inviteEmail = string.Empty;
                    _invites.Add(newInvite);
                }
                else
                {
                    ToastService.ShowToast("Une erreur s'est produite.", classString: "bg-danger");
                }
            }
        }

        private void SetUserIsAdmin(ChangeEventArgs args, RemotelyUser orgUser)
        {
            if (!User.IsAdministrator)
            {
                return;
            }

            var isAdmin = (bool)args.Value;
            DataService.ChangeUserIsAdmin(User.OrganizationID, orgUser.Id, isAdmin);
            ToastService.ShowToast("Administrateur enregistré.");
        }

        private void ShowDefaultOrgHelp()
        {
            ModalService.ShowModal("Organisation par défaut", new[]
            {
                @"Option disponible uniquement pour les administrateurs du serveur.  Lorsque sélectionnée,
                cette option définit l'entreprise courante comme entreprise par défaut."
            });
        }

        private void ShowDeviceGroupHelp()
        {
            ModalService.ShowModal("Groupes d'appareils", new[]
           {
                "Les groupes permettent de restreindre l'accès à certains utilisateurs " +
                "et filtrer la page principale.",
                "Les appareils qui ne sont pas dans un groupe sont accessibles à tous."
            });
        }

        private void ShowInvitesHelp()
        {
            ModalService.ShowModal("Invitations", new[]
           {
                "Toutes les invitations en cours sont répertoriées ici et peuvent être annulées si besoin.",

                "Si l'utilisateur n'existe pas, l'envoi du mail leur permettra de créer un compte et les ajouter à l'entreprise actuelle. " +
                "Un lien de réinitialisation du mot de passe peut être envoyé depuis le tableau.",

                "La case à cocher permet de déclarer un utilisateur comme administrateur."
            });
        }

        private void ShowRelayCodeHelp()
        {
            ModalService.ShowModal("Relay Code", new[]
            {
                @"Le relay code est ajouté à la fin du nom des fichiers EXE.  Si les clients ont été compilés depuis la source et intègrent l'URL du serveur, ils utiliseront ce code pour trouver les informations de branding à utiliser."
            });
        }
        private void ShowUsersHelp()
        {
            ModalService.ShowModal("Utilisateurs", new[]
            {
                "Tous les utilisateurs sont gérés ici",
                "Les administrateurs auront accès à cet écran de contrôle aussi bien qu'aux ordinateurs."
            });
        }
    }
}
