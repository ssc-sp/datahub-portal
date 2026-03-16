using Datahub.Core.Model.Users;

namespace Datahub.Application.Services.UserManagement;

public interface IExternalUserInvitationService
{
    Task<WorkspaceInvitation> CreateInvitationAsync(
        int externalUserId,
        string projectAcronym,
        string invitedEmail,
        string invitationRationale,
        DateTimeOffset? invitationExpiry = null,
        CancellationToken cancellationToken = default);

    Task<bool> IsInvitationTokenValidAsync(
        Guid invitationToken,
        CancellationToken cancellationToken = default);
}
