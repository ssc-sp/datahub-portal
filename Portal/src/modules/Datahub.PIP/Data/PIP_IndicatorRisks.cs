using System.ComponentModel.DataAnnotations;

namespace Datahub.PIP.Data;

public class PIP_IndicatorRisks
{
	[Key]
	public int IndicatorRisk_ID { get; set; }
	public PIP_IndicatorAndResults Pip_Indicator { get; set; }
	public PIP_Risks Pip_Risk { get; set; }
}