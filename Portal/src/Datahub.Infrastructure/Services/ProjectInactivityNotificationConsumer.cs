using MassTransit;
using Microsoft.Extensions.Logging;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Datahub;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Datahub.Infrastructure.Services;

public class ProjectInactivityNotificationConsumer : IConsumer<ProjectInactivityNotifications>
{
    private readonly ILogger _logger; 
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;


    public ProjectInactivityNotificationConsumer(ILoggerFactory loggerFactory, IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
    {
        _logger = loggerFactory.CreateLogger<ProjectInactivityNotificationConsumer>();
        _dbContextFactory = dbContextFactory;
    }

    public async Task Consume(ConsumeContext<ProjectInactivityNotifications> context)
    {
        var message = context.Message;

        // persist in DB
        _logger.LogInformation("Saving project inactivity notification in DB ...");
        await AddInactivityNotification(message.Project_ID, message.NotificationDate, message.DaysBeforeDeletion, message.SentTo, CancellationToken.None);

        // log activity
        _logger.LogInformation($"Saved project inactivity notification for: '{message.Project_ID}'.");
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
