using MassTransit;
using Datahub.Core.Model.Datahub;
using Microsoft.EntityFrameworkCore;
using Datahub.Core.Model.Projects;

namespace Datahub.Infrastructure.Services;

public class ProjectInactivityNotificationConsumer : IConsumer<ProjectInactivityNotifications>
{
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
     
    public ProjectInactivityNotificationConsumer(IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task Consume(ConsumeContext<ProjectInactivityNotifications> context)
    {
        var message = context.Message;
        using var ctx = await _dbContextFactory.CreateDbContextAsync();

        ctx.ProjectInactivityNotifications.Add(message);

        await ctx.SaveChangesAsync();
        return;
    }

}
