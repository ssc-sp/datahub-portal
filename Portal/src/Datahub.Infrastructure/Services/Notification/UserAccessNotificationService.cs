using System;
using Datahub.Application.Services;
using Datahub.Application.Services.Notification;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
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

    public async Task NotifyAccessRegrantedAsync(UserLockStatus lockStatus)
    {
        if (lockStatus == null)
        {
            return;
        }

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

        var workspaceName = string.IsNullOrWhiteSpace(lockStatus.WorkspaceAcronym)
            ? "all workspaces"
            : lockStatus.WorkspaceAcronym;

        var recipientEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            userEmail
        };

        if (lockStatus.WorkspaceId.HasValue)
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();
            var adminEmails = await ctx.UserRolesLinks
                .Where(u => u.Project_ID == lockStatus.WorkspaceId.Value &&
                            (u.RoleId == (int)Project_Role.RoleNames.Admin ||
                             u.RoleId == (int)Project_Role.RoleNames.WorkspaceLead))
                .Select(u => u.PortalUser.Email)
                .ToListAsync();

            foreach (var email in adminEmails)
            {
                if (!string.IsNullOrWhiteSpace(email))
                {
                    recipientEmails.Add(email);
                }
            }
        }

        foreach (var email in recipientEmails)
        {
            await _gcNotifyService.SendUserAccessRegrantedNotification(
                email,
                lockStatus.UserName ?? userEmail,
                workspaceName);
        }
    }
}