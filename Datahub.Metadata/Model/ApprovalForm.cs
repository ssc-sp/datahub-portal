using System.ComponentModel.DataAnnotations;

namespace NRCan.Datahub.Metadata.Model
{
    public class ApprovalForm
    {
        public int ApprovalFormId { get; set; }
        [StringLength(32)]
        public string Department_NAME { get; set; }
        [StringLength(32)]
        public string Sector_NAME { get; set; }
        [StringLength(32)]
        public string Branch_NAME { get; set; }
        [StringLength(32)]
        public string Division_NAME { get; set; }
        [StringLength(32)]
        public string Section_NAME { get; set; }
        [StringLength(200)]
        [Required]
        public string Name_NAME { get; set; }
        [StringLength(32)]
        public string Phone_TXT { get; set; }
        [StringLength(200)]
        [Required]
        public string Email_EMAIL { get; set; }
        public string Dataset_Title_TXT { get; set; }
        public string Type_Of_Data_TXT { get; set; }
        public bool Copyright_Restrictions_FLAG { get; set; }
        public bool Authority_To_Release_FLAG { get; set; }
        public bool Private_Personal_Information_FLAG { get; set; }
        public bool Subject_To_Exceptions_Or_Eclusions_FLAG { get; set; }
        public bool Not_Clasified_Or_Protected_FLAG { get; set; }
        public bool Can_Be_Released_For_Free_FLAG { get; set; }
        public bool Machine_Readable_FLAG { get; set; }
        public bool Non_Propietary_Format_FLAG { get; set; }
        public bool Localized_Metadata_FLAG { get; set; }
        public bool Requires_Blanket_Approval_FLAG { get; set; }
        public bool Updated_On_Going_Basis_FLAG { get; set; }
        public bool Collection_Of_Datasets_FLAG { get; set; }
        public bool Approval_InSitu_FLAG { get; set; }
        public bool Approval_Other_FLAG { get; set; }
        public string Approval_Other_TXT { get; set; }
    }
}
