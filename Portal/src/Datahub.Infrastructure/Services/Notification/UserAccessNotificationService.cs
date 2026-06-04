using System;
using Datahub.Application.Services;
using Datahub.Application.Services.Notification;

namespace Datahub.Infrastructure.Services.Notification;

public class UserAccessNotificationService : IUserAccessNotificationService
{
    private readonly IGCNotifyService _gcNotifyService;

    public UserAccessNotificationService(IGCNotifyService gcNotifyService)
    {
        _gcNotifyService = gcNotifyService;
    }

    public async Task NotifyAccessRegrantedAsync(UserLockStatus lockStatus, string unlockedBy)
    {
        if (lockStatus == null)
        {
            return;
        }

        var unlockedByDisplay = string.IsNullOrWhiteSpace(unlockedBy) ? "DataHub administrator" : unlockedBy;

        await _gcNotifyService.SendUserAccessRegrantedNotification(
            lockStatus.UserEmail,
            lockStatus.UserName,
            "all workspaces",
            unlockedByDisplay);
    }
}