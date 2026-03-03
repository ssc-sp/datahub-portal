using System;
using Datahub.Application.Services;
using Datahub.Application.Services.Notification;
using Datahub.Core.Model.Context;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Infrastructure.Services.Notification;

public class UserAccessNotificationService : IUserAccessNotificationService
{
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
    private readonly IGCNotifyService _gcNotifyService;

    public UserAccessNotificationService(
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
        IGCNotifyService gcNotifyService)
    {
        _dbContextFactory = dbContextFactory;
        _gcNotifyService = gcNotifyService;
    }

    public async Task NotifyAccessRegrantedAsync(UserLockStatus lockStatus, string unlockedBy)
    {
        if (lockStatus == null)
        {
            return;
        }

        var unlockedByDisplay = string.IsNullOrWhiteSpace(unlockedBy) ? "DataHub administrator" : unlockedBy;

        var userEmail = lockStatus.UserEmail;
        if (string.IsNullOrWhiteSpace(userEmail))
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();
            userEmail = await ctx.PortalUsers
                .Where(u => u.Id == lockStatus.PortalUserId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();
        }

        if (string.IsNullOrWhiteSpace(userEmail))
        {
            return;
        }

        var recipientEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            userEmail
        };

        foreach (var email in recipientEmails)
        {
            await _gcNotifyService.SendUserAccessRegrantedNotification(
                email,
                lockStatus.UserName ?? userEmail,
                "all workspaces",
                unlockedByDisplay);
        }
    }
}