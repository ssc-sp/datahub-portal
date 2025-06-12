using System.ComponentModel.DataAnnotations.Schema;
using Datahub.Core.Utils;

namespace Datahub.Core.Model.Datahub
{
    /// <summary>
    /// Represents a TBS open government submission, detailing the steps required for publishing open government data.
    /// </summary>
    public class TbsOpenGovSubmission : OpenDataSubmission
    {
        /// <summary>
        /// A file type constant for describing dataset files.
        /// </summary>
        public const string DATASET_FILE_TYPE = "Dataset";

        /// <summary>
        /// A file type constant for describing guide files.
        /// </summary>
        public const string GUIDE_FILE_TYPE = "Guide";

        /// <summary>
        /// A file type constant for describing IMSO approval files.
        /// </summary>
        public const string IMSO_APPROVAL_FILE_TYPE = "ImsoApproval";

        /// <summary>
        /// The name of the publication metadata profile.
        /// </summary>
        public const string PUBLICATION_METADATA_PROFILE_NAME = "publication";

        /// <summary>
        /// The name of the resource metadata profile.
        /// </summary>
        public const string RESOURCE_METADATA_PROFILE_NAME = "pub_resource";

        /// <summary>
        /// The localization prefix for TBS open government publishing.
        /// </summary>
        public const string LOCALIZATION_PREFIX = nameof(OpenDataPublishProcessType.TbsOpenGovPublishing);

        /// <summary>
        /// Specifies the various steps in the TBS open government submission process.
        /// </summary>
        public enum ProcessSteps
        {
            /// <summary>
            /// The submission is awaiting metadata.
            /// </summary>
            AwaitingMetadata = 1,

            /// <summary>
            /// The submission is awaiting open government approval criteria.
            /// </summary>
            AwaitingApprovalCriteria,

            /// <summary>
            /// The submission is awaiting required files.
            /// </summary>
            AwaitingFiles,

            /// <summary>
            /// The submission is undergoing data quality checks locally.
            /// </summary>
            CheckingDataQuality,

            /// <summary>
            /// The submission is uploading files and metadata.
            /// </summary>
            Uploading,

            /// <summary>
            /// The submission is awaiting remote data quality checks.
            /// </summary>
            AwaitingRemoteDqCheck,

            /// <summary>
            /// The submission is awaiting IMSO approval.
            /// </summary>
            AwaitingImsoApproval,

            /// <summary>
            /// The submission is in the process of publishing.
            /// </summary>
            Publishing,

            /// <summary>
            /// The submission has been successfully published.
            /// </summary>
            Published
        }

        /// <summary>
        /// Gets or sets a value indicating whether the submission's metadata is complete.
        /// </summary>
        public bool MetadataComplete { get; set; }

        /// <summary>
        /// Gets or sets the ID of the associated Open Government Criteria Form, if any.
        /// </summary>
        public int? OpenGovCriteriaFormId { get; set; }

        /// <summary>
        /// Gets or sets the date when open government criteria were met.
        /// </summary>
        public DateTime? OpenGovCriteriaMetDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the local data quality check has started.
        /// </summary>
        public bool LocalDQCheckStarted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the local data quality check has passed.
        /// </summary>
        public bool LocalDQCheckPassed { get; set; }

        /// <summary>
        /// Gets or sets the date of the initial open government submission.
        /// </summary>
        public DateTime? InitialOpenGovSubmissionDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the open government data quality check has passed.
        /// </summary>
        public bool OpenGovDQCheckPassed { get; set; }

        /// <summary>
        /// Gets or sets the date when IMSO approval was requested.
        /// </summary>
        public DateTime? ImsoApprovalRequestDate { get; set; }

        /// <summary>
        /// Gets or sets the date when IMSO approval was granted.
        /// </summary>
        public DateTime? ImsoApprovedDate { get; set; }

        /// <summary>
        /// Gets or sets the date when the submission was published to open government.
        /// </summary>
        public DateTime? OpenGovPublicationDate { get; set; }

        /// <summary>
        /// Gets a value indicating whether IMSO approval has been granted, based on linked files and approval date.
        /// </summary>
        [NotMapped]
        public bool ImsoApproved => Files?.Any(f => f.FilePurpose == IMSO_APPROVAL_FILE_TYPE) ?? false &&
            OpenDataPublishingUtils.IsDateSetAndPassed(ImsoApprovedDate);

        /// <summary>
        /// Gets a value indicating whether IMSO approval has been requested, based on the request date.
        /// </summary>
        [NotMapped]
        public bool ImsoApprovalRequested => OpenDataPublishingUtils.IsDateSetAndPassed(ImsoApprovalRequestDate);

        /// <summary>
        /// Gets the localization prefix for TBS open government publishing.
        /// </summary>
        public override string LocalizationPrefix => LOCALIZATION_PREFIX;
    }
}
