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
        Task<int> CreateSystemNotifications(List<string> userIds, string textKey, params string[] arguments);
        Task<int> CreateSystemNotification(string userId, string textKey, params string[] arguments) =>
            CreateSystemNotifications(new List<string>() { userId }, textKey, arguments);

        Task<int> CreateSystemNotificationsWithLink(List<string> userIds, string actionLink, string textKey, params string[] arguments);
        Task<int> CreateSystemNotificationWithLink(string userId, string actionLink, string textKey, params string[] arguments) =>
            CreateSystemNotificationsWithLink(new List<string>() {  userId }, actionLink, textKey, arguments);

        Task<int> GetNotificationCountForUser(string userId, bool unreadOnly = false);
        Task<List<SystemNotification>> GetNotificationsForUser(string userId, bool unreadOnly = false);

        Task SetReadStatus(long notificationId, bool readStatus);

        event Func<string, Task> Notify;
    }
}
