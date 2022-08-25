
using MudBlazor.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Datahub.Portal.Data.Finance
{
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


        [AeFormCategory("Fund Information")]
        [MaxLength(50)]
        public string Key_Activity { get; set; }

        [AeFormCategory("Fund Information")]
        [MaxLength(100)]
        public string Key_Driver { get; set; }

        [AeFormCategory("Fund Information")]
        public double? Budget { get; set; }


        [AeFormCategory("Salary Forecast")]
        public double? SFT_Forecast { get; set; }


        [AeFormCategory("O&M Forecast")]
        public double? THC { get; set; }
        [AeFormCategory("O&M Forecast")]
        public double? Other_OnM { get; set; }


        [AeFormCategory("Capital Forecast")]
        public double? Personel { get; set; }
        [AeFormCategory("Capital Forecast")]
        public double? Non_Personel { get; set; }

        [AeFormCategory("G&C Forecast")]
        public double? Grants { get; set; }
        [AeFormCategory("G&C Forecast")]
        public double? Contribution { get; set; }

        [AeFormCategory("Total Forecast")]
        public double? Total_Forecast { get; set; }

        [AeFormCategory("Total Forecast")]
        public double? AdditionalNotes { get; set; }


        [AeFormIgnore]
        public string Last_Updated_UserId { get; set; }

        [AeFormIgnore]
        public DateTime Last_Updated_DT { get; set; }

        [AeFormIgnore]
        public string Created_UserId { get; set; }

        [AeFormIgnore]
        public DateTime Created_DT { get; set; }

        [AeFormIgnore]
        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}
