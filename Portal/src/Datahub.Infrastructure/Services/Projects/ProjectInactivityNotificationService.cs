using Datahub.Application.Services.Projects;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Infrastructure.Services.Projects
{
	public class ProjectInactivityNotificationService(IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
        : IProjectInactivityNotificationService
    {
        public async Task<int> AddInactivityNotification(int projectId, DateTime notificationDate, int daysBeforeDeletion, string sentTo, CancellationToken ct)
        {
            await using var ctx = await dbContextFactory.CreateDbContextAsync(ct);
            var notification = new ProjectInactivityNotifications
            {
                Project_ID = projectId,
                NotificationDate = notificationDate,
                DaysBeforeDeletion = daysBeforeDeletion,
                SentTo = sentTo
            };
            ctx.ProjectInactivityNotifications.Add(notification);
            return await ctx.SaveChangesAsync(ct);
        }
    }
}