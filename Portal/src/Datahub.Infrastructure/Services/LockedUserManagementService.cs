using Datahub.Application.Services;

namespace Datahub.Infrastructure.Services;

/// <summary>
/// Service for managing user locks and unlock operations
/// Note: This is a stub implementation pending full database schema integration
/// </summary>
public class LockedUserManagementService : ILockedUserManagementService
{
    public async Task LockUserAsync(int portalUserId, int? workspaceId, string reason, string? evidenceUrl, int performedByUserId)
    {
        // TODO: Implement actual lock logic when UserWorkspaceLock DB table is integrated
        await Task.CompletedTask;
    }

    public async Task UnlockUserAsync(int portalUserId, int? workspaceId, int performedByUserId)
    {
        // TODO: Implement actual unlock logic when UserWorkspaceLock DB table is integrated
        await Task.CompletedTask;
    }

    public async Task RecordEvidenceUploadAsync(int portalUserId, int workspaceId, string evidenceUrl)
    {
        // TODO: Implement evidence upload recording when UserWorkspaceLock DB table is integrated
        await Task.CompletedTask;
    }

    public Task<bool> IsUserLockedAsync(int portalUserId, int? workspaceId = null)
    {
        // TODO: Implement user lock check when UserWorkspaceLock DB table is integrated
        return Task.FromResult(false);
    }

    public Task<UserLockStatus?> GetUserLockStatusAsync(int portalUserId, int? workspaceId = null)
    {
        // TODO: Implement lock status retrieval when UserWorkspaceLock DB table is integrated
        return Task.FromResult<UserLockStatus?>(null);
    }

    public Task<List<UserLockStatus>> GetLockedUsersInWorkspaceAsync(int workspaceId)
    {
        // TODO: Implement locked users retrieval when UserWorkspaceLock DB table is integrated
        return Task.FromResult(new List<UserLockStatus>());
    }
}
