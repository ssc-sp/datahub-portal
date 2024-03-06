using System.ComponentModel.DataAnnotations;

namespace Datahub.PIP.Data;

public class PIPIndicatorRisks
{
    [Key]
    public int IndicatorRiskID { get; set; }
    public PIPIndicatorAndResults PipIndicator { get; set; }
    public PIPRisks PipRisk { get; set; }
}