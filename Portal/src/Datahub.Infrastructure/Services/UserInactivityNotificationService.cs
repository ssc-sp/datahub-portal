using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Users;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Infrastructure.Services
{
    public class UserInactivityNotificationService(IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
        : IUserInactivityNotificationService
    {
        public async Task<int> AddInactivityNotification(int userId, DateTime notificationDate, int daysBeforeLocked, int daysBeforeDeleted,
            CancellationToken ct)
        {
            await using var ctx = await dbContextFactory.CreateDbContextAsync(ct);
            var notification = new UserInactivityNotifications
            {
                User_ID = userId,
                NotificationDate = notificationDate,
                DaysBeforeLocked = daysBeforeLocked,
                DaysBeforeDeleted = daysBeforeDeleted
            };
            ctx.UserInactivityNotifications.Add(notification);
            return await ctx.SaveChangesAsync(ct);
        }
    }
}