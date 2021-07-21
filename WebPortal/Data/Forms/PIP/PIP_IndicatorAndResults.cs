using Elemental.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NRCan.Datahub.ProjectForms.Data.PIP
{
    public class PIP_IndicatorAndResults
    {
        [Key]
        [AeFormIgnore]
        public int IndicatorAndResult_ID { get; set; }

        [AeLabel(row: "1", column: "1", isDropDown: true, placeholder: "Please Select")] 
        [MaxLength(400)] 
        public string Outcome_Level_DESC { get; set; }

        [AeLabel(row: "1", column: "2")]
        [MaxLength(1000)] 
        public string Program_Output_Or_Outcome_DESC { get; set; }

        [AeLabel(row: "3", column: "1")]
        [MaxLength(4000)] 
        public string Indicator_DESC { get; set; }

        [AeLabel(row: "4", column: "1", isDropDown: true, placeholder: "Please Select")]
        [MaxLength(1000)]
        public string Source_Of_Indicator_DESC { get; set; }

        [AeLabel(row: "4", column: "2", isDropDown: true, placeholder: "Please Select")]
        [MaxLength(1000)]
        public string Source_Of_Indicator2_DESC { get; set; }
        
        [AeLabel(row: "4", column: "3", isDropDown: true, placeholder: "Please Select")]
        [MaxLength(1000)]
        public string Source_Of_Indicator3_DESC { get; set; }

        [AeLabel(row: "7", column: "1", isDropDown: true, placeholder: "Please Select")]
        [MaxLength(50)]
        public string Can_Report_On_Indicator { get; set; }
        [AeLabel(row: "7", column: "2", isDropDown: true, placeholder: "Please Select")]
        [MaxLength(100)]
        public string Cannot_Report_On_Indicator { get; set; }

        [AeLabel(row: "8", column: "1")]
        [MaxLength(4000)]
        public string DRF_Indicator_No { get; set; }
        [AeLabel(row: "8", column: "2")]
        [MaxLength(1000)] 
        public string Tb_Sub_Indicator_Identification_Number_ID { get; set; }

        [AeLabel(row: "9", column: "1")]
        [MaxLength(1000)]
        public string Branch_Optional_DESC { get; set; }
        [AeLabel(row: "9", column: "2")]
        [MaxLength(1000)]
        public string Sub_Program { get; set; }

        [AeLabel(row: "10", column: "1", isDropDown: true, placeholder: "Please Select")] 
        [MaxLength(1000)] 
        public string Indicator_Category_DESC { get; set; }
        [AeLabel(row: "10", column: "2", isDropDown: true, placeholder: "Please Select")] [MaxLength(1000)] 
        public string Indicator_Direction_DESC { get; set; }
        [AeLabel(row: "10", column: "3", isDropDown: true, placeholder: "Please Select")] [MaxLength(1000)] 
        public string Indicator__Progressive_Or_Aggregate_DESC { get; set; }
        
        [AeLabel(row: "11", column: "1", placeholder: "How it relates to the output or outcome; why is it relevant?")] [MaxLength(4000)] 
        [AeFormCategory("Methodology", 10)]
        public string Indicator_Rationale_DESC { get; set; }

        [AeLabel(row: "12", column: "1")]
        [AeFormCategory("Methodology", 10)]
        [MaxLength(2000)] 
        public string Indicator_Calculation_Formula_NUM { get; set; }

        [AeLabel(row: "13", column: "1")]
        [AeFormCategory("Methodology", 10)]
        [MaxLength(4000)]
        public string Measurement_Strategy { get; set; }

        [AeLabel(row: "14", column: "1")]
        [AeFormCategory("Methodology", 10)]
        [MaxLength(1000)] 
        public string Baseline_DESC { get; set; }
        [AeLabel(row: "14", column: "2")]
        [AeFormCategory("Methodology", 10)]
        [MaxLength(100)]
        public string Date_Of_Baseline_DT { get; set; }

        [AeLabel(row: "15", column: "1")]
        [AeFormCategory("Methodology", 10)]
        [MaxLength(4000)]
        public string Notes_Definitions { get; set; }

        [AeLabel(row: "16", column: "1")]
        [AeFormCategory("Methodology", 10)]
        [MaxLength(1000)]         
        public string Data_Source_DESC { get; set; }
        [AeLabel(row: "16", column: "2")]
        [AeFormCategory("Methodology", 10)]
        [MaxLength(1000)] 
        public string Data_Owner_NAME { get; set; }         


        [AeLabel(row: "17", column: "1", isDropDown: true, placeholder: "Please Select")] [MaxLength(1000)]
        [AeFormCategory("Methodology", 10)]
        public string Frequency_DESC { get; set; }
        [AeLabel(row: "17", column: "2", isDropDown: true, placeholder: "Please Select")] [MaxLength(1000)]
        [AeFormCategory("Methodology", 10)]
        public string Data_Type_DESC { get; set; }

        [AeLabel(row: "18", column: "1", placeholder: "Additional definitions/information necessary to understand how the data will be collected for this indicator; Sectors may also insert a link to a methodology sheet in this column")]
        [MaxLength(8000)]
        [AeFormCategory("Methodology", 10)]
        public string Methodology_How_Will_The_Indicator_Be_Measured { get; set; }

        [AeLabel(row: "19", column: "1", isDropDown: true, placeholder: "Please Select")] [MaxLength(1000)] 
        [AeFormCategory("Target", 20)]
        public string Target_Type_DESC { get; set; }
        [AeLabel(row: "19", column: "2")]
        [AeFormCategory("Target", 20)]
        [MaxLength(4000)] 
        public string Target_202021_DESC { get; set; }
        [AeLabel(row: "19", column: "3")]
        [AeFormCategory("Target", 20)]
        public DateTime? Date_To_Achieve_Target_DT { get; set; }


        [AeLabel(row: "20", column: "1")]
        [AeFormCategory("Actual Results", 30)]
        [MaxLength(8000)]
        public string Result_202021_DESC { get; set; }
        [AeLabel(row: "20", column: "2")]
        [AeFormCategory("Actual Results", 30)]
        public DateTime? Date_201920_Result_Collected_DT { get; set; }
        [AeLabel(row: "20", column: "3", isDropDown: true, placeholder: "Please Select")] [MaxLength(1000)] 
        [AeFormCategory("Actual Results", 30)]
        public string Target_Met { get; set; }

        
        [AeLabel(row: "21", column: "1", placeholder: "Please provide additional context for the results achieved. This is required for targets not been met, to be achieved in the future, or if the result is significantly higher than the target. There is a character limit of 1000 for this cell in order to meet GC InfoBase character limit.")] [MaxLength(8000)]
        [AeFormCategory("Actual Results", 30)]
        public string Explanation { get; set; }

        [AeLabel(row: "22", column: "1")]
        [AeFormCategory("Actual Results", 30)]
        [MaxLength(1000)] 
        public string Result_201920_DESC { get; set; }
        [AeLabel(row: "22", column: "2")]
        [AeFormCategory("Actual Results", 30)]
        [MaxLength(1000)] 
        public string Result_201819_DESC { get; set; }
        [AeLabel(row: "22", column: "3")]
        [AeFormCategory("Actual Results", 30)]
        [MaxLength(1000)] 
        public string Result_201718_DESC { get; set; }


        




        [AeLabel(row: "28", column: "1")]
        [AeFormCategory("Latest Update Information", 60)]
        [Editable(false)] public string Last_Updated_UserId { get; set; }
        [AeLabel(row: "28", column: "2")]
        [AeFormCategory("Latest Update Information", 60)]
        [Required] public DateTime Date_Updated_DT { get; set; }


        public PIP_Tombstone PIP_Tombstone { get; set; }
        [AeFormIgnore]
        public DateTime Last_Updated_DT { get; set; }

        [AeFormIgnore]
        [Timestamp]
        public byte[] Timestamp { get; set; }

        


    }
}
