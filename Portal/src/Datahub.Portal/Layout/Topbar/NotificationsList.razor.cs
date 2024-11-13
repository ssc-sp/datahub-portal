using Datahub.Core.Model.Datahub;
using MudBlazor.Utilities;


namespace Datahub.Portal.Layout.Topbar;

public partial class NotificationsList
{
    private async Task<string> GetLocalTime(SystemNotification notification)
    {
        var timestampUtc = DateTime.SpecifyKind(notification.Generated_TS, DateTimeKind.Utc);
        var localDatetime = await _timezoneService.LocalDateTime(timestampUtc);
        return localDatetime.ToString("yyyy-MM-dd HH:mm:ss");
    }
    
    private async Task RefreshNotifications()
    {
        _isLoading = true;
        StateHasChanged();
        _unreadNotificationCount =  await _systemNotificationService.GetNotificationCountForUser(_currentUserId, true);
        _totalNotificationCount = await _systemNotificationService.GetNotificationCountForUser(_currentUserId, _unreadOnly);
        _notifications = await _systemNotificationService.GetNotificationsForUser(_currentUserId, _unreadOnly, _currentPage -1);
        _isLoading = false;
        StateHasChanged();
    }

    private async Task ToggleUnread(SystemNotification notification)
    {
        await _systemNotificationService.SetReadStatus(notification.Notification_ID, !notification.Read_FLAG);
    }

    private async Task GoToActionLink(SystemNotification notification)
    {
        var t = _systemNotificationService.SetReadStatus(notification.Notification_ID, true);

        if (!string.IsNullOrEmpty(notification.ActionLink_URL))
        {
            _navigationManager.NavigateTo(notification.ActionLink_URL);
        }

        await t;
    }
    
    private async Task PageChanged(int i)
    {
        _currentPage = i;
        await RefreshNotifications();
    }

    private async Task ToggleUnreadOnly(bool value)
    {
        _unreadOnly = value;
        _currentPage = 1;
        await RefreshNotifications();
    }
    
    private async Task OnNotify(string userId)
    {
        if (userId == _currentUserId)
        {
            await InvokeAsync(RefreshNotifications);
        }
    }
    
    private static string BuildNotificationStyle(SystemNotification notification)
    {
        return new StyleBuilder()
            .AddStyle("border-left", $"4px solid var(--mud-palette-primary)", when: !notification.Read_FLAG)
            .Build();
    }
}