using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Datahub.Core.Model.Projects;

namespace Datahub.Core.Model.Users
{
    /// <summary>
    /// Represents an invite request for an external user, identified by their OID.
    /// Captures when the request was made and its content/payload.
    /// </summary>
    public class WorkspaceInvitation
    {
        /// <summary>
        /// Gets or sets the unique identifier for the invite request.
        /// </summary>
        public int RequestID { get; set; }

        /// <summary>
        /// Gets or sets the navigation to the related external user.
        /// </summary>
        public ExternalUser User { get; set; } = null!;

        /// <summary>
        /// Gets or sets the workspace (project) this invite targets.
        /// </summary>
        public Datahub_Project Project { get; set; } = null!;

        /// <summary>
        /// Gets or sets the invitation token associated with the invite request.
        /// </summary>
        public required Guid InvitationToken { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the email address of the invited user.
        /// </summary>
        public required string InvitedEmail { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating the expiration timestamp of the invitation token.
        /// </summary>
        public DateTimeOffset InvitationExpiry { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the invitation token has been accepted.
        /// </summary>
        public DateTimeOffset? InvitationTokenAccepted { get; set; }

        /// <summary>
        /// Gets or sets the invitation code associated with the invite request.
        /// </summary>
        public string InvitationCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the invitation code has been accepted.
        /// </summary>
        public DateTimeOffset? InvitationCodeAccepted { get; set; }

        /// <summary>
        /// Gets or sets the timestamp for when the invite request was created.
        /// </summary>
        public DateTimeOffset Request_DT { get; set; }
    }
}
