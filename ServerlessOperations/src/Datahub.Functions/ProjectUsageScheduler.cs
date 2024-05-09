using Datahub.Core.Model.Datahub;
using Datahub.Infrastructure.Queues.Messages;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Datahub.Application.Services.Budget;
using Datahub.Infrastructure.Extensions;
using Datahub.Shared.Configuration;
using MassTransit;
using Microsoft.Extensions.Configuration;

namespace Datahub.Functions;

public class ProjectUsageScheduler(
    ILoggerFactory loggerFactory,
    IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
    ISendEndpointProvider sendEndpointProvider,
    IWorkspaceCostManagementService workspaceCostMgmtService,
    IConfiguration config)
{
    private readonly ILogger<ProjectUsageScheduler> _logger = loggerFactory.CreateLogger<ProjectUsageScheduler>();
    private readonly AzureConfig _azConfig = new(config);

    [Function("ProjectUsageScheduler")]
    public async Task Run([TimerTrigger("%ProjectUsageCRON%")] TimerInfo timerInfo)
    {
        await RunScheduler();
    }

#if DEBUG
    [Function("ProjectUsageSchedulerHttp")]
    public async Task RunHttp(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
        HttpRequestData req)
    {
        var body = await req.ReadAsStringAsync();
        var manualRollover = body.Contains("manualRollover");
        await RunScheduler(manualRollover);
    }
#endif

    private async Task RunScheduler(bool manualRollover = false)
    {
        using var ctx = await dbContextFactory.CreateDbContextAsync();

        var timeout = 0;

        var projects = ctx.Projects.ToList();
        var sortedProjects = projects.OrderBy(p => GetLastUpdate(ctx, p.Project_ID)).ToList();
        var subscriptionCosts = await workspaceCostMgmtService.QuerySubscriptionCosts(null,
            DateTime.UtcNow.Date.AddDays(-7), DateTime.UtcNow.Date);

        if (subscriptionCosts is null)
        {
            _logger.LogError("Cannot query costs at this time.");
            return;
        }

        foreach (var resource in sortedProjects)
        {
            var usageMessage = new ProjectUsageUpdateMessage(resource.Project_Acronym_CD, subscriptionCosts, timeout,
                manualRollover);

            // send/post the message
            await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.ProjectUsageUpdateQueueName, usageMessage);

            var capacityMessage = ConvertToCapacityUpdateMessage(usageMessage, timeout, manualRollover);

            // send/post the message,
            await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.ProjectCapacityUpdateQueueName, capacityMessage);
        }

        // TODO: deadman switch?
        _logger.LogInformation($"All projects scheduled for usage and capacity update");
    }

    private DateTime GetLastUpdate(DatahubProjectDBContext ctx, int projectId)
    {
        var lastUpdate = ctx.Project_Credits.Where(u => u.ProjectId == projectId).Select(u => u.LastUpdate)
            .FirstOrDefault();

        return lastUpdate ?? DateTime.MinValue;
    }

    static ProjectCapacityUpdateMessage ConvertToCapacityUpdateMessage(ProjectUsageUpdateMessage message, int timeout,
        bool manualRollover)
    {
        return new(message.ProjectAcronym, timeout, manualRollover);
    }
}