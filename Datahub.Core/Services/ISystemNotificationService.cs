using Datahub.Core.EFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public interface ISystemNotificationService
    {
        Task<int> CreateSystemNotifications(List<string> userIds, string textKey, params object[] arguments);
        Task<int> CreateSystemNotification(string userId, string textKey, params object[] arguments) =>
            CreateSystemNotifications(new List<string>() { userId }, textKey, arguments);

        Task<int> CreateSystemNotificationsWithLink(List<string> userIds, string actionLink, string linkKey, string textKey, params object[] arguments);
        Task<int> CreateSystemNotificationWithLink(string userId, string actionLink, string linkKey, string textKey, params object[] arguments) =>
            CreateSystemNotificationsWithLink(new List<string>() {  userId }, actionLink, linkKey, textKey, arguments);
        Task<int> CreateSystemNotificationsWithLink(List<string> userIds, string actionLink, string textKey, params object[] arguments) =>
            CreateSystemNotificationsWithLink(userIds, actionLink, null, textKey, arguments);
        Task<int> CreateSystemNotificationWithLink(string userId, string actionLink, string textKey, params object[] arguments) =>
            CreateSystemNotificationsWithLink(new List<string>() { userId }, actionLink, null, textKey, arguments);

        Task<int> GetNotificationCountForUser(string userId, bool unreadOnly = false);
        Task<List<SystemNotification>> GetNotificationsForUser(string userId, bool unreadOnly = false, int pageNumber = 0);

        Task SetReadStatus(long notificationId, bool readStatus);

        int GetNotificationPageSize();

        event Func<string, Task> Notify;
    }

    // These classes (or similar) may be useful in a more general context
    public record BilingualStringArgument(string English, string French);
    public record LocalizedKeyStringArgument(string Key);
}
