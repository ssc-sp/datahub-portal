using Datahub.Core.Model.Users;

namespace Datahub.Application.Services;

/// <summary>
/// Service for managing user locks and unlock operations
/// </summary>
public interface ILockedUserManagementService
{
    /// <summary>
    /// Locks a user in a specific workspace or globally
    /// </summary>
    /// <param name="portalUserId">The user to lock</param>
    /// <param name="workspaceId">The workspace ID, or null for global lock</param>
    /// <param name="reason">Reason for locking</param>
    /// <param name="evidenceUrl">URL to virus scan evidence</param>
    /// <param name="performedByUserId">Admin user performing the action</param>
    /// <returns>The created lock event</returns>
    Task<UserWorkspaceLock> LockUserAsync(int portalUserId, int? workspaceId, string reason, string? evidenceUrl, int performedByUserId);

    /// <summary>
    /// Unlocks a user in a specific workspace or globally
    /// </summary>
    /// <param name="portalUserId">The user to unlock</param>
    /// <param name="workspaceId">The workspace ID, or null for global unlock</param>
    /// <param name="notes">Notes about why user was unlocked</param>
    /// <param name="performedByUserId">Admin user performing the action</param>
    /// <returns>The created unlock event</returns>
    Task<UserWorkspaceLock> UnlockUserAsync(int portalUserId, int? workspaceId, string? notes, int performedByUserId);

    /// <summary>
    /// Records evidence upload for a locked user
    /// </summary>
    /// <param name="portalUserId">The locked user</param>
    /// <param name="workspaceId">The workspace ID, or null for global</param>
    /// <param name="evidenceUrl">URL to the uploaded evidence</param>
    /// <param name="uploadedByUserId">Admin user who uploaded evidence</param>
    /// <returns>The created evidence upload event</returns>
    Task<UserWorkspaceLock> RecordEvidenceUploadAsync(int portalUserId, int? workspaceId, string evidenceUrl, int uploadedByUserId);

    /// <summary>
    /// Checks if a user is currently locked in a workspace
    /// </summary>
    /// <param name="portalUserId">The user to check</param>
    /// <param name="workspaceId">The workspace ID, or null to check global lock</param>
    /// <returns>True if user is locked</returns>
    Task<bool> IsUserLockedAsync(int portalUserId, int? workspaceId);

    /// <summary>
    /// Gets the lock status for a user in a specific workspace
    /// </summary>
    /// <param name="portalUserId">The user</param>
    /// <param name="workspaceId">The workspace ID, or null for global</param>
    /// <returns>Lock details if locked, null if not locked</returns>
    Task<UserLockStatus?> GetUserLockStatusAsync(int portalUserId, int? workspaceId);

    /// <summary>
    /// Gets all locked users across all workspaces
    /// </summary>
    /// <returns>List of currently locked users with their lock details</returns>
    Task<List<UserLockStatus>> GetAllLockedUsersAsync();

    /// <summary>
    /// Gets the lock history for a specific user
    /// </summary>
    /// <param name="portalUserId">The user</param>
    /// <param name="workspaceId">Optional workspace ID to filter by</param>
    /// <returns>Complete lock/unlock history</returns>
    Task<List<UserWorkspaceLock>> GetUserLockHistoryAsync(int portalUserId, int? workspaceId = null);

    /// <summary>
    /// Gets all locked users in a specific workspace
    /// </summary>
    /// <param name="workspaceId">The workspace ID</param>
    /// <returns>List of locked users in the workspace</returns>
    Task<List<UserLockStatus>> GetLockedUsersInWorkspaceAsync(int workspaceId);
}

/// <summary>
/// Represents the current lock status of a user
/// </summary>
public class UserLockStatus
{
    public int PortalUserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public int? WorkspaceId { get; set; }
    public string? WorkspaceAcronym { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LockedDate { get; set; }
    public string? LockReason { get; set; }
    public string? LatestEvidenceUrl { get; set; }
    public DateTime? LatestEvidenceDate { get; set; }
    public int LockEventCount { get; set; }
}
