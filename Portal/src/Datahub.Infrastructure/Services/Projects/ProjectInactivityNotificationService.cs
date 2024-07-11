using Datahub.Application.Services.Projects;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Datahub.Infrastructure.Services.Projects
{
	public class ProjectInactivityNotificationService : IProjectInactivityNotificationService
    {
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;

        public ProjectInactivityNotificationService(IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<EntityEntry<ProjectInactivityNotifications>> AddInactivityNotification(int projectId, DateTime notificationDate, int daysBeforeDeletion, string sentTo, CancellationToken ct)
        {
            using var ctx = await _dbContextFactory.CreateDbContextAsync(ct);
            var notification = new ProjectInactivityNotifications
            {
                Project_ID = projectId,
                NotificationDate = notificationDate,
                DaysBeforeDeletion = daysBeforeDeletion,
                SentTo = sentTo
            };
            return ctx.ProjectInactivityNotifications.Add(notification);
        }
    }
}