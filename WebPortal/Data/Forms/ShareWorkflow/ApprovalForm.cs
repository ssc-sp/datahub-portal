using System.ComponentModel.DataAnnotations;
using Ae = Elemental.Components;
using Mb = MudBlazor.Forms;

namespace Datahub.Portal.Data.Forms.ShareWorkflow
{
    public class ApprovalForm
    {
        /** Section:  **/
        [Key]
        [Ae.AeFormIgnore]
        [Mb.AeFormIgnore]
        public int ApprovalFormId { get; set; }

        /** Section: Business Partner **/
        [StringLength(256)]
        [Ae.AeFormCategory("Business Owner")]
        [Mb.AeFormCategory("Business Owner")]
        public string Department_NAME { get; set; }

        [Ae.AeFormIgnore]
        [Mb.AeFormIgnore]
        public int Sector_ID { get; set; }

        /** Section: Business Partner **/
        [StringLength(256)]
        [Ae.AeFormCategory("Business Owner")]
        [Mb.AeFormCategory("Business Owner")]
        [Ae.AeLabel(isDropDown: true, placeholder: " ")]
        public string Sector_NAME { get; set; }

        [Ae.AeFormIgnore]
        [Mb.AeFormIgnore]
        public int Branch_ID { get; set; }

        /** Section: Business Partner **/
        [StringLength(256)]
        [Ae.AeFormCategory("Business Owner")]
        [Mb.AeFormCategory("Business Owner")]
        [Ae.AeLabel(isDropDown: true, placeholder: " ")]
        public string Branch_NAME { get; set; }

        /** Section: Business Partner **/
        [StringLength(256)]
        [Ae.AeFormCategory("Business Owner")]
        [Mb.AeFormCategory("Business Owner")]
        [Ae.AeLabel(isDropDown: true, placeholder: " ")]
        public string Division_NAME { get; set; }

        /** Section: Business Partner **/
        [StringLength(256)]
        [Ae.AeFormCategory("Business Owner")]
        [Mb.AeFormCategory("Business Owner")]
        [Ae.AeLabel(isDropDown: true, placeholder: " ")]
        public string Section_NAME { get; set; }

        /** Section: Business Partner **/
        [StringLength(256)]
        [Ae.AeFormCategory("Business Owner")]
        [Mb.AeFormCategory("Business Owner")]
        [Required]
        public string Name_NAME { get; set; }

        /** Section: Business Partner **/
        [StringLength(32)]
        [Ae.AeFormCategory("Business Owner")]
        [Mb.AeFormCategory("Business Owner")]
        public string Phone_TXT { get; set; }

        /** Section: Business Partner **/
        [StringLength(128)]
        [Ae.AeFormCategory("Business Owner")]
        [Mb.AeFormCategory("Business Owner")]
        [Required]
        public string Email_EMAIL { get; set; }

        /** Section: Source Information **/
        [Required]
        [Ae.AeFormCategory("Source Information")]
        [Mb.AeFormCategory("Source Information")]
        [StringLength(256)]
        public string Dataset_Title_TXT { get; set; }

        /** Section: Source Information **/
        [Required]
        [StringLength(16)]
        [Ae.AeFormCategory("Source Information")]
        [Mb.AeFormCategory("Source Information")]
        [Ae.AeLabel(isDropDown: true)]
        public string Type_Of_Data_TXT { get; set; }

        /** Section: Legal / Licensing / Copyright **/
        [Ae.AeFormCategory("Mandatory Release Criteria")]
        [Mb.AeFormCategory("Mandatory Release Criteria")]
        public bool Copyright_Restrictions_FLAG { get; set; }

        /** Section: Authority to Release **/
        [Ae.AeFormCategory("Mandatory Release Criteria")]
        [Mb.AeFormCategory("Mandatory Release Criteria")]
        public bool Authority_To_Release_FLAG { get; set; }

        /** Section: Privacy **/
        [Ae.AeFormCategory("Mandatory Release Criteria")]
        [Mb.AeFormCategory("Mandatory Release Criteria")]
        public bool Private_Personal_Information_FLAG { get; set; }

        /** Section: Access to Information **/
        [Ae.AeFormCategory("Mandatory Release Criteria")]
        [Mb.AeFormCategory("Mandatory Release Criteria")]
        public bool Subject_To_Exceptions_Or_Eclusions_FLAG { get; set; }

        /** Section: Security **/
        [Ae.AeFormCategory("Mandatory Release Criteria")]
        [Mb.AeFormCategory("Mandatory Release Criteria")]
        public bool Not_Clasified_Or_Protected_FLAG { get; set; }

        /** Section: Cost **/
        [Ae.AeFormCategory("Mandatory Release Criteria")]
        [Mb.AeFormCategory("Mandatory Release Criteria")]
        public bool Can_Be_Released_For_Free_FLAG { get; set; }

        /** Section: Format **/
        [Ae.AeFormCategory("Mandatory Release Criteria")]
        [Mb.AeFormCategory("Mandatory Release Criteria")]
        public bool Machine_Readable_FLAG { get; set; }

        /** Section: Format **/
        [Ae.AeFormCategory("Mandatory Release Criteria")]
        [Mb.AeFormCategory("Mandatory Release Criteria")]
        public bool Non_Propietary_Format_FLAG { get; set; }

        /** Section: Format **/
        [Ae.AeFormCategory("Mandatory Release Criteria")]
        [Mb.AeFormCategory("Mandatory Release Criteria")]
        public bool Localized_Metadata_FLAG { get; set; }

        /** Section: Format **/
        [Ae.AeFormIgnore]
        [Mb.AeFormIgnore]
        [Ae.AeFormCategory("Blanket Approvals")]
        [Mb.AeFormCategory("Blanket Approvals")]
        public bool Requires_Blanket_Approval_FLAG { get; set; }

        /** Section: Blanket Approval **/
        [Ae.AeFormCategory("Blanket Approvals")]
        [Mb.AeFormCategory("Blanket Approvals")]
        public bool Updated_On_Going_Basis_FLAG { get; set; }

        /** Section: Blanket Approval **/
        [Ae.AeFormCategory("Blanket Approvals")]
        [Mb.AeFormCategory("Blanket Approvals")]
        public bool Collection_Of_Datasets_FLAG { get; set; }

        /** Section: Blanket Approval **/
        [Ae.AeFormCategory("Blanket Approvals")]
        [Mb.AeFormCategory("Blanket Approvals")]
        public bool Approval_InSitu_FLAG { get; set; }

        /** Section: Blanket Approval **/
        [Ae.AeFormCategory("Blanket Approvals")]
        [Mb.AeFormCategory("Blanket Approvals")]
        [Ae.AeFormIgnore]
        [Mb.AeFormIgnore]
        public bool Approval_Other_FLAG { get; set; }

        /** Section: Blanket Approval **/
        [Ae.AeFormCategory("Blanket Approvals")]
        [Mb.AeFormCategory("Blanket Approvals")]
        public string Approval_Other_TXT { get; set; }
    }
}
