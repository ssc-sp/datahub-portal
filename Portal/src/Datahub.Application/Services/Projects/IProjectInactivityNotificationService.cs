
namespace Datahub.Application.Services.Projects
{
    public interface IProjectInactivityNotificationService
    {
        public Task<int> AddInactivityNotification(int projectId, DateTime notificationDate, int daysBeforeDeletion, string sentTo, CancellationToken ct);
    }
}