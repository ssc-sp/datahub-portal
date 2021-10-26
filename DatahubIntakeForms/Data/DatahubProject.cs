using System.ComponentModel.DataAnnotations;
using Elemental.Components.Forms;
using Microsoft.AspNetCore.Mvc;

namespace Datahub.Portal.Data
{
    public class DatahubProject
    {
        [AeFormIgnore]
        [Key]
        public int ProjectID { get; set; }

        [StringLength(200)]
        [AeLabel("Description")]
        [Required]
        public string Description { get; set; }

        [StringLength(200)]
        [Required]
        [AeLabel("Azure Tags")]

        public string AzureTags { get; set; }

        [StringLength(200)]
        [Required]
        [AeLabel("Contact Sector")]

        public string Sector { get; set; }

        [StringLength(200)]
        [Required]
        [AeLabel("Contact Branch")]

        public string Branch { get; set; }

        [StringLength(200)]
        [Required]
        [AeLabel("Contact Email")]
        public string ContactEmail { get; set; }
        
        [StringLength(200)]
        [Required]
        [AeLabel("Contact Name")]

        public string ContactName { get; set; }

        [AeFormIgnore]

        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}