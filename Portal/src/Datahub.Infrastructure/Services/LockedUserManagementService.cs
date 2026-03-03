using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Users;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Infrastructure.Services;

/// <summary>
/// Implementation of user lock management service
/// </summary>
public class LockedUserManagementService : ILockedUserManagementService
{
    private readonly IDbContextFactory<DatahubProjectDBContext> _contextFactory;

    public LockedUserManagementService(IDbContextFactory<DatahubProjectDBContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<UserWorkspaceLock> LockUserAsync(int portalUserId, string reason, string? evidenceUrl, int performedByUserId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var lockEvent = new UserWorkspaceLock
        {
            PortalUserId = portalUserId,
            EventType = LockEventType.Locked,
            EventDate = DateTime.UtcNow,
            Reason = reason,
            EvidenceUrl = evidenceUrl,
            PerformedByUserId = performedByUserId
        };

        context.UserWorkspaceLocks.Add(lockEvent);
        await context.SaveChangesAsync();

        return lockEvent;
    }

    public async Task<UserWorkspaceLock> UnlockUserAsync(int portalUserId, string? notes, int performedByUserId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var unlockEvent = new UserWorkspaceLock
        {
            PortalUserId = portalUserId,
            EventType = LockEventType.Unlocked,
            EventDate = DateTime.UtcNow,
            Notes = notes,
            PerformedByUserId = performedByUserId
        };

        context.UserWorkspaceLocks.Add(unlockEvent);
        await context.SaveChangesAsync();

        return unlockEvent;
    }

    public async Task<UserWorkspaceLock> RecordEvidenceUploadAsync(int portalUserId, string evidenceUrl, int uploadedByUserId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var evidenceEvent = new UserWorkspaceLock
        {
            PortalUserId = portalUserId,
            EventType = LockEventType.EvidenceUploaded,
            EventDate = DateTime.UtcNow,
            EvidenceUrl = evidenceUrl,
            PerformedByUserId = uploadedByUserId
        };

        context.UserWorkspaceLocks.Add(evidenceEvent);
        await context.SaveChangesAsync();

        return evidenceEvent;
    }

    public async Task<bool> IsUserLockedAsync(int portalUserId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Get the most recent event for this user
        var latestEvent = await context.UserWorkspaceLocks
            .Where(l => l.PortalUserId == portalUserId)
            .OrderByDescending(l => l.EventDate)
            .FirstOrDefaultAsync();

        return latestEvent != null && latestEvent.EventType == LockEventType.Locked;
    }

    public async Task<UserLockStatus?> GetUserLockStatusAsync(int portalUserId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var events = await context.UserWorkspaceLocks
            .Include(l => l.User)
            .Where(l => l.PortalUserId == portalUserId)
            .OrderByDescending(l => l.EventDate)
            .ToListAsync();

        if (!events.Any())
            return null;

        var latestEvent = events.First();
        var isLocked = latestEvent.EventType == LockEventType.Locked;

        if (!isLocked)
            return null;

        var lockEvent = events.FirstOrDefault(e => e.EventType == LockEventType.Locked);
        var latestEvidence = events.FirstOrDefault(e => !string.IsNullOrEmpty(e.EvidenceUrl));

        return new UserLockStatus
        {
            PortalUserId = portalUserId,
            UserName = latestEvent.User?.DisplayName,
            UserEmail = latestEvent.User?.Email,
            IsLocked = true,
            LockedDate = lockEvent?.EventDate,
            LockReason = lockEvent?.Reason,
            LatestEvidenceUrl = latestEvidence?.EvidenceUrl,
            LatestEvidenceDate = latestEvidence?.EventDate,
            LockEventCount = events.Count
        };
    }

    public async Task<List<UserLockStatus>> GetAllLockedUsersAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Get all users who have lock events
        var userIds = await context.UserWorkspaceLocks
            .Select(l => l.PortalUserId)
            .Distinct()
            .ToListAsync();

        var lockedUsers = new List<UserLockStatus>();

        foreach (var userId in userIds)
        {
            var status = await GetUserLockStatusAsync(userId);
            if (status != null && status.IsLocked)
            {
                lockedUsers.Add(status);
            }
        }

        return lockedUsers;
    }

    public async Task<List<UserWorkspaceLock>> GetUserLockHistoryAsync(int portalUserId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.UserWorkspaceLocks
            .Include(l => l.User)
            .Include(l => l.PerformedByUser)
            .Where(l => l.PortalUserId == portalUserId);

        return await query
            .OrderByDescending(l => l.EventDate)
            .ToListAsync();
    }
}
