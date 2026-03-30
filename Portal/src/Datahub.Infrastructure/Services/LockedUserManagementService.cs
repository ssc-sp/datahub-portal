using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Infrastructure.Services;

/// <summary>
/// Implementation of user lock management service
/// </summary>
public class LockedUserManagementService : ILockedUserManagementService
{
    private readonly IDbContextFactory<DatahubProjectDBContext> _contextFactory;
    private static readonly TimeSpan LockOffset = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan DefaultUnlockDuration = TimeSpan.FromDays(365);

    public LockedUserManagementService(IDbContextFactory<DatahubProjectDBContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<ExternalUserLockAuditEvent> LockUserAsync(int portalUserId, string reason, string? evidenceUrl, int performedByUserId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var externalUser = await context.ExternalUsers
            .FirstOrDefaultAsync(e => e.PortalUserId == portalUserId);

        DateTimeOffset? previousExpiryDate = null;
        DateTimeOffset? appliedExpiryDate = null;

        if (externalUser != null)
        {
            previousExpiryDate = externalUser.UserExpiryDate;
            appliedExpiryDate = DateTimeOffset.UtcNow.Subtract(LockOffset);
            externalUser.UserExpiryDate = appliedExpiryDate.Value;
        }

        var lockEvent = new ExternalUserLockAuditEvent
        {
            PortalUserId = portalUserId,
            EventType = ExternalUserLockEventType.Locked,
            EventDate = DateTime.UtcNow,
            Reason = reason,
            EvidenceUrl = evidenceUrl,
            PreviousExpiryDate = previousExpiryDate,
            AppliedExpiryDate = appliedExpiryDate,
            PerformedByUserId = performedByUserId
        };

        context.ExternalUserLockAuditEvents.Add(lockEvent);
        await context.SaveChangesAsync();

        return lockEvent;
    }

    public async Task<ExternalUserLockAuditEvent> UnlockUserAsync(int portalUserId, string? notes, int performedByUserId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var externalUser = await context.ExternalUsers
            .FirstOrDefaultAsync(e => e.PortalUserId == portalUserId);

        DateTimeOffset? previousExpiryDate = null;
        DateTimeOffset? appliedExpiryDate = null;

        if (externalUser != null)
        {
            previousExpiryDate = externalUser.UserExpiryDate;

            var latestLockEvent = await context.ExternalUserLockAuditEvents
                .Where(l => l.PortalUserId == portalUserId && l.EventType == ExternalUserLockEventType.Locked)
                .OrderByDescending(l => l.EventDate)
                .FirstOrDefaultAsync();

            var restoredExpiry = latestLockEvent?.PreviousExpiryDate;
            appliedExpiryDate = restoredExpiry.HasValue && restoredExpiry.Value > DateTimeOffset.UtcNow
                ? restoredExpiry.Value
                : DateTimeOffset.UtcNow.Add(DefaultUnlockDuration);

            externalUser.UserExpiryDate = appliedExpiryDate.Value;
        }

        var unlockEvent = new ExternalUserLockAuditEvent
        {
            PortalUserId = portalUserId,
            EventType = ExternalUserLockEventType.Unlocked,
            EventDate = DateTime.UtcNow,
            Notes = notes,
            PreviousExpiryDate = previousExpiryDate,
            AppliedExpiryDate = appliedExpiryDate,
            PerformedByUserId = performedByUserId
        };

        context.ExternalUserLockAuditEvents.Add(unlockEvent);
        await context.SaveChangesAsync();

        return unlockEvent;
    }

    public async Task<ExternalUserLockAuditEvent> RecordEvidenceUploadAsync(int portalUserId, string evidenceUrl, int uploadedByUserId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var evidenceEvent = new ExternalUserLockAuditEvent
        {
            PortalUserId = portalUserId,
            EventType = ExternalUserLockEventType.EvidenceUploaded,
            EventDate = DateTime.UtcNow,
            EvidenceUrl = evidenceUrl,
            PerformedByUserId = uploadedByUserId
        };

        context.ExternalUserLockAuditEvents.Add(evidenceEvent);
        await context.SaveChangesAsync();

        return evidenceEvent;
    }

    public async Task<bool> IsUserLockedAsync(int portalUserId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            // Get the most recent event for this user
            var latestEvent = await context.ExternalUserLockAuditEvents
                .Where(l => l.PortalUserId == portalUserId)
                .OrderByDescending(l => l.EventDate)
                .FirstOrDefaultAsync();

            return latestEvent != null && latestEvent.EventType == ExternalUserLockEventType.Locked;
        }
        catch (SqlException ex) when (IsMissingExternalUserLockAuditTable(ex))
        {
            return false;
        }
    }

    public async Task<UserLockStatus?> GetUserLockStatusAsync(int portalUserId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var events = await context.ExternalUserLockAuditEvents
                .Include(l => l.User)
                .Where(l => l.PortalUserId == portalUserId)
                .OrderByDescending(l => l.EventDate)
                .ToListAsync();

            if (!events.Any())
                return null;

            var latestEvent = events.First();
            var isLocked = latestEvent.EventType == ExternalUserLockEventType.Locked;

            if (!isLocked)
                return null;

            var lockEvent = events.FirstOrDefault(e => e.EventType == ExternalUserLockEventType.Locked);
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
        catch (SqlException ex) when (IsMissingExternalUserLockAuditTable(ex))
        {
            return null;
        }
    }

    public async Task<List<UserLockStatus>> GetAllLockedUsersAsync()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            // Get all users who have lock events
            var userIds = await context.ExternalUserLockAuditEvents
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
        catch (SqlException ex) when (IsMissingExternalUserLockAuditTable(ex))
        {
            return new List<UserLockStatus>();
        }
    }

    public async Task<List<ExternalUserLockAuditEvent>> GetUserLockHistoryAsync(int portalUserId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var query = context.ExternalUserLockAuditEvents
                .Include(l => l.User)
                .Include(l => l.PerformedByUser)
                .Where(l => l.PortalUserId == portalUserId);

            return await query
                .OrderByDescending(l => l.EventDate)
                .ToListAsync();
        }
        catch (SqlException ex) when (IsMissingExternalUserLockAuditTable(ex))
        {
            return new List<ExternalUserLockAuditEvent>();
        }
    }

    private static bool IsMissingExternalUserLockAuditTable(SqlException exception)
    {
        return exception.Number == 208 &&
               exception.Message.Contains("ExternalUserLockAuditEvents", StringComparison.OrdinalIgnoreCase);
    }
}
