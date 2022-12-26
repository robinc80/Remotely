using Avalonia.Controls;
using Immense.RemoteControl.Desktop.UI.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Immense.RemoteControl.Desktop.Shared.Abstractions;
using Microsoft.Extensions.Logging;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Immense.RemoteControl.Desktop.UI.ViewModels
{
    public interface IPromptForAccessWindowViewModel
    {
        ICommand CloseCommand { get; }
        ICommand MinimizeCommand { get; }
        string OrganizationName { get; set; }
        bool PromptResult { get; set; }
        string RequesterName { get; set; }
        string RequestMessage { get; }
        ICommand SetResultNo { get; }
        ICommand SetResultYes { get; }
    }

    public class PromptForAccessWindowViewModel : BrandedViewModelBase, IPromptForAccessWindowViewModel
    {
        public PromptForAccessWindowViewModel(
            string requesterName,
            string organizationName,
            IBrandingProvider brandingProvider,
            IAvaloniaDispatcher dispatcher,
            ILogger<BrandedViewModelBase> logger)
            : base(brandingProvider, dispatcher, logger)
        {
            if (!string.IsNullOrWhiteSpace(requesterName))
            {
                RequesterName = requesterName;
            }

            if (!string.IsNullOrWhiteSpace(organizationName))
            {
                OrganizationName = organizationName;
            }
        }

        public ICommand CloseCommand { get; } = new RelayCommand<Window>(window =>
        {
            window?.Close();
        });

        public ICommand MinimizeCommand { get; } = new RelayCommand<Window>(window =>
        {
            if (window is not null)
            {
                window.WindowState = WindowState.Minimized;
            }
        });

        public string OrganizationName
        {
            get => Get<string>() ?? "Le garage à PC";
            set
            {
                Set(value);
                OnPropertyChanged(nameof(RequestMessage));
            }

        }

        public bool PromptResult { get; set; }

        public string RequesterName
        {
            get => Get<string>() ?? "un technicien";
            set
            {
                Set(value);
                OnPropertyChanged(nameof(RequestMessage));
            }
        }

        public string RequestMessage
        {
            get
            {
                return $"Voulez-vous permettre {RequesterName} de contrôler votre ordinateur ?";
            }
        }
        public ICommand SetResultNo => new RelayCommand<Window>(window =>
        {
            PromptResult = false;
            if (window is not null)
            {
                window.Close();
            }
        });

        public ICommand SetResultYes => new RelayCommand<Window>(window =>
        {
            PromptResult = true;
            if (window is not null)
            {
                window.Close();
            }
        });
    }
}
