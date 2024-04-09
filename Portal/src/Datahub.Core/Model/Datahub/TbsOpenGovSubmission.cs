using System.ComponentModel.DataAnnotations.Schema;
using Datahub.Core.Utils;

namespace Datahub.Core.Model.Datahub
{
    public class TbsOpenGovSubmission : OpenDataSubmission
    {
        public const string DATASET_FILE_TYPE = "Dataset";
        public const string GUIDE_FILE_TYPE = "Guide";
        public const string IMSO_APPROVAL_FILE_TYPE = "ImsoApproval";

        public const string PUBLICATION_METADATA_PROFILE_NAME = "publication";
        public const string RESOURCE_METADATA_PROFILE_NAME = "pub_resource";

        public const string LOCALIZATION_PREFIX = nameof(OpenDataPublishProcessType.TbsOpenGovPublishing);

        public enum ProcessSteps
        {
            AwaitingMetadata = 1,
            AwaitingApprovalCriteria,
            AwaitingFiles,
            CheckingDataQuality,
            Uploading,
            AwaitingRemoteDqCheck,
            AwaitingImsoApproval,
            Publishing,
            Published
        }

        public bool MetadataComplete { get; set; }
        public int? OpenGovCriteriaFormId { get; set; }
        public DateTime? OpenGovCriteriaMetDate { get; set; }
        public bool LocalDQCheckStarted { get; set; }
        public bool LocalDQCheckPassed { get; set; }
        public DateTime? InitialOpenGovSubmissionDate { get; set; }
        public bool OpenGovDQCheckPassed { get; set; }
        public DateTime? ImsoApprovalRequestDate { get; set; }
        public DateTime? ImsoApprovedDate { get; set; }
        public DateTime? OpenGovPublicationDate { get; set; }

        [NotMapped]
        public bool ImsoApproved => Files?.Any(f => f.FilePurpose == IMSO_APPROVAL_FILE_TYPE) ?? false &&
            OpenDataPublishingUtils.IsDateSetAndPassed(ImsoApprovedDate);
        [NotMapped]
        public bool ImsoApprovalRequested => OpenDataPublishingUtils.IsDateSetAndPassed(ImsoApprovalRequestDate);

        public override string LocalizationPrefix => LOCALIZATION_PREFIX;
    }
}
