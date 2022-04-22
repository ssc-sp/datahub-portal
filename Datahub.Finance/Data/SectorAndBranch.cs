using Elemental.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Datahub.Portal.Data.Finance
{
    public class SectorAndBranch
    {
        [Key]
        [AeFormIgnore]
        public int SectorBranch_ID { get; set; }

        [AeLabel(isDropDown: true)] [Required] public string Sector_TXT { get; set; }
        [AeLabel(isDropDown: true)] [Required] public string Branch_TXT { get; set; }
        [Column(TypeName = "Money")] public double? Branch_Budget_NUM { get; set; }
        [Column(TypeName = "Money")] public double? Allocated_Budget_NUM { get; set; }
        [Column(TypeName = "Money")] public double? Unallocated_Budget_NUM { get; set; }

        public Sector Sector { get; set; }
        public Branch Branch { get; set; }

        [AeFormIgnore]
        public string Last_Updated_UserId { get; set; }

        [AeFormIgnore]
        public DateTime Last_Updated_DT { get; set; }

        [AeFormIgnore]
        [Timestamp]
        public byte[] Timestamp { get; set; }
    }

    public class Sector
    { 
        [Key]
        public int SectorId { get; set; }        
        public string SectorNameEn { get; set; }
        public string SectorNameFr { get; set; }
        
        public int SectorAndBranchForSectorId { get; set; }
        public List<SectorAndBranch> SectorAndBranch { get; set; }
        public List<Branch> Branches { get; set; }
    }

    public class Branch
    {
        [Key]
        public int BranchId { get; set; }
        public string BranchNameEn { get; set; }
        public string BranchNameFr { get; set; }

        public int SectorId { get; set; }
        public int SectorAndBranchForBranchId{ get; set; }
        public List<SectorAndBranch> SectorAndBranch { get; set; }
        public Sector Sector { get; set; }
    }


}
