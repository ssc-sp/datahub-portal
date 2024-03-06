using System.ComponentModel.DataAnnotations;
using Datahub.Core.Model.Projects;

namespace Datahub.Core.Model.Datahub;

public class OrganizationLevel
{
    [Key]
    public int SectorAndBranchSID { get; set; }

    public int OrganizationID { get; set; }

    [StringLength(4000)]
    public string FullAcronymE { get; set; }
    [StringLength(4000)]
    public string FullAcronymF { get; set; }
    [StringLength(4000)]
    public string OrgAcronymE { get; set; }
    [StringLength(4000)]
    public string OrgAcronymF { get; set; }
    [StringLength(4000)]
    public string OrgNameE { get; set; }
    [StringLength(4000)]
    public string OrgNameF { get; set; }
    [StringLength(1)]
    public string OrgLevel { get; set; }
    public int? SuperiorOrgId { get; set; }

    //[ForeignKey("SectorId")]
    public List<DatahubProject> Sectors { get; set; }
    //[ForeignKey("BranchId")]
    public List<DatahubProject> Branches { get; set; }
    //[ForeignKey("DivisionId")]
    public List<DatahubProject> Divisions { get; set; }
}