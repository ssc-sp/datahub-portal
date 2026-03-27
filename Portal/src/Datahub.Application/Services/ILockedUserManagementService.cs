using Datahub.Core.Model.Users;

namespace Datahub.Application.Services;

/// <summary>
/// Service for managing user locks and unlock operations
/// </summary>
public interface ILockedUserManagementService
{
    /// <summary>
    /// Locks a user globally
    /// </summary>
    /// <param name="portalUserId">The user to lock</param>
    /// <param name="reason">Reason for locking</param>
    /// <param name="evidenceUrl">URL to virus scan evidence</param>
    /// <param name="performedByUserId">Admin user performing the action</param>
    /// <returns>The created lock event</returns>
    Task<ExternalUserLockAuditEvent> LockUserAsync(int portalUserId, string reason, string? evidenceUrl, int performedByUserId);

    /// <summary>
    /// Unlocks a user globally
    /// </summary>
    /// <param name="portalUserId">The user to unlock</param>
    /// <param name="notes">Notes about why user was unlocked</param>
    /// <param name="performedByUserId">Admin user performing the action</param>
    /// <returns>The created unlock event</returns>
    Task<ExternalUserLockAuditEvent> UnlockUserAsync(int portalUserId, string? notes, int performedByUserId);

    /// <summary>
    /// Records evidence upload for a locked user
    /// </summary>
    /// <param name="portalUserId">The locked user</param>
    /// <param name="evidenceUrl">URL to the uploaded evidence</param>
    /// <param name="uploadedByUserId">Admin user who uploaded evidence</param>
    /// <returns>The created evidence upload event</returns>
    Task<ExternalUserLockAuditEvent> RecordEvidenceUploadAsync(int portalUserId, string evidenceUrl, int uploadedByUserId);

    /// <summary>
    /// Checks if a user is currently locked
    /// </summary>
    /// <param name="portalUserId">The user to check</param>
    /// <returns>True if user is locked</returns>
    Task<bool> IsUserLockedAsync(int portalUserId);

    /// <summary>
    /// Gets the lock status for a user
    /// </summary>
    /// <param name="portalUserId">The user</param>
    /// <returns>Lock details if locked, null if not locked</returns>
    Task<UserLockStatus?> GetUserLockStatusAsync(int portalUserId);

    /// <summary>
    /// Gets all locked users across all workspaces
    /// </summary>
    /// <returns>List of currently locked users with their lock details</returns>
    Task<List<UserLockStatus>> GetAllLockedUsersAsync();

    /// <summary>
    /// Gets the lock history for a specific user
    /// </summary>
    /// <param name="portalUserId">The user</param>
    /// <returns>Complete lock/unlock history</returns>
    Task<List<ExternalUserLockAuditEvent>> GetUserLockHistoryAsync(int portalUserId);
}

/// <summary>
/// Represents the current lock status of a user
/// </summary>
public class UserLockStatus
{
    public int PortalUserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LockedDate { get; set; }
    public string? LockReason { get; set; }
    public string? LatestEvidenceUrl { get; set; }
    public DateTime? LatestEvidenceDate { get; set; }
    public int LockEventCount { get; set; }
}
