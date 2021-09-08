using Elemental.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NRCan.Datahub.Shared.EFCore
{
    public enum PBIUserLicenseType
    {
        FreeUser, ProUser
    }

    public class PBI_User_License_Request
    {
        [AeFormIgnore]
        public int ID { get; set; }

        [StringLength(200)]
        [Required]
        public string UserEmail { get; set; }

        [Required]
        [StringLength(10)]
        public string LicenseType { get; set; }

        public int RequestID { get; set; }

        public PBI_License_Request LicenseRequest { get; set; }

    }
}
