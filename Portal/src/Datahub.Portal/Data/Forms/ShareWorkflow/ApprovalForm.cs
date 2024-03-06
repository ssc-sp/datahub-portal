using System.ComponentModel.DataAnnotations;
using MudBlazor.Forms;

namespace Datahub.Portal.Data.Forms.ShareWorkflow;

public class ApprovalForm
{
    /** Section:  **/
    [Key]
    [AeFormIgnore]
    public int ApprovalFormId { get; set; }

    /** Section: Business Partner **/
    [StringLength(256)]
    [AeFormCategory("Business Owner")]
    public string DepartmentNAME { get; set; }

    [AeFormIgnore]
    public int SectorID { get; set; }

    /** Section: Business Partner **/
    [StringLength(256)]
    [AeFormCategory("Business Owner")]
    public string SectorNAME { get; set; }

    [AeFormIgnore]
    public int BranchID { get; set; }

    /** Section: Business Partner **/
    [StringLength(256)]
    [AeFormCategory("Business Owner")]
    public string BranchNAME { get; set; }

    /** Section: Business Partner **/
    [StringLength(256)]
    [AeFormCategory("Business Owner")]
    public string DivisionNAME { get; set; }

    /** Section: Business Partner **/
    [StringLength(256)]
    [AeFormCategory("Business Owner")]
    public string SectionNAME { get; set; }

    /** Section: Business Partner **/
    [StringLength(256)]
    [AeFormCategory("Business Owner")]
    [Required]
    public string NameNAME { get; set; }

    /** Section: Business Partner **/
    [StringLength(32)]
    [AeFormCategory("Business Owner")]
    public string PhoneTXT { get; set; }

    /** Section: Business Partner **/
    [StringLength(128)]
    [AeFormCategory("Business Owner")]
    [Required]
    public string EmailEMAIL { get; set; }

    /** Section: Source Information **/
    [Required]
    [AeFormCategory("Source Information")]
    [StringLength(256)]
    public string DatasetTitleTXT { get; set; }

    /** Section: Source Information **/
    [Required]
    [StringLength(16)]
    [AeFormCategory("Source Information")]
    public string TypeOfDataTXT { get; set; }

    /** Section: Legal / Licensing / Copyright **/
    [AeFormCategory("Mandatory Release Criteria")]
    public bool CopyrightRestrictionsFLAG { get; set; }

    /** Section: Authority to Release **/
    [AeFormCategory("Mandatory Release Criteria")]
    public bool AuthorityToReleaseFLAG { get; set; }

    /** Section: Privacy **/
    [AeFormCategory("Mandatory Release Criteria")]
    public bool PrivatePersonalInformationFLAG { get; set; }

    /** Section: Access to Information **/
    [AeFormCategory("Mandatory Release Criteria")]
    public bool SubjectToExceptionsOrEclusionsFLAG { get; set; }

    /** Section: Security **/
    [AeFormCategory("Mandatory Release Criteria")]
    public bool NotClasifiedOrProtectedFLAG { get; set; }

    /** Section: Cost **/
    [AeFormCategory("Mandatory Release Criteria")]
    public bool CanBeReleasedForFreeFLAG { get; set; }

    /** Section: Format **/
    [AeFormCategory("Mandatory Release Criteria")]
    public bool MachineReadableFLAG { get; set; }

    /** Section: Format **/
    [AeFormCategory("Mandatory Release Criteria")]
    public bool NonPropietaryFormatFLAG { get; set; }

    /** Section: Format **/
    [AeFormCategory("Mandatory Release Criteria")]
    public bool LocalizedMetadataFLAG { get; set; }

    /** Section: Format **/
    [AeFormIgnore]
    [AeFormCategory("Blanket Approvals")]
    public bool RequiresBlanketApprovalFLAG { get; set; }

    /** Section: Blanket Approval **/
    [AeFormCategory("Blanket Approvals")]
    public bool UpdatedOnGoingBasisFLAG { get; set; }

    /** Section: Blanket Approval **/
    [AeFormCategory("Blanket Approvals")]
    public bool CollectionOfDatasetsFLAG { get; set; }

    /** Section: Blanket Approval **/
    [AeFormCategory("Blanket Approvals")]
    public bool ApprovalInSituFLAG { get; set; }

    /** Section: Blanket Approval **/
    [AeFormCategory("Blanket Approvals")]
    [AeFormIgnore]
    public bool ApprovalOtherFLAG { get; set; }

    /** Section: Blanket Approval **/
    [AeFormCategory("Blanket Approvals")]
    public string ApprovalOtherTXT { get; set; }
}