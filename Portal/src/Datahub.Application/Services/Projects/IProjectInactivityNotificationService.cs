using Datahub.Core.Model.Projects;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Datahub.Application.Services.Projects
{
    public interface IProjectInactivityNotificationService
    {
        public Task<EntityEntry<ProjectInactivityNotifications>> AddInactivityNotification(int projectId, DateTime notificationDate, int daysBeforeDeletion, string sentTo, CancellationToken ct);
    }
}