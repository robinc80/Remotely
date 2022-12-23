using Remotely.Shared.Attributes;
using Remotely.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace Remotely.Shared.Models
{
    public class Device
    {
		public static Device Empty { get; } = new();
		
        [Sortable]
        [Display(Name = "Version de l'agent")]
        public string AgentVersion { get; set; }

        public ICollection<Alert> Alerts { get; set; }
        [StringLength(100)]

        [Sortable]
        [Display(Name = "Alias")]
        public string Alias { get; set; }

        [Sortable]
        [Display(Name = "Utilisation du CPU")]
        public double CpuUtilization { get; set; }

        [Sortable]
        [Display(Name = "Utilisateur courant")]
        public string CurrentUser { get; set; }

        public DeviceGroup DeviceGroup { get; set; }
        public string DeviceGroupID { get; set; }

        [Sortable]
        [Display(Name = "Nom de l'appareil")]
        public string DeviceName { get; set; }
        public List<Drive> Drives { get; set; }

        [Key]
        public string ID { get; set; }

        public bool Is64Bit { get; set; }
        public bool IsOnline { get; set; }

        [Sortable]
        [Display(Name = "Dernière connexion")]
        public DateTimeOffset LastOnline { get; set; }

        [StringLength(5000)]
        public string Notes { get; set; }       

        [JsonIgnore]
        public Organization Organization { get; set; }

        public string OrganizationID { get; set; }
        public Architecture OSArchitecture { get; set; }

        [Sortable]
        [Display(Name = "OS")]
        public string OSDescription { get; set; }

        [Sortable]
        [Display(Name = "Plateforme")]
        public string Platform { get; set; }

        [Sortable]
        [Display(Name = "Processor Count")]
        public int ProcessorCount { get; set; }

        public string PublicIP { get; set; }
        public string ServerVerificationToken { get; set; }

        [JsonIgnore]
        public List<ScriptResult> ScriptResults { get; set; }

        [JsonIgnore]
        public List<ScriptRun> ScriptRuns { get; set; }
        [JsonIgnore]
        public List<ScriptRun> ScriptRunsCompleted { get; set; }

        [JsonIgnore]
        public List<ScriptSchedule> ScriptSchedules { get; set; }

        [StringLength(200)]
        [Sortable]
        [Display(Name = "Tags")]
        public string Tags { get; set; } = "";

        [Sortable]
        [Display(Name = "RAM totale")]
        public double TotalMemory { get; set; }

        [Sortable]
        [Display(Name = "Stockage total")]
        public double TotalStorage { get; set; }

        [Sortable]
        [Display(Name = "RAM utilisée")]
        public double UsedMemory { get; set; }

        [Sortable]
        [Display(Name = "RAM utilisée (%)")]
        public double UsedMemoryPercent => UsedMemory / TotalMemory;

        [Sortable]
        [Display(Name = "Stockage utilisé")]
        public double UsedStorage { get; set; }

        [Sortable]
        [Display(Name = "Stockage utilisé (%)")]
        public double UsedStoragePercent => UsedStorage / TotalStorage;
    }
}