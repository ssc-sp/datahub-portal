using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Users;

namespace Datahub.Application.Services.UserManagement;

public interface IExternalUserInvitationService
{

    /// <summary>
    /// Creates a new workspace invitation for an external user to join a project with a specified role.
    /// </summary>
    /// <remarks>The invitation will be sent to the specified email address and grants access to the project
    /// with the assigned role. If an expiry is provided, the invitation will be invalid after the specified date and
    /// time.</remarks>
    /// <param name="externalUserId">The unique identifier of the external user to be invited. Must be a valid user ID.</param>
    /// <param name="projectAcronym">The acronym of the project to which the user is being invited. Cannot be null or empty.</param>
    /// <param name="invitedEmail">The email address of the user receiving the invitation. Cannot be null or empty.</param>
    /// <param name="invitationRationale">The rationale or reason for the invitation. Used to provide context to the recipient.</param>
    /// <param name="projectRoleId">The identifier of the project role assigned to the invited user. Must correspond to a valid project role.</param>
    /// <param name="invitationExpiry">The optional expiration date and time for the invitation. If null, the invitation does not expire.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. Allows the operation to be cancelled.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created workspace invitation.</returns>
    Task<WorkspaceInvitation> CreateInvitationAsync(
        int externalUserId,
        string projectAcronym,
        string invitedEmail,
        string invitationRationale,
        int projectRoleId,
        DateTimeOffset? invitationExpiry = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether the specified invitation token is valid and can be used for registration.
    /// </summary>
    /// <param name="invitationToken">The unique identifier of the invitation token to validate.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the validation operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the invitation
    /// token is valid; otherwise, <see langword="false"/>.</returns>
    Task<bool> IsInvitationTokenValidAsync(
        Guid invitationToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels an existing invitation, making it invalid and preventing the invited user from accessing the project.
    /// </summary>
    /// <param name="requestId">The unique identifier of the invitation request to be cancelled.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the cancelled workspace invitation, if found.</returns>
    Task<WorkspaceInvitation?> CancelInvitationAsync(
        int requestId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resends an existing invitation to the same or a different email address.
    /// </summary>
    /// <param name="externalUserId">The unique identifier of the external user to be invited again.</param>
    /// <param name="projectAcronym">The acronym of the project to which the user is being invited. Cannot be null or empty.</param>
    /// <param name="invitedEmail">The new email address of the user receiving the invitation. Cannot be null or empty.</param>
    /// <param name="projectRoleId">The identifier of the project role assigned to the invited user. Must correspond to a valid project role.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. Allows the operation to be cancelled.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the resent workspace invitation.</returns>
    Task<WorkspaceInvitation> ResendInvitationAsync(
        int externalUserId,
        string projectAcronym,
        string invitedEmail,
        int projectRoleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates an invitation token and code, then completes the invitation by linking the authenticated
    /// external user identity and assigning the requested workspace role.
    /// </summary>
    /// <param name="invitationToken">The invitation token from the invitation URL.</param>
    /// <param name="invitationCode">The invitation validation code entered by the user.</param>
    /// <param name="externalUserOid">The authenticated external user's OID/subject identifier.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><see langword="true"/> when the invitation was completed; otherwise <see langword="false"/>.</returns>
    Task<bool> CompleteInvitationAsync(
        Guid invitationToken,
        string invitationCode,
        string externalUserOid,
        CancellationToken cancellationToken = default);
}
