using System.ComponentModel.DataAnnotations;
using Datahub.Core.Model.Projects;

namespace Datahub.Core.Model.Datahub;

public class Organization_Level
{
    [Key]
    public int SectorAndBranchS_ID { get; set; }

    public int Organization_ID { get; set; }

    [StringLength(4000)]
    public string Full_Acronym_E { get; set; }
    [StringLength(4000)]
    public string Full_Acronym_F { get; set; }
    [StringLength(4000)]
    public string Org_Acronym_E { get; set; }
    [StringLength(4000)]
    public string Org_Acronym_F { get; set; }
    [StringLength(4000)]
    public string Org_Name_E { get; set; }
    [StringLength(4000)]
    public string Org_Name_F { get; set; }
    [StringLength(1)]
    public string Org_Level { get; set; }
    public int? Superior_OrgId { get; set; }

    //[ForeignKey("SectorId")]
    public List<Datahub_Project> Sectors { get; set; }
    //[ForeignKey("BranchId")]
    public List<Datahub_Project> Branches { get; set; }
    //[ForeignKey("DivisionId")]
    public List<Datahub_Project> Divisions { get; set; }
}