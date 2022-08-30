
using MudBlazor.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Datahub.Portal.Data.Finance
{
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
        public double? FTE { get; set; }

        [AeFormCategory("Salary Data")]
        public double? Salary { get; set; }

        [AeFormCategory("Planned Staffing")]
        [MaxLength(4000)]        
        public string Incremental_Replacement { get; set; }

        [AeFormCategory("Planned Staffing")]
        [MaxLength(4000)]
        public string Location_Of_Hiring { get; set; }

        [AeFormCategory("Planned Staffing")]
        [MaxLength(4000)]
        public string Potential_Hiring_Process { get; set; }

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
