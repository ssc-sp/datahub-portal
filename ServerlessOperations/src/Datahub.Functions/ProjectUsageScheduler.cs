using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Storage.Blobs;
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
        await using var ctx = await dbContextFactory.CreateDbContextAsync();

        var projects = ctx.Projects.Include(p => p.DatahubAzureSubscription).ToList();
        var sortedProjects = projects.OrderBy(p => GetLastUpdate(ctx, p.Project_ID)).ToList();
        var subIds = sortedProjects.Select(p => "subscriptions/" + p.DatahubAzureSubscription.SubscriptionId).Distinct()
            .ToList();

        var allCosts = await AggregateCosts(subIds);
        var costBlobName = await PostToBlob(allCosts);

        foreach (var resource in sortedProjects)
        {
            var usageMessage = new ProjectUsageUpdateMessage(resource.Project_Acronym_CD, costBlobName,
                manualRollover);

            // send/post the message
            await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.ProjectUsageUpdateQueueName,
            usageMessage);

            var capacityMessage = ConvertToCapacityUpdateMessage(usageMessage, manualRollover);

            // send/post the message,
            await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.ProjectCapacityUpdateQueueName,
            capacityMessage);
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

    private async Task<string> PostToBlob(List<DailyServiceCost> subCosts)
    {
        var fileName = "costs-" + DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss") +Guid.NewGuid()+ ".json";
        var blobServiceClient = new BlobServiceClient(_azConfig.MediaStorageConnectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient("costs");
        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(BinaryData.FromObjectAsJson(subCosts));
        return fileName;
    }
    
    private async Task<List<DailyServiceCost>> AggregateCosts(List<string> subIds)
    {
        var allCosts = new List<DailyServiceCost>();
        foreach (var subId in subIds)
        {
            var costs = await workspaceCostMgmtService.QuerySubscriptionCosts(subId, DateTime.UtcNow.Date.AddDays(-7),
                DateTime.UtcNow.Date);
            if (costs is not null)
            {
                allCosts.AddRange(costs);
            }
            else
            {
                _logger.LogError($"Cannot query costs for subscription {subId}");
            }
        }

        return allCosts;
    }

    static ProjectCapacityUpdateMessage ConvertToCapacityUpdateMessage(ProjectUsageUpdateMessage message,
        bool manualRollover)
    {
        return new(message.ProjectAcronym, manualRollover);
    }

    record BlobCostObject
    {
        [JsonPropertyName("costs")]
        public List<DailyServiceCost> Costs { get; set; }
    }
}
