using System.ComponentModel.DataAnnotations;
using Datahub.Core.Model.Achievements;
using MudBlazor.Forms;

namespace Datahub.Core.Model.Projects
{
    /// <summary>
    /// Represents a user record associated with a FSDH workspace.
    /// </summary>
    public class Datahub_Project_User
    {
        /// <summary>
        /// Gets or sets the unique identifier for the workspace user record.
        /// </summary>
        [AeFormIgnore]
        [Key]
        public int ProjectUser_ID { get; set; }

        /// <summary>
        /// Gets or sets the workspace user identifier.
        /// </summary>
        public int? PortalUserId { get; set; }

        /// <summary>
        /// Gets or sets the identifier for the user who approved this record.
        /// </summary>
        public int? ApprovedPortalUserId { get; set; }

        /// <summary>
        /// Gets or sets the role identifier associated with this user record.
        /// </summary>
        public int? RoleId { get; set; }

        /// <summary>
        /// Gets or sets the workspace identifier.
        /// </summary>
        public int Project_ID { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the record was approved.
        /// </summary>
        public DateTime? Approved_DT { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is a data steward.
        /// </summary>
        public bool IsDataSteward { get; set; }

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the associated workspace.
        /// </summary>
        public Datahub_Project Project { get; set; }

        /// <summary>
        /// Gets or sets the associated role.
        /// </summary>
        public Project_Role Role { get; set; }

        /// <summary>
        /// Gets or sets the portal user. This includes email and graph GUID.
        /// </summary>
        public PortalUser PortalUser { get; set; }

        /// <summary>
        /// Gets or sets the portal user who approved this record.
        /// </summary>
        public PortalUser ApprovedPortalUser { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets the timestamp for concurrency control.
        /// </summary>
        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}