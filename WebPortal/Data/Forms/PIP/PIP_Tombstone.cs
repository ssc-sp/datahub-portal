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

        /** Section: Performance Information Profile  **/
        [Editable(false)] [Required] [MaxLength(400)] public string Program_Title { get; set; }
        /** Section: Performance Information Profile  **/
        [Editable(false)] [Required] [MaxLength(400)] public string Lead_Sector { get; set; }
        [Editable(false)] [Required] [MaxLength(400)] public string Program_Official_Title { get; set; }
        /** Section: Performance Information Profile  **/
        [Editable(false)]
        [Required]
        [MaxLength(400)]
        [AeLabel(isDropDown: true, placeholder: "Please Select")]
        public string Core_Responsbility_DESC { get; set; }

        /** Section: Performance Information Profile  **/
        [Required] public DateTime Date_Updated_DT { get; set; }
        /** Section: Actual Spending (as provided by CMSS) **/
        [Editable(false)] [AeFormCategory("Actual Spending (as per GC InfoBase)", 20)] [Column(TypeName = "Money")] public double? Planned_Spending_AMTL { get; set; }
        /** Section: Actual Spending (as per GC InfoBase) **/
        [AeLabel(placeholder: "to be updated by CMSS")] [AeFormCategory("Actual Spending (as per GC InfoBase)", 20)] [Column(TypeName = "Money")] public double? Actual_Spending_AMTL { get; set; }
        /** Section: Sign-off Approval **/
        [AeFormCategory("Date of PIP Approval", 30)] public DateTime? Approval_By_Program_Offical_DT { get; set; }
        /** Section: Sign-off Approval **/
        [AeFormCategory("Date of PIP Approval", 30)] public DateTime? Consultation_With_The_Head_Of_Evaluation_DT { get; set; }
        /** Section: Sign-off Approval **/
        [AeFormCategory("Date of PIP Approval", 30)] public DateTime? Functional_SignOff_DT { get; set; }
        /** Section: Program Inventory Program Description **/
        [AeLabel(size:100)] [Editable(false)] [AeFormCategory("Program Inventory Program Description", 40)] public string? Program_Inventory_Program_Description_URL { get; set; }
        /** Section: Program Tag **/
        [Editable(false)] [AeLabel(isDropDown: true, placeholder: "Please Select")] [AeFormCategory("Program Tags", 50)] [Required] [MaxLength(400)] public string Departmental_Result_1_CD { get; set; }
        /** Section: Program Tag **/
        [Editable(false)] [AeLabel(isDropDown: true, placeholder: "Please Select")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string? Departmental_Result_2_CD { get; set; }
        [Editable(false)] [AeLabel(isDropDown: true, placeholder: "Please Select")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string? Departmental_Result_3_CD { get; set; }
        /** Section: Program Tag **/
        [AeLabel(isDropDown: true, placeholder: "Please Select")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string? Strategic_Priorities_1_DESC { get; set; }
        /** Section: Program Tag **/
        [AeLabel(isDropDown: true, placeholder: "Please Select")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string? Strategic_Priorities_2_DESC { get; set; }
        /** Section: Program Tag **/
        [AeLabel(isDropDown: true, placeholder: "Please Select")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string? Mandate_Letter_Commitment_1_DESC { get; set; }
        /** Section: Program Tag **/
        [AeLabel(isDropDown: true, placeholder: "Please Select")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string? Mandate_Letter_Commitment_2_DESC { get; set; }
        /** Section: Program Tag **/
        [AeLabel(isDropDown: true, placeholder: "Please Select")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string? Mandate_Letter_Commitment_3_DESC { get; set; }
        /** Section: Program Tag **/
        [AeLabel(isDropDown: true, placeholder: "Please Select")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string? Mandate_Letter_Commitment_4_DESC { get; set; }
        /** Section: Program Tag **/
        [AeLabel(isDropDown: true, placeholder: "Please Select")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string? Transfer_Payment_Programs_1_DESC { get; set; }
        /** Section: Program Tag **/
        [AeLabel(isDropDown: true, placeholder: "Please Select")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string? Transfer_Payment_Programs_2_DESC { get; set; }
        /** Section: Program Tag **/
        [AeLabel(isDropDown: true, placeholder: "Please Select")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string? Transfer_Payment_Programs_3_DESC { get; set; }
        /** Section: Program Tag **/
        [AeLabel(isDropDown: true, placeholder: "Please Select")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string? Transfer_Payment_Programs_Less5_1_DESC { get; set; }
        /** Section: Program Tag **/
        [AeLabel(isDropDown: true, placeholder: "Please Select")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string? Transfer_Payment_Programs_Less5_2_DESC { get; set; }
        /** Section: Program Tag **/
        [AeLabel(isDropDown: true, placeholder: "Please Select")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string? Transfer_Payment_Programs_Less5_3_DESC { get; set; }

        /** Section: Program Tag **/
        [AeLabel(isDropDown: true, placeholder: "Please Select")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string? Horizontal_Initiative_1_DESC { get; set; }
        /** Section: Program Tag **/
        [AeLabel(isDropDown: true, placeholder: "Please Select")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string? Horizontal_Initiative_2_DESC { get; set; }
        /** Section: Program Tag **/
        [AeLabel(isDropDown: true, placeholder: "Please Select")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string? Horizontal_Initiative_3_DESC { get; set; }
        /** Section: Program Tag **/
        [Editable(false)] [AeLabel(isDropDown: true, placeholder: "Please Select")] [AeFormCategory("Program Tags", 50)] [Required] [MaxLength(400)] public string Method_Of_Intervention_1_DESC { get; set; }
        /** Section: Program Tag **/
        [Editable(false)] [AeLabel(isDropDown: true, placeholder: "Please Select")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string? Method_Of_Intervention_2_DESC { get; set; }
        /** Section: Program Tag **/
        [Editable(false)] [AeLabel(isDropDown: true, placeholder: "Please Select")] [AeFormCategory("Program Tags", 50)] [Required] [MaxLength(400)] public string Target_Group_1_DESC { get; set; }
        /** Section: Program Tag **/
        [Editable(false)] [AeLabel(isDropDown: true, placeholder: "Please Select")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string? Target_Group_2_DESC { get; set; }
        /** Section: Program Tag **/
        [Editable(false)] [AeLabel(isDropDown: true, placeholder: "Please Select")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string? Target_Group_3_DESC { get; set; }
        /** Section: Program Tag **/
        [Editable(false)] [AeLabel(isDropDown: true, placeholder: "Please Select")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string? Target_Group_4_DESC { get; set; }
        /** Section: Program Tag **/
        [Editable(false)] [AeLabel(isDropDown: true, placeholder: "Please Select")] [AeFormCategory("Program Tags", 50)] [MaxLength(400)] public string? Target_Group_5_DESC { get; set; }
        /** Section: Program Tag **/
        [Editable(false)] [AeLabel(isDropDown: true, placeholder: "Please Select")] [AeFormCategory("Program Tags", 50)] [Required] [MaxLength(400)] public string Government_Of_Canada_Activity_Tags_DESC { get; set; }
        /** Section: Program Tag **/
        [Editable(false)] [AeFormCategory("Program Tags", 50)] [Required] [MaxLength(4000)] public string Canadian_Classification_Of_Functions_Of_Government_DESC { get; set; }

        [AeFormIgnore]
        public string Last_Updated_UserId { get; set; }

        [AeFormIgnore]
        public DateTime Last_Updated_DT { get; set; }

        [AeFormIgnore]
        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}
