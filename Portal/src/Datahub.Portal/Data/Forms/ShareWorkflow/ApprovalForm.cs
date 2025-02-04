using System.ComponentModel.DataAnnotations;
using MudBlazor.Forms;

namespace Datahub.Portal.Data.Forms.ShareWorkflow;

public class ApprovalForm
{
    public const string DATASET_DATA_TYPE_VALUE = "Data";
    public const string OPEN_INFORMATION_DATA_TYPE_VALUE = "Info";

    /** Section:  **/
    [Key]
    [AeFormIgnore]
    public int ApprovalFormId { get; set; }

    /** Section: Business Partner **/
    [StringLength(256)]
    [AeFormCategory("Business Owner")]
    public string Department_NAME { get; set; }

    [AeFormIgnore]
    public int Sector_ID { get; set; }

    /** Section: Business Partner **/
    [StringLength(256)]
    [AeFormCategory("Business Owner")]
    public string Sector_NAME { get; set; }

    [AeFormIgnore]
    public int Branch_ID { get; set; }

    /** Section: Business Partner **/
    [StringLength(256)]
    [AeFormCategory("Business Owner")]
    public string Branch_NAME { get; set; }

    /** Section: Business Partner **/
    [StringLength(256)]
    [AeFormCategory("Business Owner")]
    public string Division_NAME { get; set; }

    /** Section: Business Partner **/
    [StringLength(256)]
    [AeFormCategory("Business Owner")]
    public string Section_NAME { get; set; }

    /** Section: Business Partner **/
    [StringLength(256)]
    [AeFormCategory("Business Owner")]
    [Required]
    public string Name_NAME { get; set; }

    /** Section: Business Partner **/
    [StringLength(32)]
    [AeFormCategory("Business Owner")]
    public string Phone_TXT { get; set; }

    /** Section: Business Partner **/
    [StringLength(128)]
    [AeFormCategory("Business Owner")]
    [Required]
    public string Email_EMAIL { get; set; }

    /** Section: Source Information **/
    [Required]
    [AeFormCategory("Source Information")]
    [StringLength(256)]
    public string Dataset_Title_TXT { get; set; }

    /** Section: Source Information **/
    [Required]
    [StringLength(16)]
    [AeFormCategory("Source Information")]
    public string Type_Of_Data_TXT { get; set; }

    // 1
    /** Section: Confidentiality **/
    [AeFormCategory("Mandatory Release Criteria")]
    public bool Confidentiality_FLAG { get; set; }

    // 2
    /** Section: Access to Information **/
    [AeFormCategory("Mandatory Release Criteria")]
    public bool Subject_To_Exceptions_Or_Eclusions_FLAG { get; set; }

    // 3
    /** Section: Authority to Release **/
    [AeFormCategory("Mandatory Release Criteria")]
    public bool Authority_To_Release_FLAG { get; set; }

    // 4a
    /** Section: Privacy **/
    [AeFormCategory("Mandatory Release Criteria")]
    public bool Privacy_Exemption_FLAG { get; set; }

    // 4b
    /** Section: Privacy **/
    [AeFormCategory("Mandatory Release Criteria")]
    public bool Private_Personal_Information_FLAG { get; set; }

    // 5a:
    /** Section: Formats **/
    [AeFormCategory("Mandatory Release Criteria")]
    public bool Accessible_Format_FLAG { get; set; }

    // 5b
    /** Section: Formats **/
    [AeFormCategory("Mandatory Release Criteria")]
    public bool Machine_Readable_FLAG { get; set; }

    // 5c
    /** Section: Format **/
    [AeFormCategory("Mandatory Release Criteria")]
    public bool Non_Propietary_Format_FLAG { get; set; }
    
    // 6
    /** Section: Official Languages **/
    [AeFormCategory("Mandatory Release Criteria")]
    public bool Localized_FLAG { get; set; }
    
    // 7
    /** Section: Security **/
    [AeFormCategory("Mandatory Release Criteria")]
    public bool Security_Compliant_FLAG { get; set; }

    // 8
    /** Section: Other - Legal / Regulatory / Policy / Contractual **/
    [AeFormCategory("Mandatory Release Criteria")]
    public bool Misc_Compliant_FLAG { get; set; }


    /** Section: Cost **/
    [AeFormCategory("Mandatory Release Criteria")]
    [AeFormIgnore]
    public bool Can_Be_Released_For_Free_FLAG { get; set; }



    /** Section: Format **/
    [AeFormIgnore]
    [AeFormCategory("Blanket Approvals")]
    public bool Requires_Blanket_Approval_FLAG { get; set; }

    /** Section: Blanket Approval **/
    [AeFormCategory("Blanket Approvals")]
    public bool Updated_On_Going_Basis_FLAG { get; set; }

    /** Section: Blanket Approval **/
    [AeFormCategory("Blanket Approvals")]
    [AeFormIgnore]
    public bool Collection_Of_Datasets_FLAG { get; set; }

    /** Section: Blanket Approval **/
    [AeFormCategory("Blanket Approvals")]
    [AeFormIgnore]
    public bool Approval_InSitu_FLAG { get; set; }

    /** Section: Blanket Approval **/
    [AeFormCategory("Blanket Approvals")]
    [AeFormIgnore]
    public bool Approval_Other_FLAG { get; set; }

    /** Section: Blanket Approval **/
    [AeFormCategory("Blanket Approvals")]
    public string Approval_Other_TXT { get; set; }
}