using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.UserTracking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Datahub.Infrastructure.Services
{
    public class UserInactivityNotificationService : IUserInactivityNotificationService
    {
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;

        public UserInactivityNotificationService(IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<EntityEntry<UserInactivityNotifications>> AddInactivityNotification(int userId, DateTime notificationDate, int daysBeforeLocked, int daysBeforeDeleted,
            CancellationToken ct)
        {
            using var ctx = await _dbContextFactory.CreateDbContextAsync(ct);
            var notification = new UserInactivityNotifications
            {
                User_ID = userId,
                NotificationDate = notificationDate,
                DaysBeforeLocked = daysBeforeLocked,
                DaysBeforeDeleted = daysBeforeDeleted
            };
            return ctx.UserInactivityNotifications.Add(notification);
        }
    }
}