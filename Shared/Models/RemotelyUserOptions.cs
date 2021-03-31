using Remotely.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Remotely.Shared.Models
{
    public class RemotelyUserOptions
    {
        [Display(Name = "Console Prompt")]
        [StringLength(5)]
        public string ConsolePrompt { get; set; } = "~>";

        [Display(Name = "Raccourci Web")]
        [StringLength(10)]
        public string CommandModeShortcutWeb { get; set; } = "/web";
        [Display(Name = "PS Core")]
        [StringLength(10)]
        public string CommandModeShortcutPSCore { get; set; } = "/pscore";
        [Display(Name = "Windows PS")]
        [StringLength(10)]
        public string CommandModeShortcutWinPS { get; set; } = "/winps";
        [Display(Name = "Raccourci CMD")]
        [StringLength(10)]
        public string CommandModeShortcutCMD { get; set; } = "/cmd";
        [Display(Name = "Raccourci Bash")]
        [StringLength(10)]
        public string CommandModeShortcutBash { get; set; } = "/bash";

        [Display(Name = "Thème")]
        public Theme Theme { get; set; }
    }
}
