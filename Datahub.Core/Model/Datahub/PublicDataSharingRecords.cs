using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Datahub.Core.EFCore
{
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
        AccessOpenData
    }

    public class SharedDataFile
    {
        private static readonly string SHARING_STATUS_LOCALIZATION_PREFIX = "SHARING-STATUS";
        public static readonly string PUBLIC_URL_SHARING_STATUS_LOCALIZATION_PREFIX = SHARING_STATUS_LOCALIZATION_PREFIX + ".PublicUrl";
        public static readonly string OPEN_DATA_SHARING_STATUS_LOCALIZATION_PREFIX = SHARING_STATUS_LOCALIZATION_PREFIX + ".OpenData";


        [Key]
        public long SharedDataFile_ID { get; set; }

        public Guid File_ID { get; set; }

        public bool IsOpenDataRequest_FLAG { get; set; } = false;

        public string Filename_TXT { get; set; }
        
        public string FolderPath_TXT { get; set; }
        public string ProjectCode_CD { get; set; }
        public bool IsProjectBased => !string.IsNullOrEmpty(ProjectCode_CD);

        [Required]
        [StringLength(200)]
        public string RequestingUser_ID { get; set; }
        
        [StringLength(200)]
        public string ApprovingUser_ID { get; set; }

        public DateTime RequestedDate_DT { get; set; }
        public DateTime? SubmittedDate_DT { get; set; }
        public DateTime? ApprovedDate_DT { get; set; }
        public DateTime? PublicationDate_DT { get; set; }
        public DateTime? ExpirationDate_DT { get; set; }

        public bool MetadataCompleted_FLAG { get; set; }

        public PublicUrlSharingStatus GetPublicUrlSharingStatus()
        {
            if (ApprovedDate_DT.HasValue && ExpirationDate_DT.HasValue && ExpirationDate_DT.Value <= DateTime.UtcNow)
            {
                return PublicUrlSharingStatus.Expired;
            }
            if (PublicationDate_DT.HasValue && PublicationDate_DT.Value <= DateTime.UtcNow)
            {
                return PublicUrlSharingStatus.AccessPublicUrl;
            }
            else if (ApprovedDate_DT.HasValue && ApprovedDate_DT.Value <= DateTime.UtcNow)
            {
                return PublicUrlSharingStatus.PendingPublication;
            }
            else if (SubmittedDate_DT.HasValue && SubmittedDate_DT.Value <= DateTime.UtcNow)
            {
                return PublicUrlSharingStatus.PendingApproval;
            }
            else if (MetadataCompleted_FLAG)
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

            if (IsOpenDataRequest_FLAG && this is OpenDataSharedFile file)
            {
                prefix = OPEN_DATA_SHARING_STATUS_LOCALIZATION_PREFIX;
                var status = file.GetOpenDataSharingStatus();
                statusCode = status.ToString();
            }
            else
            {
                prefix = PUBLIC_URL_SHARING_STATUS_LOCALIZATION_PREFIX;
                var status = GetPublicUrlSharingStatus();
                statusCode = status.ToString();
            }

            return $"{prefix}.{statusCode}.Title";
        }
    }

    [Table("OpenDataSharedFile")]
    public class OpenDataSharedFile: SharedDataFile
    {
        public int? ApprovalForm_ID { get; set; }
        public string SignedApprovalForm_URL { get; set; }

        public OpenDataSharingStatus GetOpenDataSharingStatus()
        {
            if (PublicationDate_DT.HasValue && PublicationDate_DT.Value <= DateTime.UtcNow)
            {
                return OpenDataSharingStatus.AccessOpenData;
            }
            else if (ApprovedDate_DT.HasValue && ApprovedDate_DT.Value <= DateTime.UtcNow)
            {
                return OpenDataSharingStatus.PendingPublication;
            }
            else if (!string.IsNullOrEmpty(SignedApprovalForm_URL))
            {
                return OpenDataSharingStatus.PendingApproval;
            }
            else if (ApprovalForm_ID.HasValue && ApprovalForm_ID > 0)
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
}