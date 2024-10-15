using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Storage.Blobs;
using Datahub.Infrastructure.Queues.Messages;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Datahub.Application.Services.Cost;
using Datahub.Application.Services.ResourceGroups;
using Datahub.Application.Services.Storage;
using Datahub.Infrastructure.Extensions;
using Datahub.Shared.Configuration;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Functions.Domain.Exceptions;

[assembly: InternalsVisibleTo("Datahub.SpecflowTests")]

namespace Datahub.Functions;

public class ProjectUsageScheduler(
    ILoggerFactory loggerFactory,
    IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
    ISendEndpointProvider sendEndpointProvider,
    IWorkspaceCostManagementService workspaceCostMgmtService,
    IWorkspaceStorageManagementService workspaceStorageMgmtService,
    IWorkspaceResourceGroupsManagementService rgMgmtService,
    IConfiguration config)
{
    public bool Mock { get; set; } = false;
    private readonly ILogger<ProjectUsageScheduler> _logger = loggerFactory.CreateLogger<ProjectUsageScheduler>();
    private readonly AzureConfig _azConfig = new(config);
    private const int WORKSPACE_UPDATE_LIMIT = 100;

    [Function("ProjectUsageScheduler")]
    public async Task Run([TimerTrigger("%ProjectUsageCRON%")] TimerInfo timerInfo)
    {
        await RunScheduler();
    }

    /*
     * Example request body:
     * {
     *  "manualRollover": false,
     *  "acronyms": ["DIE1", "DIE2"],
     * }
     */
    [Function("ProjectUsageSchedulerHttp")]
    public async Task<HttpResponseData> RunHttp(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
        HttpRequestData req)
    {
        _logger.LogInformation("Processing manual project usage request");
        var body = await req.ReadAsStringAsync();
        if (string.IsNullOrEmpty(body))
        {
            _logger.LogError("Request body is empty");
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            await response.WriteStringAsync("Request body is empty");
            return response;
        }

        _logger.LogInformation("Request body: {Body}", body);
        var schedulerRequest = ParseRequestBody(body);

        _logger.LogInformation("Manual rollover is set to: {ManualRollover}", schedulerRequest.ManualRollover);
        _logger.LogInformation("Acronyms: {Acronyms}", schedulerRequest.Acronyms);
        await RunScheduler(schedulerRequest.Acronyms, schedulerRequest.ManualRollover);
        var responseOk = req.CreateResponse(HttpStatusCode.OK);
        responseOk.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        await responseOk.WriteStringAsync("Request processed for manual project usage update" +
                                          (schedulerRequest.ManualRollover ? " with rollover" : " without rollover") +
                                          " for acronyms: " + string.Join(", ", schedulerRequest.Acronyms));
        return responseOk;
    }

    internal async Task<(int, int)> RunScheduler(List<string>? acronyms = default, bool manualRollover = false)
    {
        // Arrange
        _logger.LogInformation("Running project usage scheduler");
        acronyms ??= new List<string>();

        _logger.LogInformation("Grabbing top {Count} projects to update", WORKSPACE_UPDATE_LIMIT);
        var projects = await GetProjects(acronyms, WORKSPACE_UPDATE_LIMIT);
        if (!projects.Any())
        {
            _logger.LogInformation("No projects to update");
            return (0, 0);
        }

        _logger.LogInformation("Found {Count} projects to update", projects.Count);

        var subIds = projects.Select(p => "/subscriptions/" + p.DatahubAzureSubscription.SubscriptionId)
            .Distinct()
            .ToList();

        // Query and aggregate costs
        _logger.LogInformation("Querying and aggregating costs for {Count} subscriptions", subIds.Count);
        var aggregateTime = Stopwatch.StartNew();
        var (allCosts, allTotals) = await AggregateCosts(subIds);
        aggregateTime.Stop();
        _logger.LogInformation("Aggregated costs for {Count} subscriptions in {Time}ms", subIds.Count,
            aggregateTime.ElapsedMilliseconds);
        _logger.LogInformation("Obtained {Count} costs and {TotalCount} totals", allCosts.Count, allTotals.Count);

        // Upload costs and totals to blob storage
        _logger.LogInformation("Uploading costs and totals to blob storage");
        var uploadTimer = Stopwatch.StartNew();
        var (costBlobName, totalBlobName) = await PostToBlob(allCosts, allTotals);
        uploadTimer.Stop();
        _logger.LogInformation("Uploaded costs and totals to blob storage in {Time}ms",
            uploadTimer.ElapsedMilliseconds);
        _logger.LogInformation("Costs: {CostBlob}, Totals: {TotalBlob}", costBlobName, totalBlobName);

        // Send messages to update usage and capacity
        _logger.LogInformation("Sending messages to update usage and capacity for {Count} projects", projects.Count);
        var costMessages = 0;
        var storageMessages = 0;
        foreach (var usageMessage in projects.Select(resource => new ProjectUsageUpdateMessage(
                     resource.Project_Acronym_CD, costBlobName, totalBlobName,
                     manualRollover)))
        {
            var (costUpdate, storageUpdate) = await SendMessagesIfNeeded(usageMessage);
            costMessages += costUpdate ? 1 : 0;
            storageMessages += storageUpdate ? 1 : 0;
            // We delay to avoid sending all the messages at the same time to avoid throttling
            await Task.Delay(500);
        }

        _logger.LogInformation("Sent {CostMessages} cost update messages and {StorageMessages} storage update messages",
            costMessages, storageMessages);
        return (costMessages, storageMessages);
    }

    internal async Task<(bool, bool)> SendMessagesIfNeeded(ProjectUsageUpdateMessage usageMessage)
    {
        try
        {
            var costUpdate = NeedsCostUpdate(usageMessage.ProjectAcronym);
            var storageUpdate = NeedsStorageUpdate(usageMessage.ProjectAcronym);

            if (costUpdate)
            {
                // send/post the message
                await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.ProjectUsageUpdateQueueName,
                    usageMessage);
            }

            if (storageUpdate)
            {
                var capacityMessage = ConvertToCapacityUpdateMessage(usageMessage);

                // send/post the message,
                await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.ProjectCapacityUpdateQueueName,
                    capacityMessage);
            }

            return (costUpdate, storageUpdate);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while sending messages");
            throw new MessageSchedulingException($"Error while sending messages: {e.Message}");
        }
    }

    internal async Task<List<Datahub_Project>> GetProjects(List<string> acronyms, int limit)
    {
        try
        {
            await using var ctx = await dbContextFactory.CreateDbContextAsync();
            var projects = ctx.Projects
                .Include(p => p.DatahubAzureSubscription)
                .Include(p => p.Credits).ToList();

            if (acronyms.Any())
            {
                // If any acronyms were given, we explicitly grab those projects
                projects = projects.Where(p => acronyms.Contains(p.Project_Acronym_CD)).Take(limit).ToList();
            }
            else
            {
                // Otherwise, we grab the last 100 projects that were updated the longest ago
                projects = projects.OrderBy(LastUpdate).Where(NeedsUpdate).Take(limit).ToList();
            }

            return projects;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while getting projects");
            throw new ProjectFilteringException($"Error while getting projects: {e.Message}");
        }
    }

    internal async Task<(string, string)> PostToBlob(List<DailyServiceCost> costs, List<DailyServiceCost> totals)
    {
        var guid = Guid.NewGuid();
        var date = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss");
        try
        {
            var costBlob = await UploadToBlob("costs", date, guid, costs);
            var totalBlob = await UploadToBlob("totals", date, guid, totals);
            return (costBlob, totalBlob);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while uploading costs to blob");
            throw new BlobUploadException($"Error while uploading costs to blob: {e.Message}");
        }
    }


    internal async Task<(List<DailyServiceCost>, List<DailyServiceCost>)> AggregateCosts(List<string> subIds)
    {
        try
        {
            var allCosts = new List<DailyServiceCost>();
            var allTotals = new List<DailyServiceCost>();
            var startFiscalYear = CostManagementUtilities.CurrentFiscalYear.StartDate;

            foreach (var subId in subIds)
            {
                var rgNames = await rgMgmtService.GetAllSubscriptionResourceGroupsAsync(subId);

                var costs = await workspaceCostMgmtService.QuerySubscriptionCostsAsync(subId,
                    DateTime.UtcNow.Date.AddDays(-7),
                    DateTime.UtcNow.Date, QueryGranularity.Daily, rgNames);

                var totals = await workspaceCostMgmtService.QuerySubscriptionCostsAsync(subId, startFiscalYear,
                    DateTime.UtcNow.Date, QueryGranularity.Total, rgNames);

                allCosts.AddRange(costs);
                allTotals.AddRange(totals);
            }

            return (allCosts, allTotals);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while aggregating costs");
            throw new CostQueryException($"Error while aggregating costs: {e.Message}");
        }
    }

    internal async Task<string> UploadToBlob(string key, string date, Guid guid, List<DailyServiceCost> list)
    {
        var fileName = $"{key}-{date}-{guid}.json";
        if (!Mock)
        {
            var blobServiceClient = new BlobServiceClient(_azConfig.MediaStorageConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient("costs");
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(BinaryData.FromObjectAsJson(list));
        }

        return fileName;
    }

    private ProjectUsageSchedulerRequest ParseRequestBody(string body)
    {
        var schedulerRequest = JsonSerializer.Deserialize<ProjectUsageSchedulerRequest>(body);
        schedulerRequest.Acronyms ??= new List<string>();
        return schedulerRequest;
    }

    private DateTime LastUpdate(Datahub_Project p)
    {
        return p.Credits?.LastUpdate ?? DateTime.MinValue;
    }

    private bool NeedsUpdate(Datahub_Project p)
    {
        return NeedsCostUpdate(p.Project_Acronym_CD) || NeedsStorageUpdate(p.Project_Acronym_CD);
    }

    private bool NeedsCostUpdate(string workspaceAcronym)
    {
        return workspaceCostMgmtService.CheckUpdateNeeded(workspaceAcronym);
    }

    private bool NeedsStorageUpdate(string workspaceAcronym)
    {
        return workspaceStorageMgmtService.CheckUpdateNeeded(workspaceAcronym);
    }

    static ProjectCapacityUpdateMessage ConvertToCapacityUpdateMessage(ProjectUsageUpdateMessage message)
    {
        return new(message.ProjectAcronym, false);
    }

    struct ProjectUsageSchedulerRequest
    {
        [JsonPropertyName("acronyms")] public List<string> Acronyms { get; set; }
        [JsonPropertyName("manualRollover")] public bool ManualRollover { get; set; }
    }
}