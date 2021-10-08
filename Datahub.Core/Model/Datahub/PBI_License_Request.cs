using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Elemental.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;

namespace Datahub.Shared.EFCore
{

    public enum DataSourceProtection
    {
        Unclassified, ProtectedA, ProtectedB, Unknown
    }

    public class PBI_License_Request
    {
        [AeFormIgnore]
        [Key]
        public int Request_ID { get; set; }

        [AeLabel("Is a Premium License required? ***(Refer to Appendix II for more details on licence types)")]
        [Required]
        public bool Premium_License_Flag { get; set; }

        [StringLength(200)]
        [Required]
        [AeLabel("Contact Email")]
        public string Contact_Email { get; set; }
        
        [StringLength(200)]
        [Required]
        [AeLabel("Contact Name")]
        public string Contact_Name { get; set; }


        public Datahub_Project Project { get; set; }

        [AeFormIgnore]
        public int Project_ID { get; set; }

        public bool Desktop_Usage_Flag { get; set; }

        public List<PBI_User_License_Request> User_License_Requests { get; set; }

        [Required]
        public string User_ID { get; set; }

        [AeFormIgnore]
        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}