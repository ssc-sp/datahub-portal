using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Datahub.Core.Data;
using MudBlazor.Forms;

namespace Datahub.Finance.Data;

public class Forecast
{
	[Key]
	[AeFormIgnore]
	public int Forecast_ID { get; set; }

	[AeFormIgnore]
	public FundCenter FundCenter { get; set; }

	[AeFormCategory("Salary Data")]
	[MaxLength(50)]
	[Required]
	public string Employee_Planned_Staffing { get; set; }

	[AeFormCategory("Salary Data")]
	public int? Employee_Position_Number { get; set; }

	[AeFormCategory("Salary Data")]
	[MaxLength(400)]
	public string Employee_Last_Name { get; set; }

	[AeFormCategory("Salary Data")]
	[MaxLength(400)]
	public string Employee_First_Name { get; set; }

	[AeFormCategory("Salary Data")]
	public bool Is_Indeterminate { get; set; }

	[AeFormCategory("Salary Data")]
	[MaxLength(20)]
	public string Classification { get; set; }


	[AeFormCategory("Salary Data")]
	[MaxLength(20)]
	public string Fund { get; set; }

	[AeFormCategory("Salary Data")]
	public DateTime Start_Date { get; set; }


	[AeFormCategory("Salary Data")]
	public DateTime End_Date { get; set; }

	[AeFormCategory("Salary Data")]
	[Range(0, 1, ErrorMessage = "Only a number between 0 and 1 is allowed")]
	public double? FTE { get; set; }

	[AeFormCategory("Salary Data")]
	[DisplayFormat(DataFormatString = "C2")]
	public double? Salary { get; set; }

	[AeFormIgnore]
	public int? Incremental_Replacement { get; set; }

	[NotMapped]
	[AeFormCategory("Planned Staffing")]
	[MudForm(IsDropDown = true)]
	public DropDownContainer IR_Values { get; set; }


	[AeFormCategory("Planned Staffing")]
	[MaxLength(4000)]
	[AeFormIgnore]
	public string Location_Of_Hiring { get; set; }

	[AeFormIgnore]
	public int? Potential_Hiring_Process { get; set; }

	[NotMapped]
	[AeFormCategory("Planned Staffing")]
	[MudForm(IsDropDown = true)]
	public DropDownContainer PHP_Values { get; set; }


	[AeFormIgnore]
	public int? FTE_Accomodations_Requirements { get; set; }

	[NotMapped]
	[AeFormCategory("Planned Staffing")]
	[MudForm(IsDropDown = true)]
	public DropDownContainer FTE_Accomodations_Req_Values { get; set; }

	[AeFormIgnore]
	public int? FTE_Accomodations_Location { get; set; }

	[NotMapped]
	[AeFormCategory("Planned Staffing")]
	[MudForm(IsDropDown = true)]
	public DropDownContainer FTE_Accomodations_Location_Values { get; set; }

	[AeFormCategory("Planned Staffing")]
	[MaxLength(500)]
	public string Other_Locations { get; set; }

	[AeFormIgnore]
	public int? Position_Workspace_Type { get; set; }

	[NotMapped]
	[AeFormCategory("Planned Staffing")]
	[MudForm(IsDropDown = true)]
	public DropDownContainer Position_Workspace_Type_Values { get; set; }

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