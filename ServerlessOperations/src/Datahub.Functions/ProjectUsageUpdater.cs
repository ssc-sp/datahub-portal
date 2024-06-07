using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Datahub.Application.Services.Budget;
using Datahub.Application.Services.Storage;
using Datahub.Functions.Extensions;
using Datahub.Infrastructure.Extensions;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using Datahub.Shared.Configuration;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions;
public class ProjectUsageUpdater(
    ILoggerFactory loggerFactory,
    QueuePongService pongService,
    IWorkspaceCostManagementService workspaceCostMgmtService,
    IWorkspaceBudgetManagementService workspaceBudgetMgmtService,
    ISendEndpointProvider sendEndpointProvider,
    IWorkspaceStorageManagementService workspaceStorageMgmtService,
    IConfiguration config)
{
    private readonly ILogger<ProjectUsageUpdater> _logger = loggerFactory.CreateLogger<ProjectUsageUpdater>();
    private readonly AzureConfig _azConfig = new(config);


    [Function("ProjectUsageUpdater")]
    public async Task<bool> Run(
        [ServiceBusTrigger(QueueConstants.ProjectUsageUpdateQueueName,
            Connection = "DatahubServiceBus:ConnectionString")]
        ServiceBusReceivedMessage serviceBusReceivedMessage,
        CancellationToken cancellationToken)
    {
        var rolledOver = false;
        // test for ping
        // if (await pongService.Pong(serviceBusReceivedMessage.Body.ToString()))
        // return false;

        // deserialize message
        var message = await serviceBusReceivedMessage.DeserializeAndUnwrapMessageAsync<ProjectUsageUpdateMessage>();

        _logger.LogInformation("Downloading costs from blob...");
        var costs = await GetFromBlob(message.CostsBlobName);
        
        _logger.LogInformation("Querying cost management...");
        var (costRollover, spentAmount) =
            await workspaceCostMgmtService.UpdateWorkspaceCostAsync(costs, message.ProjectAcronym);
        _logger.LogInformation("Querying budget...");
        var budgetSpentAmount =
            await workspaceBudgetMgmtService.UpdateWorkspaceBudgetSpentAsync(message.ProjectAcronym);

        // The query to cost checks if the last update was outside of the current fiscal year, if so that means we are in a new fiscal year
        // The query to budget checks if the amount spent captured by the budget is less than previously. If so, that means the budget was renewed.
        if (message.ForceRollover || costRollover)
        {
            _logger.LogInformation($"Budget rollover initiated.");
            var currentBudget = await workspaceBudgetMgmtService.GetWorkspaceBudgetAmountAsync(message.ProjectAcronym);
            _logger.LogInformation($"Spend captured by cost management: {spentAmount}");
            _logger.LogInformation($"Spend captured by budget : {budgetSpentAmount}");

            // Checking if the two captured costs are within 5% of each other to ensure that the rollover is valid
            var relativeDifference = (budgetSpentAmount - spentAmount) / budgetSpentAmount;
            if (relativeDifference <= (decimal)0.05)
            {
                _logger.LogInformation($"Executing rollover for {message.ProjectAcronym}");
                await workspaceBudgetMgmtService.SetWorkspaceBudgetAmountAsync(message.ProjectAcronym,
                    currentBudget - budgetSpentAmount, true);
                rolledOver = true;
                // Generate fiscal year report here?
            }
            else
            {
                _logger.LogWarning(
                    $"Aborted rollover due to large difference between captured spend and captured costs ({relativeDifference:P})");
            }
        }

        // queue the usage notification message
        await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.ProjectUsageNotificationQueueName,
            new ProjectUsageNotificationMessage(message.ProjectAcronym), cancellationToken);
        return rolledOver;
    }

    [Function("ProjectCapacityUsageUpdater")]
    public async Task UpdateProjectCapacity(
        [ServiceBusTrigger(QueueConstants.ProjectCapacityUpdateQueueName,
            Connection = "DatahubServiceBus:ConnectionString")]
        ServiceBusReceivedMessage serviceBusReceivedMessage,
        CancellationToken cancellationToken)
    {
        // test for ping
        if (await pongService.Pong(serviceBusReceivedMessage.Body.ToString()))
            return;

        // deserialize message
        var message = await serviceBusReceivedMessage.DeserializeAndUnwrapMessageAsync<ProjectUsageUpdateMessage>();

        // update the capacity
        _logger.LogInformation("Querying storage capacity...");
        var capacityUsed = await workspaceStorageMgmtService.UpdateStorageCapacity(message.ProjectAcronym);

        // log capacity found
        _logger.LogInformation($"Used storage capacity for: '{message.ProjectAcronym}' is {capacityUsed}.");
    }

    public async Task<List<DailyServiceCost>> GetFromBlob(string fileName)
    {
        _logger.LogInformation($"Downloading costs from blob {fileName}");
        var blobServiceClient = new BlobServiceClient(_azConfig.MediaStorageConnectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient("costs");
        var blobClient = containerClient.GetBlobClient(fileName);
        var response = await blobClient.DownloadContentAsync();
        if (response.HasValue)
        {
            _logger.LogInformation($"Downloaded costs from blob {fileName}");
            var plainTextResult = response.Value.Content.ToString();
            var costs = JsonSerializer.Deserialize<BlobCostObject>(plainTextResult);
            return costs.Costs;
        }
        _logger.LogError($"Cannot download costs from blob {fileName}");
        return null;
    }

    record BlobCostObject
    {
        [JsonPropertyName("costs")] public List<DailyServiceCost> Costs { get; set; }
    }
}