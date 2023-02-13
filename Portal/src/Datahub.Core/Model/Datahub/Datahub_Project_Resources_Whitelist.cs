using System;
using System.ComponentModel.DataAnnotations;
// ReSharper disable InconsistentNaming

namespace Datahub.Core.Model.Datahub
{
    public class Datahub_Project_Resources_Whitelist
    {
        [Key]
        public int Id { get; set; }
        public int ProjectId { get; set; }

        public string AdminLastUpdated_ID { get; set; }
        public string AdminLastUpdated_UserName { get; set; }
        
        public DateTime LastUpdated { get; set; }
        
        public bool AllowStorage { get; set; }
        public bool AllowDatabricks { get; set; }
        public bool AllowVMs { get; set; }
        
        public Datahub_Project Project { get; set; }
        
        public bool IsEqual(Datahub_Project_Resources_Whitelist other)
        {
            return other != null &&
                   ProjectId == other.ProjectId &&
                   AllowStorage == other.AllowStorage &&
                   AllowDatabricks == other.AllowDatabricks &&
                   AllowVMs == other.AllowVMs;
        }
    }
}