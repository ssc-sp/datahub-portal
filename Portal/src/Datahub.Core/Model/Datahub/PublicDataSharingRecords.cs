using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Datahub.Core.Model.Datahub;

public enum PublicUrlSharingStatus
{
    EnterMetadata,
    RequestApproval,
    PendingApproval,
    PendingPublication,
    AccessPublicUrl,
    Expired
}

public enum OpenDataSharingStatus
{
    EnterMetadata,
    OpenGovApprovalForm,
    SubmitSignedPDF,
    PendingApproval,
    PendingPublication,
    PendingUpload,
    AccessOpenData
}

public enum FileStorageType
{
    Datahub,
    OpenData
}

public enum OpenDataUploadStatus
{
    NotStarted,
    UploadingFile,
    RecordCreated,
    UploadCompleted,
    Failed
}

public class SharedDataFile
{
    private static readonly string SHARING_STATUS_LOCALIZATION_PREFIX = "SHARING-STATUS";
    public static readonly string PUBLICURLSHARINGSTATUSLOCALIZATIONPREFIX = SHARING_STATUS_LOCALIZATION_PREFIX + ".PublicUrl";
    public static readonly string OPENDATASHARINGSTATUSLOCALIZATIONPREFIX = SHARING_STATUS_LOCALIZATION_PREFIX + ".OpenData";

    [Key]
    public long SharedDataFileID { get; set; }

    public Guid FileID { get; set; }

    public bool IsOpenDataRequestFLAG { get; set; } = false;

    public string FilenameTXT { get; set; }

    public string FolderPathTXT { get; set; }
    public string ProjectCodeCD { get; set; }
    public bool IsProjectBased => !string.IsNullOrEmpty(ProjectCodeCD);

    [Required]
    [StringLength(200)]
    public string RequestingUserID { get; set; }

    [StringLength(200)]
    public string ApprovingUserID { get; set; }

    public DateTime RequestedDateDT { get; set; }
    public DateTime? SubmittedDateDT { get; set; }
    public DateTime? ApprovedDateDT { get; set; }
    public DateTime? PublicationDateDT { get; set; }
    public DateTime? ExpirationDateDT { get; set; }
    public DateTime? UnpublishDateDT { get; set; }
    public bool MetadataCompletedFLAG { get; set; }

    public PublicUrlSharingStatus GetPublicUrlSharingStatus()
    {
        if (ApprovedDateDT.HasValue && ExpirationDateDT.HasValue && ExpirationDateDT.Value <= DateTime.UtcNow)
        {
            return PublicUrlSharingStatus.Expired;
        }
        if (PublicationDateDT.HasValue && PublicationDateDT.Value <= DateTime.UtcNow)
        {
            return PublicUrlSharingStatus.AccessPublicUrl;
        }
        else if (ApprovedDateDT.HasValue && ApprovedDateDT.Value <= DateTime.UtcNow)
        {
            return PublicUrlSharingStatus.PendingPublication;
        }
        else if (SubmittedDateDT.HasValue && SubmittedDateDT.Value <= DateTime.UtcNow)
        {
            return PublicUrlSharingStatus.PendingApproval;
        }
        else if (MetadataCompletedFLAG)
        {
            return PublicUrlSharingStatus.RequestApproval;
        }
        else
        {
            return PublicUrlSharingStatus.EnterMetadata;
        }
    }

    public string GetStatusKey()
    {
        string prefix;
        string statusCode;

        if (IsOpenDataRequestFLAG && this is OpenDataSharedFile file)
        {
            prefix = OPENDATASHARINGSTATUSLOCALIZATIONPREFIX;
            var status = file.GetOpenDataSharingStatus();
            statusCode = status.ToString();
        }
        else
        {
            prefix = PUBLICURLSHARINGSTATUSLOCALIZATIONPREFIX;
            var status = GetPublicUrlSharingStatus();
            statusCode = status.ToString();
        }

        return $"{prefix}.{statusCode}.Title";
    }
}

[Table("OpenDataSharedFile")]
public class OpenDataSharedFile : SharedDataFile
{
    public int? ApprovalFormID { get; set; }
    public string SignedApprovalFormURL { get; set; }
    public bool ApprovalFormReadFLAG { get; set; }
    public FileStorageType? FileStorageCD { get; set; }
    public OpenDataUploadStatus UploadStatusCD { get; set; }
    public string UploadErrorTXT { get; set; }
    public string FileUrlTXT { get; set; }
    public bool ApprovalFormEditedFLAG { get; set; }

    public OpenDataSharingStatus GetOpenDataSharingStatus()
    {
        if (FileStorageCD.HasValue)
        {
            return OpenDataSharingStatus.AccessOpenData;
        }
        if (PublicationDateDT.HasValue && PublicationDateDT.Value <= DateTime.UtcNow)
        {
            return OpenDataSharingStatus.PendingUpload;
        }
        else if (ApprovedDateDT.HasValue && ApprovedDateDT.Value <= DateTime.UtcNow)
        {
            return OpenDataSharingStatus.PendingPublication;
        }
        else if (!string.IsNullOrEmpty(SignedApprovalFormURL))
        {
            return OpenDataSharingStatus.PendingApproval;
        }
        else if (ApprovalFormID.HasValue && ApprovalFormID > 0 && ApprovalFormEditedFLAG)
        {
            return OpenDataSharingStatus.SubmitSignedPDF;
        }
        else if (MetadataCompletedFLAG)
        {
            return OpenDataSharingStatus.OpenGovApprovalForm;
        }
        else
        {
            return OpenDataSharingStatus.EnterMetadata;
        }
    }
}