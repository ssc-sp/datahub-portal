using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Model.Datahub
{
    public class VersionTag
    {
        [Key]
        public int VersionTagId { get; set; }
        [Required]
        [StringLength(20)]
        public string Tag { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }
}
