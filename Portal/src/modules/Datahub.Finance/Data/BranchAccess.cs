using System.ComponentModel.DataAnnotations;

namespace Datahub.Finance.Data;

public class BranchAccess
{
    [Key]
    public int BranchAccessID { get; set; }
    [MaxLength(20)]
    public string BranchFundCenter { get; set; }
    [MaxLength(100)]
    public string User { get; set; }
    public bool IsInactive { get; set; }


}