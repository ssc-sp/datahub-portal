using Datahub.Core.Model.UserTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Datahub.Application.Services
{
    public interface IUserInactivityNotificationService
    {
        public Task<EntityEntry<UserInactivityNotifications>> AddInactivityNotification(int userId, DateTime notificationDate, int daysBeforeLocked, int daysBeforeDeleted, CancellationToken ct);
    }
}