using System.ComponentModel.DataAnnotations;

namespace Datahub.PIP.Data;

public class PIPTombstoneRisks
{
    [Key]
    public int TombstoneRiskID { get; set; }
    public PIPTombstone PipTombstone { get; set; }
    public PIPRisks PipRisk { get; set; }
}