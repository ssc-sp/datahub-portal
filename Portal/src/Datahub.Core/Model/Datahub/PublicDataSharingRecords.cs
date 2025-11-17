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

/// <summary>
/// Represents a generic file that is shared through open data or potentially other future services.
/// </summary>
public class SharedDataFile
{
    /// <summary>
    /// Prefix used for localizing sharing status labels.
    /// </summary>
    private static readonly string SHARING_STATUS_LOCALIZATION_PREFIX = "SHARING-STATUS";

    /// <summary>
    /// Prefix used for localizing public URL sharing status labels.
    /// </summary>
    public static readonly string PUBLIC_URL_SHARING_STATUS_LOCALIZATION_PREFIX = SHARING_STATUS_LOCALIZATION_PREFIX + ".PublicUrl";

    /// <summary>
    /// Prefix used for localizing open data sharing status labels.
    /// </summary>
    public static readonly string OPEN_DATA_SHARING_STATUS_LOCALIZATION_PREFIX = SHARING_STATUS_LOCALIZATION_PREFIX + ".OpenData";

    /// <summary>
    /// Gets or sets the unique identifier of the shared data file.
    /// </summary>
    [Key]
    public long SharedDataFile_ID { get; set; }

    /// <summary>
    /// Gets or sets the physical location or reference of the file.
    /// </summary>
    public Guid File_ID { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this file is intended for an open data request.
    /// </summary>
    public bool IsOpenDataRequest_FLAG { get; set; } = false;

    /// <summary>
    /// Gets or sets the name of the file.
    /// </summary>
    public string? Filename_TXT { get; set; }

    /// <summary>
    /// Gets or sets the folder path where the file is located.
    /// </summary>
    public string? FolderPath_TXT { get; set; }

    /// <summary>
    /// Gets or sets the code of the project that owns this file.
    /// </summary>
    public string? ProjectCode_CD { get; set; }

    /// <summary>
    /// Gets a value indicating whether this file is associated with a project.
    /// </summary>
    public bool IsProjectBased => !string.IsNullOrEmpty(this.ProjectCode_CD);

    /// <summary>
    /// Gets or sets the user ID of the person requesting publication of this file.
    /// </summary>
    [Required]
    [StringLength(200)]
    public string RequestingUser_ID { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user ID of the individual responsible for approving the publication of this file.
    /// </summary>
    [StringLength(200)]
    public string? ApprovingUser_ID { get; set; }

    /// <summary>
    /// Gets or sets the date when the file was requested for sharing.
    /// </summary>
    public DateTime RequestedDate_DT { get; set; }

    /// <summary>
    /// Gets or sets the date when the file was submitted for approval.
    /// </summary>
    public DateTime? SubmittedDate_DT { get; set; }

    /// <summary>
    /// Gets or sets the date when the file was approved for publication.
    /// </summary>
    public DateTime? ApprovedDate_DT { get; set; }

    /// <summary>
    /// Gets or sets the date when the file was published for public access.
    /// </summary>
    public DateTime? PublicationDate_DT { get; set; }

    /// <summary>
    /// Gets or sets the date when the file's public access expires.
    /// </summary>
    public DateTime? ExpirationDate_DT { get; set; }

    /// <summary>
    /// Gets or sets the date when the file was unpublished.
    /// </summary>
    public DateTime? UnpublishDate_DT { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether all required metadata fields have been completed.
    /// </summary>
    public bool MetadataCompleted_FLAG { get; set; }

    /// <summary>
    /// Determines the current public URL sharing status of this file.
    /// </summary>
    /// <returns>The current <see cref="PublicUrlSharingStatus"/>.</returns>
    public PublicUrlSharingStatus GetPublicUrlSharingStatus()
    {
        if (this.ApprovedDate_DT.HasValue && this.ExpirationDate_DT.HasValue && this.ExpirationDate_DT.Value <= DateTime.UtcNow)
        {
            return PublicUrlSharingStatus.Expired;
        }

        if (this.PublicationDate_DT.HasValue && this.PublicationDate_DT.Value <= DateTime.UtcNow)
        {
            return PublicUrlSharingStatus.AccessPublicUrl;
        }
        else if (this.ApprovedDate_DT.HasValue && this.ApprovedDate_DT.Value <= DateTime.UtcNow)
        {
            return PublicUrlSharingStatus.PendingPublication;
        }
        else if (this.SubmittedDate_DT.HasValue && this.SubmittedDate_DT.Value <= DateTime.UtcNow)
        {
            return PublicUrlSharingStatus.PendingApproval;
        }
        else if (this.MetadataCompleted_FLAG)
        {
            return PublicUrlSharingStatus.RequestApproval;
        }
        else
        {
            return PublicUrlSharingStatus.EnterMetadata;
        }
    }

    /// <summary>
    /// Retrieves the localized resource key for the current status of this file.
    /// </summary>
    /// <returns>A string containing the localized status key.</returns>
    public string GetStatusKey()
    {
        string prefix;
        string statusCode;

        if (this.IsOpenDataRequest_FLAG && this is OpenDataSharedFile file)
        {
            prefix = OPEN_DATA_SHARING_STATUS_LOCALIZATION_PREFIX;
            var status = file.GetOpenDataSharingStatus();
            statusCode = status.ToString();
        }
        else
        {
            prefix = PUBLIC_URL_SHARING_STATUS_LOCALIZATION_PREFIX;
            var status = this.GetPublicUrlSharingStatus();
            statusCode = status.ToString();
        }

        return $"{prefix}.{statusCode}.Title";
    }
}

/// <summary>
/// Represents a file used for open data sharing, extending the base SharedDataFile
/// with additional properties and logic specific to open data publishing.
/// </summary>
[Table("OpenDataSharedFile")]
public class OpenDataSharedFile : SharedDataFile
{
    /// <summary>
    /// Gets or sets the ID of the approval form associated with this file.
    /// </summary>
    public int? ApprovalForm_ID { get; set; }

    /// <summary>
    /// Gets or sets the URL to the signed approval form for this file.
    /// </summary>
    public string? SignedApprovalForm_URL { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the approval form has been read.
    /// </summary>
    public bool ApprovalFormRead_FLAG { get; set; }

    /// <summary>
    /// Gets or sets the file storage location for this open data file.
    /// </summary>
    public FileStorageType? FileStorage_CD { get; set; }

    /// <summary>
    /// Gets or sets the upload status of this file to the open data portal.
    /// </summary>
    public OpenDataUploadStatus UploadStatus_CD { get; set; }

    /// <summary>
    /// Gets or sets any error messages that occurred during the file upload process.
    /// </summary>
    public string? UploadError_TXT { get; set; }

    /// <summary>
    /// Gets or sets the public URL of the file once it has been uploaded.
    /// </summary>
    public string? FileUrl_TXT { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the approval form has been edited after loading.
    /// </summary>
    public bool ApprovalFormEdited_FLAG { get; set; }

    /// <summary>
    /// Determines the current open data sharing status of this file.
    /// </summary>
    /// <returns>The current <see cref="OpenDataSharingStatus"/> for this file.</returns>
    public OpenDataSharingStatus GetOpenDataSharingStatus()
    {
        if (FileStorage_CD.HasValue)
        {
            return OpenDataSharingStatus.AccessOpenData;
        }
        if (PublicationDate_DT.HasValue && PublicationDate_DT.Value <= DateTime.UtcNow)
        {
            return OpenDataSharingStatus.PendingUpload;
        }
        else if (ApprovedDate_DT.HasValue && ApprovedDate_DT.Value <= DateTime.UtcNow)
        {
            return OpenDataSharingStatus.PendingPublication;
        }
        else if (!string.IsNullOrEmpty(SignedApprovalForm_URL))
        {
            return OpenDataSharingStatus.PendingApproval;
        }
        else if (ApprovalForm_ID.HasValue && ApprovalForm_ID > 0 && ApprovalFormEdited_FLAG)
        {
            return OpenDataSharingStatus.SubmitSignedPDF;
        }
        else if (MetadataCompleted_FLAG)
        {
            return OpenDataSharingStatus.OpenGovApprovalForm;
        }
        else
        {
            return OpenDataSharingStatus.EnterMetadata;
        }
    }
}