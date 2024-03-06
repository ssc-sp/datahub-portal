using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Datahub.Core.Data;
using MudBlazor.Forms;

namespace Datahub.Finance.Data;

public class SummaryForecast
{
    [Key]
    [AeFormIgnore]
    public int ForecastID { get; set; }


    [AeFormIgnore]
    public FundCenter FundCenter { get; set; }


    [AeFormCategory("Fund Information")]
    [MaxLength(10)]
    [Required]
    public string Fund { get; set; }


    [AeFormIgnore]
    public int? KeyActivity { get; set; }
    [NotMapped]
    [AeFormCategory("Planned Staffing")]
    [MudForm(IsDropDown = true)]
    public DropDownContainer KAValues { get; set; }



    [AeFormCategory("Planned Staffing")]
    [MaxLength(5000)]
    public string KeyActivityAdditionalInformation { get; set; }



    [AeFormCategory("Salary Forecast")]
    [DisplayFormat(DataFormatString = "C2")]
    public double? Budget { get; set; }

    [AeFormCategory("Salary Forecast")]
    [Editable(false)]
    public double? FTESum { get; set; }


    [DisplayFormat(DataFormatString = "C2")]
    [AeFormCategory("Salary Forecast")]
    [Editable(false)]
    public double? SFTForecastGross { get; set; }

    [DisplayFormat(DataFormatString = "C2")]
    [AeFormCategory("Salary Forecast")]
    [Editable(false)]
    public double? SFTForecast { get; set; }

    [DisplayFormat(DataFormatString = "C2")]
    [AeFormCategory("O&M Forecast")]
    public double? THC { get; set; }
    [AeFormCategory("O&M Forecast")]
    [DisplayFormat(DataFormatString = "C2")]
    public double? OtherOnM { get; set; }

    [DisplayFormat(DataFormatString = "C2")]
    [AeFormCategory("Capital Forecast")]
    public double? Personel { get; set; }
    [AeFormCategory("Capital Forecast")]
    [DisplayFormat(DataFormatString = "C2")]
    public double? NonPersonel { get; set; }

    [DisplayFormat(DataFormatString = "C2")]
    [AeFormCategory("G&C Forecast")]
    public double? Grants { get; set; }
    [AeFormCategory("G&C Forecast")]
    [DisplayFormat(DataFormatString = "C2")]
    public double? Contribution { get; set; }


    [DisplayFormat(DataFormatString = "C2")]
    [AeFormCategory("Total Forecast")]
    [Editable(false)]
    public double? TotalForecast => (SFTForecast ?? 0) + (THC ?? 0) + (OtherOnM ?? 0) + (Personel ?? 0) + (NonPersonel ?? 0) + (Grants ?? 0) + (Contribution ?? 0);

    [AeFormCategory("Total Forecast")]
    public string AdditionalNotes { get; set; }


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