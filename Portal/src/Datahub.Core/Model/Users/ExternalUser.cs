using System.ComponentModel.DataAnnotations;

namespace Datahub.Core.Model.Users
{
    /// <summary>
    /// Represents an external (non-portal) user linked to a <see cref="PortalUser"/>.
    /// Tracks login activity, permissions updates, and related invite requests.
    /// </summary>
    public class ExternalUser
    {
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the GCCF object identifier (OID) associated with the external identity.
        /// A blank OID indicates that the user has not completed the invitation flow or has been deactivated.
        /// </summary>
        public required string ExternalSubject { get; set; }

        /// <summary>
        /// Gets or sets the first name of the person.
        /// </summary>
        public required string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the person.
        /// </summary>
        public required string LastName { get; set; }

        /// <summary>
        /// Gets or sets the optional organization context associated with the entity.
        /// </summary>
        public required string Organization { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the first login event for the external user. This is redundant with PortalUser but kept for historical tracking.
        /// </summary>
        public DateTimeOffset? FirstLoginDateTime { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the creation of this external user record.
        /// </summary>
        public DateTimeOffset? CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the update of this external user record.
        /// </summary>
        public DateTimeOffset? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the most recent login event for the external user. This is redundant with PortalUser but kept for historical tracking.
        /// </summary>
        public DateTimeOffset? LastLoginDateTime { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the external user was deactivated.
        /// </summary>
        public DateTimeOffset? UserDeactivatedAt { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the portal user who deactivated this external user.
        /// </summary>
        public PortalUser? DeactivatedByUser { get; set; }

        /// <summary>
        /// Gets the identifier of the user who deactivated the entity, if applicable.
        /// </summary>
        public int? DeactivatedByUserId { get; internal set; }

        /// <summary>
        /// Gets or sets the navigation property to the portal user who deactivated this external user.
        /// </summary>
        public string? DeactivationReason { get; set; }

        /// <summary>
        /// Gets or sets the collection of invite requests related to this external user.
        /// </summary>
        public ICollection<WorkspaceInvitation> Invitations { get; set; } = new List<WorkspaceInvitation>();

        /// <summary>
        /// Gets or sets the foreign key to the linked <see cref="PortalUser"/> record.
        /// </summary>
        public int PortalUserId { get; set; }

        /// <summary>
        /// Gets or sets the required navigation to the owning <see cref="PortalUser"/>.
        /// </summary>
        public required PortalUser PortalUser { get; set; } = null!;

        /// <summary>
        /// Gets or sets the timestamp for concurrency control.
        /// </summary>
        public byte[]? Timestamp { get; set; }
    }
}
