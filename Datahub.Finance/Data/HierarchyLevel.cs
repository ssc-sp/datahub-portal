using Elemental.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Datahub.Portal.Data.Finance
{
    public class HierarchyLevel
    {
        [Key]
        [AeFormIgnore]
        public int HierarchyLevelID { get; set; }
        [Required]
        [MaxLength(20)]
        public string FCCode { get; set; }
        [Required]
        [MaxLength(20)] 
        public string ParentCode { get; set; }
        public string FundCenterNameEnglish { get; set; }
        public string FundCenterNameFrench { get; set; }
        
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }

        
    }
}
