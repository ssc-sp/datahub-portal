using System.ComponentModel.DataAnnotations;
using Datahub.Core.Model.Achievements;

namespace Datahub.Core.Model.Projects
{
    /// <summary>
    /// Represents a questionnaire to be filled out before deleting a project.
    /// </summary>
    public class Project_Delete_Questionnaire
    {
        /// <summary>
        /// Gets or sets the unique identifier for the questionnaire.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the workspace is no longer required.
        /// </summary>
        public bool IsWorkspaceNotRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the data has been migrated.
        /// </summary>
        public bool IsDataMigrated { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the data is not subject to litigation.
        /// </summary>
        public bool IsDataNotSubjectToLitigation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the data does not have archival value.
        /// </summary>
        public bool DoesDataNotHaveArchivalValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the deletion of the project is confirmed.
        /// </summary>
        public bool IsDeletionConfirmed { get; set; }

        /// <summary>
        /// Gets or sets the date when the project was deleted.
        /// </summary>
        public DateTime? DeletedDate { get; set; }

        /// <summary>
        /// Gets or sets the project associated with this questionnaire.
        /// </summary>
        public Datahub_Project Project { get; set; }

        /// <summary>
        /// Gets or sets the user who deleted the project.
        /// </summary>
        public PortalUser DeletedBy { get; set; }
    }
}
