using Elemental.Components;
using System.ComponentModel.DataAnnotations;

namespace NRCan.Datahub.Portal.Data.Forms.ShareWorkflow
{
    public class ApprovalForm
    {
        /** Section:  **/
        [Key]
        [AeFormIgnore]
        public int ApprovalFormId { get; set; }

        /** Section: Source Information **/
        [Required]
        [MaxLength(256)]
        public string Dataset_Title_TXT { get; set; }

        /** Section: Source Information **/
        [Required]
        [AeLabel(isDropDown: true)]
        public string Type_Of_Data_TXT { get; set; }

        /** Section: Legal / Licensing / Copyright **/
        public bool Copyright_Restrictions_FLAG { get; set; }

        /** Section: Authority to Release **/
        public bool Authority_To_Release_FLAG { get; set; }

        /** Section: Privacy **/
        public bool Private_Personal_Information_FLAG { get; set; }

        /** Section: Access to Information **/
        public bool Subject_To_Exceptions_Or_Eclusions_FLAG { get; set; }

        /** Section: Security **/
        public bool Not_Clasified_Or_Protected_FLAG { get; set; }

        /** Section: Cost **/
        public bool Can_Be_Released_For_Free_FLAG { get; set; }

        /** Section: Format **/
        public bool Machine_Readable_FLAG { get; set; }

        /** Section: Format **/
        public bool Non_Propietary_Format_FLAG { get; set; }

        /** Section: Format **/
        public bool Localized_Metadata_FLAG { get; set; }

        /** Section: Blanket Approval **/
        public bool Updated_On_Going_Basis_FLAG { get; set; }

        /** Section: Blanket Approval **/
        public bool Collection_Of_Datasets_FLAG { get; set; }

        /** Section: Blanket Approval **/
        public bool Approval_InSitu_FLAG { get; set; }

        /** Section: Blanket Approval **/
        public string Approval_Other_TXT { get; set; }
    }
}
