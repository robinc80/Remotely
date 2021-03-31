using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remotely.Shared.Models
{
    public class DesktopAppConfig
    {
private string _host = "https://sos.pcenpanne.fr";

        public string Host
        {
            get => _host;
            set
            {
                _host = value?.TrimEnd('/');
            }
        }
        public string OrganizationId { get; set; } = "";
    }
}
