namespace Datahub.Application.Services
{
    public interface IUserInactivityNotificationService
    {
        public Task<int> AddInactivityNotification(int userId, DateTime notificationDate, int daysBeforeLocked, int daysBeforeDeleted, CancellationToken ct);
    }
}