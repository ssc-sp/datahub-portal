namespace Datahub.Metadata.Model;

public class ApprovalForm
{
    public int ApprovalFormId { get; set; }
    public string DepartmentNAME { get; set; }
    public int SectorID { get; set; }
    public string SectorNAME { get; set; }
    public int BranchID { get; set; }
    public string BranchNAME { get; set; }
    public string DivisionNAME { get; set; }
    public string SectionNAME { get; set; }
    public string NameNAME { get; set; }
    public string PhoneTXT { get; set; }
    public string EmailEMAIL { get; set; }
    public string DatasetTitleTXT { get; set; }
    public string TypeOfDataTXT { get; set; }
    public bool CopyrightRestrictionsFLAG { get; set; }
    public bool AuthorityToReleaseFLAG { get; set; }
    public bool PrivatePersonalInformationFLAG { get; set; }
    public bool SubjectToExceptionsOrEclusionsFLAG { get; set; }
    public bool NotClasifiedOrProtectedFLAG { get; set; }
    public bool CanBeReleasedForFreeFLAG { get; set; }
    public bool MachineReadableFLAG { get; set; }
    public bool NonPropietaryFormatFLAG { get; set; }
    public bool LocalizedMetadataFLAG { get; set; }
    public bool RequiresBlanketApprovalFLAG { get; set; }
    public bool UpdatedOnGoingBasisFLAG { get; set; }
    public bool CollectionOfDatasetsFLAG { get; set; }
    public bool ApprovalInSituFLAG { get; set; }
    public bool ApprovalOtherFLAG { get; set; }
    public string ApprovalOtherTXT { get; set; }
}