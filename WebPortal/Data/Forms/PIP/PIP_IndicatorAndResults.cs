using Elemental.Components.Forms;
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

        [AeLabel(isDropDown: true, placeholder: "Please Select")] 
        [MaxLength(400)] 
        public string? Outcome_Level_DESC { get; set; }
        [MaxLength(1000)] 
        public string? Program_Output_Or_Outcome_DESC { get; set; }
        [MaxLength(4000)] 
        public string? Indicator_DESC { get; set; }
        [AeLabel(isDropDown: true, placeholder: "Please Select")] 
        [MaxLength(1000)] 
        public string? Source_Of_Indicator_DESC { get; set; }
        [AeLabel(isDropDown: true, placeholder: "Please Select")]
        [MaxLength(1000)]
        public string? Source_Of_Indicator2_DESC { get; set; }
        [AeLabel(isDropDown: true, placeholder: "Please Select")]
        [MaxLength(1000)]
        public string? Source_Of_Indicator3_DESC { get; set; }
        [MaxLength(1000)] 
        public string? Tb_Sub_Indicator_Identification_Number_ID { get; set; }
        [AeLabel(isDropDown: true, placeholder: "Please Select")] 
        [MaxLength(1000)] 
        public string? Indicator_Category_DESC { get; set; }
        [AeLabel(isDropDown: true, placeholder: "Please Select")] [MaxLength(1000)] 
        public string? Indicator_Direction_DESC { get; set; }
        [AeLabel(isDropDown: true, placeholder: "Please Select")] [MaxLength(1000)] 
        public string? Indicator__Progressive_Or_Aggregate_DESC { get; set; }
        [AeLabel(placeholder: "How it relates to the output or outcome; why is it relevant?")] [MaxLength(4000)] 
        public string? Indicator_Rationale_DESC { get; set; }
        [MaxLength(2000)] 
        public string? Indicator_Calculation_Formula_NUM { get; set; }
        [MaxLength(4000)]
        public string? Measurement_Strategy { get; set; }
        [MaxLength(1000)] 
        public string? Baseline_DESC { get; set; }         
        [MaxLength(100)]
        public string? Date_Of_Baseline_DT { get; set; }
        [MaxLength(4000)]
        public string? Notes_Definitions { get; set; }        
        [MaxLength(1000)] 
        public string? Data_Owner_NAME { get; set; }
        [MaxLength(1000)] 
        public string? Branch_Optional_DESC { get; set; }
        [MaxLength(1000)]
        public string? Sub_Program { get; set; }
        [MaxLength(1000)]         
        public string? Data_Source_DESC { get; set; }
        [AeLabel(isDropDown: true, placeholder: "Please Select")] [MaxLength(1000)] 
        public string? Frequency_DESC { get; set; }
        [AeLabel(isDropDown: true, placeholder: "Please Select")] [MaxLength(1000)] 
        public string? Data_Type_DESC { get; set; }
        [AeLabel(isDropDown: true, placeholder: "Please Select")] [MaxLength(1000)] 
        public string? Target_Type_DESC { get; set; }
        [MaxLength(4000)] 
        public string? Target_202021_DESC { get; set; }         
        public DateTime? Date_To_Achieve_Target_DT { get; set; }
        [AeLabel(isDropDown: true, placeholder: "Please Select")] [MaxLength(1000)] 
        public string? Target_Met { get; set; }        
        public DateTime? Date_201920_Result_Collected_DT { get; set; }
        [MaxLength(8000)]
        public string? Result_202021_DESC { get; set; }
        [AeLabel(placeholder: "Please provide additional context for the results achieved. This is required for targets not been met, to be achieved in the future, or if the result is significantly higher than the target. There is a character limit of 1000 for this cell in order to meet GC InfoBase character limit.")] [MaxLength(8000)]
        public string? Explanation { get; set; }        
        [MaxLength(1000)] 
        public string? Result_201920_DESC { get; set; }
        [MaxLength(1000)] 
        public string? Result_201819_DESC { get; set; }
        [MaxLength(1000)] 
        public string? Result_201718_DESC { get; set; }
        [AeLabel(isDropDown: true, placeholder: "Please Select")] 
        [MaxLength(1000)] 
        public string? Does_This_Indicator_Support_Gba { get; set; }
        [AeLabel(placeholder: "In what way does the indicator support GBA+")] 
        [MaxLength(4000)] 
        public string? If_Yes_Please_Provide_An_Explanation_Of_How { get; set; }
        [AeLabel(placeholder: "Additional definitions/information necessary to understand how the data will be collected for this indicator; Sectors may also insert a link to a methodology sheet in this column")] 
        [MaxLength(8000)] 
        public string? Methodology_How_Will_The_Indicator_Be_Measured { get; set; }

        public PIP_Tombstone PIP_Tombstone { get; set; }

        [AeFormIgnore]
        public string Last_Updated_UserId { get; set; }

        [AeFormIgnore]
        public DateTime Last_Updated_DT { get; set; }

        [AeFormIgnore]
        [Timestamp]
        public byte[] Timestamp { get; set; }

        


    }
}
