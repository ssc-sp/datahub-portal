using Elemental.Components.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace NRCan.Datahub.ProjectForms.Data.PIP
{
    public class PIP_Tombstone
    {
        [Key]
        [AeFormIgnore]
        public int Tombstone_ID { get; set; }

        [AeLabel(row: "1", column:"1")]
        [AeFormCategory("",1)]
        [Editable(false)] [Required] [MaxLength(400)] public string Program_Title { get; set; }
        [AeFormCategory("", 1)]
        [AeLabel(row: "1", column: "2")]
        [Editable(false)] [Required] [MaxLength(400)] public string Lead_Sector { get; set; }
        [AeFormCategory("", 1)]
        [AeLabel(row: "1", column: "3")]
        [Editable(false)] [Required] [MaxLength(400)] public string Program_Official_Title { get; set; }

        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "2", column: "1")]
        [AeFormCategory("", 1)]        
        [Required]
        [MaxLength(400)]
        [Editable(false)] public string Core_Responsbility_DESC { get; set; }

        [AeLabel(size: 100, row: "3", column: "1")] 
        [AeFormCategory("", 1)]
        [Editable(false)] public string Program_Inventory_Program_Description_URL { get; set; }

        [AeLabel(row: "4", column: "1")]
        [AeFormCategory("", 1)] 
        [MaxLength(4000)] 
        public string Program_Notes { get; set; }

        [AeLabel(row: "5", column: "1")]
        [AeFormCategory("Spending (in $) as per GC InfoBase (to be updated by CMSS)", 20)]
        [Column(TypeName = "Money")]
        [Editable(false)] public double? Planned_Spending_AMTL { get; set; }
        [AeLabel(row: "5", column: "2", placeholder: "to be updated by CMSS")]
        [AeFormCategory("Spending (in $) as per GC InfoBase (to be updated by CMSS)", 20)]
        [Column(TypeName = "Money")]
        public double? Actual_Spending_AMTL { get; set; }



        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "6", column:"1")]
        [AeFormCategory("Program Tags", 50)]
        [Required]
        [MaxLength(400)]
        [Editable(false)] public string Departmental_Result_1_CD { get; set; }
        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "6", column: "2")]
        [AeFormCategory("Program Tags", 50)]
        [MaxLength(400)]
        [Editable(false)] public string Departmental_Result_2_CD { get; set; }
        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "6", column: "3")]
        [AeFormCategory("Program Tags", 50)]
        [MaxLength(400)]
        [Editable(false)] public string Departmental_Result_3_CD { get; set; }
        
        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "7", column: "1")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string Strategic_Priorities_1_DESC { get; set; }        

        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "8", column: "1")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string Strategic_Priorities_2_DESC { get; set; }
        
        
        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "9", column: "1")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string Mandate_Letter_Commitment_1_DESC { get; set; }

        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "10", column: "1")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string Mandate_Letter_Commitment_2_DESC { get; set; }
        
        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "11", column: "1")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string Mandate_Letter_Commitment_3_DESC { get; set; }

        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "12", column: "1")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string Mandate_Letter_Commitment_4_DESC { get; set; }
        
        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "13", column: "1")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string Transfer_Payment_Programs_1_DESC { get; set; }

        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "14", column: "1")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string Transfer_Payment_Programs_2_DESC { get; set; }
        
        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "15", column: "1")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string Transfer_Payment_Programs_3_DESC { get; set; }

        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "16", column: "1")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string Transfer_Payment_Programs_4_DESC { get; set; }
        
        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "17", column: "1")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string Transfer_Payment_Programs_5_DESC { get; set; }

        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "18", column: "1")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string Transfer_Payment_Programs_6_DESC { get; set; }
        
        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "19", column: "1")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string Transfer_Payment_Programs_7_DESC { get; set; }
              
        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "20", column: "1")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string Transfer_Payment_Programs_Less5_1_DESC { get; set; }
        
        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "21", column: "1")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string Transfer_Payment_Programs_Less5_2_DESC { get; set; }

        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "22", column: "1")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string Transfer_Payment_Programs_Less5_3_DESC { get; set; }
              
        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "23", column: "1")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string Horizontal_Initiative_1_DESC { get; set; }

        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "24", column: "1")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string Horizontal_Initiative_2_DESC { get; set; }
        
        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "25", column: "1")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string Horizontal_Initiative_3_DESC { get; set; }        
        
        [Editable(false)] [AeLabel(isDropDown: true, placeholder: "Please Select", row: "26", column: "1")] [AeFormCategory("Program Tags", 50)] [Required] [MaxLength(400)] public string Method_Of_Intervention_1_DESC { get; set; }
        [Editable(false)] [AeLabel(isDropDown: true, placeholder: "Please Select", row: "26", column: "2")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string Method_Of_Intervention_2_DESC { get; set; }
        
        [Editable(false)] [AeLabel(isDropDown: true, placeholder: "Please Select", row: "27", column: "1")] [AeFormCategory("Program Tags", 50)] [Required] [MaxLength(400)] public string Target_Group_1_DESC { get; set; }        
        [Editable(false)] [AeLabel(isDropDown: true, placeholder: "Please Select", row: "27", column: "2")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string Target_Group_2_DESC { get; set; }

        [Editable(false)] [AeLabel(isDropDown: true, placeholder: "Please Select", row: "28", column: "1")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string Target_Group_3_DESC { get; set; }
        [Editable(false)] [AeLabel(isDropDown: true, placeholder: "Please Select", row: "28", column: "2")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string Target_Group_4_DESC { get; set; }

        [Editable(false)] [AeLabel(isDropDown: true, placeholder: "Please Select", row: "29", column: "1")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string Target_Group_5_DESC { get; set; }

        [Editable(false)] [AeLabel(isDropDown: true, placeholder: "Please Select", row: "30", column: "1")] [AeFormCategory("Program Tags", 50)] [Required] [MaxLength(400)] public string Government_Of_Canada_Activity_Tags_DESC { get; set; }
        
        [Editable(false)] [AeLabel(isDropDown: true, placeholder: "Please Select", row: "31", column: "1")] [AeFormCategory("Program Tags", 50)] [Required] [MaxLength(4000)] public string Canadian_Classification_Of_Functions_Of_Government_DESC { get; set; }


        [AeFormCategory("", 54)]
        [MaxLength(4000)] [AeLabel(placeholder: "Please Select", row: "32", column: "1")] 
        public string Related_Program_Or_Activities { get; set; }

        
        [AeLabel(row: "34", column: "1", isDropDown: true, placeholder: "Please Select")]
        [AeFormCategory("GBA+", 56)]
        [MaxLength(50)]
        public string Does_Indicator_Enable_Program_Measure_Equity_Option { get; set; }
        [AeFormCategory("GBA+", 56)]
        [AeLabel(row: "34", column: "2")]
        [MaxLength(8000)]
        public string Does_Indicator_Enable_Program_Measure_Equity { get; set; }



        [AeFormCategory("GBA+", 56)]
        [AeLabel(row: "35", column: "1", isDropDown: true, placeholder: "Please Select")]
        [MaxLength(1000)]
        public string Is_Equity_Seeking_Group { get; set; }

        [AeFormCategory("GBA+", 56)]
        [AeLabel(row: "36", column: "1", isDropDown: true, placeholder: "Please Select")]
        [MaxLength(1000)]
        public string Is_Equity_Seeking_Group2 { get; set; }

        [AeFormCategory("GBA+", 56)]
        [AeLabel(row: "37", column: "1", placeholder: "If others, please add comment here ")]
        [MaxLength(4000)]
        public string Is_Equity_Seeking_Group_Other { get; set; }

        [AeLabel(row: "38", column: "1")]
        [AeFormCategory("GBA+", 56)]
        [MaxLength(4000)]
        public string No_Equity_Seeking_Group { get; set; }



        [AeLabel(row: "39", column: "1")]
        [AeFormCategory("Date of PIP Approval", 57)] public DateTime? Approval_By_Program_Offical_DT { get; set; }
        [AeLabel(row: "39", column: "2")]
        [AeFormCategory("Date of PIP Approval", 57)] public DateTime? Consultation_With_The_Head_Of_Evaluation_DT { get; set; }
        [AeLabel(row: "39", column: "3")]
        [AeFormCategory("Date of PIP Approval", 57)] public DateTime? Functional_SignOff_DT { get; set; }





        [AeLabel(row: "40", column: "1")]
        [AeFormCategory("Latest Update Information", 60)]
        [Editable(false)] public string Last_Updated_UserId { get; set; }
        [AeLabel(row: "40", column: "2")]        
        [AeFormCategory("Latest Update Information", 60)]
        [Required] public DateTime Date_Updated_DT { get; set; }
        


        [AeFormIgnore]
        public DateTime Last_Updated_DT { get; set; }

        [AeFormIgnore]
        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}
