using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Datahub.Core.Data;
using MudBlazor.Forms;

namespace Datahub.Finance.Data;

public class SummaryForecast
{
	[Key]
	[AeFormIgnore]
	public int Forecast_ID { get; set; }


	[AeFormIgnore]
	public FundCenter FundCenter { get; set; }


	[AeFormCategory("Fund Information")]
	[MaxLength(10)]
	[Required]
	public string Fund { get; set; }


	[AeFormIgnore]
	public int? Key_Activity { get; set; }
	[NotMapped]
	[AeFormCategory("Planned Staffing")]
	[MudForm(IsDropDown = true)]
	public DropDownContainer KA_Values { get; set; }



	[AeFormCategory("Planned Staffing")]
	[MaxLength(5000)]
	public string Key_Activity_Additional_Information { get; set; }



	[AeFormCategory("Salary Forecast")]
	[DisplayFormat(DataFormatString = "C2")]
	public double? Budget { get; set; }

	[AeFormCategory("Salary Forecast")]
	[Editable(false)]
	public double? FTE_Sum { get; set; }


	[DisplayFormat(DataFormatString = "C2")]
	[AeFormCategory("Salary Forecast")]
	[Editable(false)]
	public double? SFT_Forecast_Gross { get; set; }

	[DisplayFormat(DataFormatString = "C2")]
	[AeFormCategory("Salary Forecast")]
	[Editable(false)]
	public double? SFT_Forecast { get; set; }

	[DisplayFormat(DataFormatString = "C2")]
	[AeFormCategory("O&M Forecast")]
	public double? THC { get; set; }
	[AeFormCategory("O&M Forecast")]
	[DisplayFormat(DataFormatString = "C2")]
	public double? Other_OnM { get; set; }

	[DisplayFormat(DataFormatString = "C2")]
	[AeFormCategory("Capital Forecast")]
	public double? Personel { get; set; }
	[AeFormCategory("Capital Forecast")]
	[DisplayFormat(DataFormatString = "C2")]
	public double? Non_Personel { get; set; }

	[DisplayFormat(DataFormatString = "C2")]
	[AeFormCategory("G&C Forecast")]
	public double? Grants { get; set; }
	[AeFormCategory("G&C Forecast")]
	[DisplayFormat(DataFormatString = "C2")]
	public double? Contribution { get; set; }


	[DisplayFormat(DataFormatString = "C2")]
	[AeFormCategory("Total Forecast")]
	[Editable(false)]
	public double? Total_Forecast => (SFT_Forecast ?? 0) + (THC ?? 0) + (Other_OnM ?? 0) + (Personel ?? 0) + (Non_Personel ?? 0) + (Grants ?? 0) + (Contribution ?? 0);

	[AeFormCategory("Total Forecast")]
	public string AdditionalNotes { get; set; }


	[AeFormIgnore]
	public string Last_Updated_UserId { get; set; }

	[AeFormIgnore]
	public DateTime Last_Updated_DT { get; set; }

	[AeFormIgnore]
	public string Created_UserId { get; set; }

	[AeFormIgnore]
	public DateTime Created_DT { get; set; }

	[AeFormIgnore]
	public bool Is_Deleted { get; set; }

	[AeFormIgnore]
	[Timestamp]
	public byte[] Timestamp { get; set; }
}