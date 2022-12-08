using Elemental.Components;
using System;
using System.ComponentModel.DataAnnotations;

namespace Datahub.Core.EFCore
{
    public class Datahub_ProjectServiceRequests
    {
        // TODO add requesting user to data model

        [Key]
        [AeFormIgnore]

        public int ServiceRequests_ID { get; set; }

        public DateTime ServiceRequests_Date_DT { get; set; }

        public string ServiceType { get; set; }
        public DateTime? Is_Completed { get; set; }
        
        [StringLength(200)]
        public string User_Name { get; set; }

        [StringLength(200)]
        public string User_ID { get; set; }

        public DateTime? Notification_Sent { get; set; }

        [AeFormIgnore]
        [Timestamp]
        public byte[] Timestamp { get; set; }

        #nullable enable
        public Project_Storage? Project_Storage { get; set; }
        #nullable disable

        public Datahub_Project Project { get; set; }
    }
}
