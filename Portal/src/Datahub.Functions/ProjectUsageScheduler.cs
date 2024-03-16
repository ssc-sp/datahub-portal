using Datahub.Core.Model.Datahub;
using Datahub.Infrastructure.Queues.Messages;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Datahub.Application.Services.Budget;
using Microsoft.Extensions.Configuration;

namespace Datahub.Functions;

public class ProjectUsageScheduler
{
    private readonly ILogger<ProjectUsageScheduler> _logger;
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
    private readonly IMediator _mediator;
    private readonly IWorkspaceCostManagementService _workspaceCostMgmtService;
    private readonly AzureConfig _azConfig;

    public ProjectUsageScheduler(ILoggerFactory loggerFactory,
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory, IMediator mediator,
        IWorkspaceCostManagementService workspaceCostMgmtService, IConfiguration config)
    {
        _logger = loggerFactory.CreateLogger<ProjectUsageScheduler>();
        _dbContextFactory = dbContextFactory;
        _mediator = mediator;
        _workspaceCostMgmtService = workspaceCostMgmtService;
        _azConfig = new AzureConfig(config);
    }

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
        await RunScheduler();
    }
#endif

    private async Task RunScheduler()
    {
        using var ctx = await _dbContextFactory.CreateDbContextAsync();
        
        var timeout = 0;

        var projects = ctx.Projects.ToList();
        var sortedProjects = projects.OrderBy(p => GetLastUpdate(ctx, p.Project_ID)).ToList();
        var subscriptionCosts = await _workspaceCostMgmtService.QuerySubscriptionCosts(_azConfig.SubscriptionId,
            DateTime.UtcNow.Date.AddYears(-2), DateTime.UtcNow.Date);

        if (subscriptionCosts is null)
        {
            _logger.LogError("Cannot query costs at this time.");
            return;
        }

        foreach (var resource in sortedProjects)
        {

            var usageMessage = new ProjectUsageUpdateMessage(resource.Project_Acronym_CD, subscriptionCosts, timeout);

            // send/post the message
            await _mediator.Send(usageMessage);

            var capacityMessage = ConvertToCapacityUpdateMessage(usageMessage, timeout);

            // send/post the message,
            await _mediator.Send(capacityMessage);
        }

        _logger.LogInformation($"All projects scheduled for usage and capacity update");
    }

    private DateTime GetLastUpdate(DatahubProjectDBContext ctx, int projectId)
    {
        var lastUpdate = ctx.Project_Credits.Where(u => u.ProjectId == projectId).Select(u => u.LastUpdate)
            .FirstOrDefault();

        return lastUpdate ?? DateTime.MinValue;
    }

    static ProjectCapacityUpdateMessage ConvertToCapacityUpdateMessage(ProjectUsageUpdateMessage message, int timeout)
    {
        return new(message.ProjectAcronym, timeout);
    }
}