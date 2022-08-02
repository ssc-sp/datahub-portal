using System;
using System.Linq;
using System.Threading.Tasks;
using Datahub.Core.EFCore;


namespace Datahub.Core.Components.Notifications;

public partial class NotificationsList
{
    private async Task<string> GetLocalTime(SystemNotification notification)
    {
        var timestampUtc = DateTime.SpecifyKind(notification.Generated_TS, DateTimeKind.Utc);
        var localDatetime = await _timezoneService.GetLocalDateTime(timestampUtc);
        return localDatetime.ToString("yyyy-MM-dd HH:mm:ss");
    }
    
    private async Task RefreshNotifications()
    {
        _isLoading = true;
        StateHasChanged();
        TotalNotificationCount = await _systemNotificationService.GetNotificationCountForUser(CurrentUserId, UnreadOnly);

        UnreadNotificationCount = UnreadOnly ?
            TotalNotificationCount :
            await _systemNotificationService.GetNotificationCountForUser(CurrentUserId, true);

        _notifications = await _systemNotificationService.GetNotificationsForUser(CurrentUserId, UnreadOnly, CurrentPage);
        
        _notifications.Add(_notifications.First());
        _notifications.Add(_notifications.First());
        _notifications.Add(_notifications.First());
        _notifications.Add(_notifications.First());
        _notifications.Add(_notifications.First());
        _isLoading = false;
        StateHasChanged();
    }
    private async Task ToggleUnread(SystemNotification notification)
    {
        await _systemNotificationService.SetReadStatus(notification.Notification_ID, !notification.Read_FLAG);
    }

    private async Task GoToActionLink(SystemNotification notification)
    {
        await ToggleUnread(notification);
        if (!string.IsNullOrEmpty(notification.ActionLink_URL))
        {
            _navigationManager.NavigateTo(notification.ActionLink_URL);
        }
    }
    
    private async Task ToggleShowUnreadOnly()
    {
        UnreadOnly = !UnreadOnly;
        CurrentPage = 0;
        await RefreshNotifications();
    }
    
    private async Task FirstPage()
    {
        CurrentPage = 0;
        await RefreshNotifications();
    }

    private async Task NextPage()
    {
        CurrentPage = Math.Min(CurrentPage + 1, MaxPage);
        await RefreshNotifications();
    }

    private async Task PrevPage()
    {
        CurrentPage = Math.Max(CurrentPage - 1, 0);
        await RefreshNotifications();
    }
    
    public void Dispose()
    {
        _systemNotificationService.Notify -= OnNotify;
    }
    
    private async Task OnNotify(string userId)
    {
        if (userId == CurrentUserId)
        {
            await InvokeAsync(RefreshNotifications);
        }
    }
}