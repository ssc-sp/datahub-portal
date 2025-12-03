using System.ComponentModel.DataAnnotations;

namespace Datahub.Core.Model.Users
{
    /// <summary>
    /// Represents an external (non-portal) user linked to a <see cref="PortalUser"/>.
    /// Tracks login activity, permissions updates, and related invite requests.
    /// </summary>
    public class ExternalUser
    {
        /// <summary>
        /// Gets or sets the mandatory unique identifier for the external user.
        /// </summary>
        public Guid ExternalUserID { get; set; }

        /// <summary>
        /// Gets or sets the GCCF object identifier (OID) associated with the external identity.
        /// A blank OID indicates that the user has not completed the invitation flow or has been deactivated.
        /// </summary>
        public Guid? OID { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the first login event for the external user.
        /// </summary>
        public DateTime? FirstLogin_DT { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the most recent login event for the external user.
        /// </summary>
        public DateTime? LastLogin_DT { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the external user was deactivated.
        /// </summary>
        public DateTimeOffset? DeactivatedDate_DT { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the portal user who deactivated this external user.
        /// </summary>
        public PortalUser? DeactivatedByUser { get; set; }

        /// <summary>
        /// Gets or sets the collection of invite requests related to this external user.
        /// </summary>
        public ICollection<ExternalUserInvite> Invitations { get; set; } = new List<ExternalUserInvite>();

        /// <summary>
        /// Gets or sets the foreign key to the linked <see cref="PortalUser"/> record.
        /// </summary>
        public int PortalUserId { get; set; }

        /// <summary>
        /// Gets or sets the required navigation to the owning <see cref="PortalUser"/>.
        /// </summary>
        public required PortalUser PortalUser { get; set; } = null!;
    }
}