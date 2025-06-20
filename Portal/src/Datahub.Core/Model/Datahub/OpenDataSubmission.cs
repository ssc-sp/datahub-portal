using System.ComponentModel.DataAnnotations.Schema;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Projects;

namespace Datahub.Core.Model.Datahub
{
    /// <summary>
    /// Defines the types of open data publishing processes.
    /// </summary>
    public enum OpenDataPublishProcessType
    {
        /// <summary>
        /// Represents the Treasury Board Secretariat Open Government publishing process.
        /// </summary>
        TbsOpenGovPublishing = 1,
    }

    /// <summary>
    /// Represents basic information for an open data submission.
    /// </summary>
    /// <param name="DatasetTitle">The title of the dataset.</param>
    /// <param name="ProcessType">The type of publishing process.</param>
    /// <param name="ProjectId">The identifier of the associated workspace.</param>
    public record OpenDataSubmissionBasicInfo(string DatasetTitle, OpenDataPublishProcessType ProcessType, int ProjectId);

    /// <summary>
    /// Represents a submission for publishing open data, associated with a specific workspace.
    /// </summary>
    public abstract class OpenDataSubmission
    {
        /// <summary>
        /// Gets or sets the unique identifier for the open data submission.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets a unique string identifier for the submission.
        /// </summary>
        public string UniqueId { get; set; }

        /// <summary>
        /// Gets or sets the type of open data publishing process.
        /// </summary>
        public OpenDataPublishProcessType ProcessType { get; set; }

        /// <summary>
        /// Gets or sets the title of the dataset being submitted.
        /// </summary>
        public string DatasetTitle { get; set; }

        /// <summary>
        /// Gets or sets the current status of the submission.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the submission is open for attaching files.
        /// </summary>
        public bool OpenForAttachingFiles { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who requested the submission.
        /// </summary>
        public int RequestingUserId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the submission was requested.
        /// </summary>
        public DateTime RequestDate { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the workspace associated with this submission.
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// Gets the localization prefix for UI elements related to this submission type.
        /// </summary>
        [NotMapped]
        public abstract string LocalizationPrefix { get; }

        /// <summary>
        /// Gets or sets the list of files included in this submission.
        /// </summary>
        public IList<OpenDataPublishFile> Files { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the associated workspace.
        /// </summary>
        public Datahub_Project Project { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the user who requested the submission.
        /// </summary>
        public PortalUser RequestingUser { get; set; }
    }
}
