using Elemental.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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
        [AeLabel(isDropDown: true)]
        public string Type_Of_Data_TXT { get; set; }

        /** Section: Legal / Licensing / Copyright **/
        //[AeLabel(isDropDown: true)]
        public bool Copyright_Restrictions_FLAG { get; set; }

        /** Section: Authority to Release **/
        //[AeLabel(isDropDown: true)]
        public bool Authority_To_Release_FLAG { get; set; }

        /** Section: Privacy **/
        //[AeLabel(isDropDown: true)]
        public bool Private_Personal_Information_FLAG { get; set; }

        /** Section: Access to Information **/
        //[AeLabel(isDropDown: true)]
        public bool Subject_To_Exceptions_Or_Eclusions_FLAG { get; set; }

        /** Section: Security **/
        //[AeLabel(isDropDown: true)]
        public bool Not_Clasified_Or_Protected_FLAG { get; set; }

        /** Section: Cost **/
        //[AeLabel(isDropDown: true)]
        public bool Can_Be_Released_For_Free_FLAG { get; set; }

        /** Section: Format **/
        //[AeLabel(isDropDown: true)]
        public bool Machine_Readable_FLAG { get; set; }

        /** Section: Format **/
        //[AeLabel(isDropDown: true)]
        public bool Non_Propietary_Format_FLAG { get; set; }

        /** Section: Format **/
        //[AeLabel(isDropDown: true)]
        public bool Localized_Metadata_FLAG { get; set; }

        /** Section: Blanket Approval **/
        //[AeLabel(isDropDown: true)]
        public bool Updated_On_Going_Basis_FLAG { get; set; }

        /** Section: Blanket Approval **/
        //[AeLabel(isDropDown: true)]
        public bool Collection_Of_Datasets_FLAG { get; set; }

        /** Section: Blanket Approval **/
        //[AeLabel(isDropDown: true)]
        public bool Approval_InSitu_FLAG { get; set; }

        /** Section: Blanket Approval **/
        [Required]
        [MaxLength(256)]
        public string Approval_Other_TXT { get; set; }
    }
}
