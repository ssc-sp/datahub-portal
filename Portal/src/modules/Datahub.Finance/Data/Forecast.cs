using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Datahub.Core.Data;
using MudBlazor.Forms;

namespace Datahub.Finance.Data;

public class Forecast
{
    [Key]
    [AeFormIgnore]
    public int ForecastID { get; set; }

    [AeFormIgnore]
    public FundCenter FundCenter { get; set; }

    [AeFormCategory("Salary Data")]
    [MaxLength(50)]
    [Required]
    public string EmployeePlannedStaffing { get; set; }

    [AeFormCategory("Salary Data")]
    public int? EmployeePositionNumber { get; set; }

    [AeFormCategory("Salary Data")]
    [MaxLength(400)]
    public string EmployeeLastName { get; set; }

    [AeFormCategory("Salary Data")]
    [MaxLength(400)]
    public string EmployeeFirstName { get; set; }

    [AeFormCategory("Salary Data")]
    public bool IsIndeterminate { get; set; }

    [AeFormCategory("Salary Data")]
    [MaxLength(20)]
    public string Classification { get; set; }


    [AeFormCategory("Salary Data")]
    [MaxLength(20)]
    public string Fund { get; set; }

    [AeFormCategory("Salary Data")]
    public DateTime StartDate { get; set; }


    [AeFormCategory("Salary Data")]
    public DateTime EndDate { get; set; }

    [AeFormCategory("Salary Data")]
    [Range(0, 1, ErrorMessage = "Only a number between 0 and 1 is allowed")]
    public double? FTE { get; set; }

    [AeFormCategory("Salary Data")]
    [DisplayFormat(DataFormatString = "C2")]
    public double? Salary { get; set; }

    [AeFormIgnore]
    public int? IncrementalReplacement { get; set; }

    [NotMapped]
    [AeFormCategory("Planned Staffing")]
    [MudForm(IsDropDown = true)]
    public DropDownContainer IRValues { get; set; }


    [AeFormCategory("Planned Staffing")]
    [MaxLength(4000)]
    [AeFormIgnore]
    public string LocationOfHiring { get; set; }

    [AeFormIgnore]
    public int? PotentialHiringProcess { get; set; }

    [NotMapped]
    [AeFormCategory("Planned Staffing")]
    [MudForm(IsDropDown = true)]
    public DropDownContainer PHPValues { get; set; }


    [AeFormIgnore]
    public int? FTEAccomodationsRequirements { get; set; }

    [NotMapped]
    [AeFormCategory("Planned Staffing")]
    [MudForm(IsDropDown = true)]
    public DropDownContainer FTEAccomodationsReqValues { get; set; }

    [AeFormIgnore]
    public int? FTEAccomodationsLocation { get; set; }

    [NotMapped]
    [AeFormCategory("Planned Staffing")]
    [MudForm(IsDropDown = true)]
    public DropDownContainer FTEAccomodationsLocationValues { get; set; }

    [AeFormCategory("Planned Staffing")]
    [MaxLength(500)]
    public string OtherLocations { get; set; }

    [AeFormIgnore]
    public int? PositionWorkspaceType { get; set; }

    [NotMapped]
    [AeFormCategory("Planned Staffing")]
    [MudForm(IsDropDown = true)]
    public DropDownContainer PositionWorkspaceTypeValues { get; set; }

    [AeFormIgnore]
    public string LastUpdatedUserId { get; set; }

    [AeFormIgnore]
    public DateTime LastUpdatedDT { get; set; }

    [AeFormIgnore]
    public string CreatedUserId { get; set; }

    [AeFormIgnore]
    public DateTime CreatedDT { get; set; }

    [AeFormIgnore]
    public bool IsDeleted { get; set; }

    [AeFormIgnore]
    [Timestamp]
    public byte[] Timestamp { get; set; }
}