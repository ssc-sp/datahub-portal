using System.ComponentModel.DataAnnotations;
using MudBlazor.Forms;
using Datahub.Core.Model.Users;

namespace Datahub.Core.Model.Projects
{
    /// <summary>
    /// Represents a user record associated with a FSDH workspace.
    /// </summary>
    public class UserRoleLinks
    {
        /// <summary>
        /// Gets or sets the unique identifier for the workspace user record.
        /// </summary>
        [AeFormIgnore]
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

        /// <summary>
        /// Gets or sets optional notes or comments about collaboration objectives with external user
        /// </summary>
        public string? ExternalUserNotes { get; set; }

        /// <summary>
        /// Gets or sets the associated workspace.
        /// </summary>
        public Datahub_Project Project { get; set; } = null!;

        /// <summary>
        /// Gets or sets the associated role.
        /// </summary>
        public Project_Role Role { get; set; } = null!;

        /// <summary>
        /// Gets or sets the portal user. This includes email and graph GUID.
        /// </summary>
        public PortalUser PortalUser { get; set; } = null!;

        /// <summary>
        /// Gets or sets the portal user who approved this record.
        /// </summary>
        public PortalUser? ApprovedPortalUser { get; set; }

        /// <summary>
        /// Gets or sets the timestamp for concurrency control.
        /// </summary>
        public byte[]? Timestamp { get; set; }
    }
}