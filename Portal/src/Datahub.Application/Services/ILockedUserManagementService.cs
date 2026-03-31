namespace Datahub.Application.Services;

/// <summary>
/// Service for managing user locks and unlock operations
/// </summary>
public interface ILockedUserManagementService
{
    /// <summary>
    /// Locks a user in a specific workspace or globally
    /// </summary>
    Task LockUserAsync(int portalUserId, int? workspaceId, string reason, string? evidenceUrl, int performedByUserId);

    /// <summary>
    /// Unlock a user globally or in a specific workspace
    /// </summary>
    Task UnlockUserAsync(int portalUserId, int? workspaceId, int performedByUserId);

    /// <summary>
    /// Record evidence upload for a locked user
    /// </summary>
    Task RecordEvidenceUploadAsync(int portalUserId, int workspaceId, string evidenceUrl);

    /// <summary>
    /// Check if a user is locked in a workspace or globally
    /// </summary>
    Task<bool> IsUserLockedAsync(int portalUserId, int? workspaceId = null);

    /// <summary>
    /// Get the lock status of a user
    /// </summary>
    Task<UserLockStatus?> GetUserLockStatusAsync(int portalUserId, int? workspaceId = null);

    /// <summary>
    /// Get all locked users in a workspace
    /// </summary>
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
