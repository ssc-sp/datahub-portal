using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Datahub.Application.Services.Cost;
using Datahub.Application.Services.Storage;
using Datahub.Functions.Domain.Exceptions;
using Datahub.Functions.Extensions;
using Datahub.Functions.Validators;
using Datahub.Infrastructure.Extensions;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using Datahub.Shared.Configuration;
using FluentValidation;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("Datahub.SpecflowTests")]

namespace Datahub.Functions;

public class ProjectUsageUpdater(
    ILoggerFactory loggerFactory,
    IWorkspaceCostManagementService workspaceCostMgmtService,
    IWorkspaceBudgetManagementService workspaceBudgetMgmtService,
    IWorkspaceStorageManagementService workspaceStorageMgmtService,
    ISendEndpointProvider sendEndpointProvider,
    IConfiguration config)
{
    private readonly ILogger<ProjectUsageUpdater> _logger = loggerFactory.CreateLogger<ProjectUsageUpdater>();
    private readonly AzureConfig _azConfig = new(config);
    public bool Mock = false;
    public List<DailyServiceCost> MockCosts { get; set; } = new(); 


    [Function("ProjectUsageUpdater")]
    public async Task Run(
        [ServiceBusTrigger(QueueConstants.ProjectUsageUpdateQueueName,
            Connection = "DatahubServiceBus:ConnectionString")]
        ServiceBusReceivedMessage serviceBusReceivedMessage,
        CancellationToken cancellationToken)
    {
        // deserialize message
        var message = await serviceBusReceivedMessage.DeserializeAndUnwrapMessageAsync<ProjectUsageUpdateMessage>();
        await UpdateUsage(message, cancellationToken);
    }

    [Function("ProjectCapacityUsageUpdater")]
    public async Task UpdateProjectCapacity(
        [ServiceBusTrigger(QueueConstants.ProjectCapacityUpdateQueueName,
            Connection = "DatahubServiceBus:ConnectionString")]
        ServiceBusReceivedMessage serviceBusReceivedMessage,
        CancellationToken cancellationToken)
    {
        var message = await serviceBusReceivedMessage.DeserializeAndUnwrapMessageAsync<ProjectCapacityUpdateMessage>();
        await UpdateCapacity(message, cancellationToken);
    }

    internal async Task<bool> UpdateUsage(ProjectUsageUpdateMessage message, CancellationToken cancellationToken)
    {
        var rolledOver = false;
        var projectUsageUpdateMessageValidator = new ProjectUsageUpdateMessageValidator();
        var validationResult = await projectUsageUpdateMessageValidator.ValidateAsync(message, cancellationToken);

        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation failed: {Errors}", validationResult.Errors.ToString());
            throw new ValidationException(validationResult.Errors);
        }

        // Costs blob download
        _logger.LogInformation("Downloading costs from blob {BlobName}...", message.CostsBlobName);
        var downloadCostsTimer = Stopwatch.StartNew();
        var costs = await FromBlob(message.CostsBlobName);
        downloadCostsTimer.Stop();
        _logger.LogInformation("Downloaded costs from blob {BlobName} in {Time}s. There are {CostsCount} entries",
            message.CostsBlobName, downloadCostsTimer.Elapsed.TotalSeconds, costs.Count);

        // Totals blob download
        _logger.LogInformation("Downloading totals from blob {BlobName}...", message.CostsBlobName);
        var downloadTotalsTimer = Stopwatch.StartNew();
        var totals = await FromBlob(message.TotalsBlobName);
        downloadTotalsTimer.Stop();
        _logger.LogInformation("Downloaded totals from blob {BlobName} in {Time}s. There are {TotalsCount} entries",
            message.TotalsBlobName, downloadTotalsTimer.Elapsed.TotalSeconds, totals.Count);

        // Use the costs to update workspace costs
        _logger.LogInformation("Updating workspace {Acronym} costs with given costs...", message.ProjectAcronym);
        var updateTimer = Stopwatch.StartNew();
        var (costRollover, spentAmount) = await UpdateWorkspaceCosts(costs, message.ProjectAcronym);
        updateTimer.Stop();
        _logger.LogInformation("Successfully updated {Acronym} costs. Time taken: {Time}s{Rollover}",
            message.ProjectAcronym, updateTimer.Elapsed.TotalSeconds,
            costRollover ? $". Budget rollover initiated for a reduction of ${spentAmount}" : String.Empty);

        // Use the totals to verify if a workspace requires a full refresh
        _logger.LogInformation("Verifying if workspace {Acronym} requires a full refresh...", message.ProjectAcronym);
        var refreshTimer = Stopwatch.StartNew();
        var refreshDone = await RefreshWorkspaceCosts(message.ProjectAcronym, totals);
        refreshTimer.Stop();
        if (refreshDone)
        {
            _logger.LogWarning("Workspace {Acronym} required a full refresh. Time taken: {Time}s",
                message.ProjectAcronym, refreshTimer.Elapsed.TotalSeconds);
        }
        else
        {
            _logger.LogInformation("Workspace {Acronym} did not require a full refresh. Time taken: {Time}s",
                message.ProjectAcronym, refreshTimer.Elapsed.TotalSeconds);
        }


        /* DEPRECATED
        // Also updates the budget spent amounts.
        _logger.LogInformation("Querying budget...");
        var budgetSpentAmount = await UpdateWorkspaceBudgetSpent(message.ProjectAcronym);
        _logger.LogInformation("Successfully updated budget.");
        */

        // Check if the cost rollover is necessary
        if (message.ForceRollover || costRollover)
        {
            _logger.LogInformation("Rollover starting for {Acronym}...", message.ProjectAcronym);
            var rolloverTimer = Stopwatch.StartNew();
            await Rollover(message.ProjectAcronym, spentAmount);
            rolloverTimer.Stop();
            _logger.LogInformation("Rollover completed for {Acronym} in {Time}s", message.ProjectAcronym,
                rolloverTimer.Elapsed.TotalSeconds);
            rolledOver = true;
        }

        await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.ProjectUsageNotificationQueueName,
            new ProjectUsageNotificationMessage(message.ProjectAcronym), cancellationToken);
        return rolledOver;
    }

    internal async Task UpdateCapacity(ProjectCapacityUpdateMessage message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting storage capacity for: \'{MessageProjectAcronym}\'", message.ProjectAcronym);
        var storageTimer = Stopwatch.StartNew();
        var capacityUsed = await workspaceStorageMgmtService.UpdateStorageCapacity(message.ProjectAcronym);
        storageTimer.Stop();
        _logger.LogInformation("Storage capacity for: \'{MessageProjectAcronym}\' updated in {Time}s",
            message.ProjectAcronym,
            storageTimer.Elapsed.TotalSeconds);

        _logger.LogInformation("Used storage capacity for: \'{MessageProjectAcronym}\' is {CapacityUsed}",
            message.ProjectAcronym, capacityUsed);
    }

    internal async Task<List<DailyServiceCost>> FromBlob(string fileName)
    {
        if (Mock)
        {
            return MockCosts;
        }
        _logger.LogInformation("Downloading from blob {FileName}", fileName);
        var blobServiceClient = new BlobServiceClient(_azConfig.MediaStorageConnectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient("costs");
        var blobClient = containerClient.GetBlobClient(fileName);
        var response = await blobClient.DownloadContentAsync();
        if (response.HasValue)
        {
            _logger.LogInformation("Downloaded costs from blob {FileName}", fileName);

            _logger.LogInformation("Parsing costs from blob {FileName}", fileName);
            var content = response.Value.Content.ToString();
            var costs = JsonSerializer.Deserialize<List<DailyServiceCost>>(content);
            if (costs is null)
            {
                _logger.LogError("Cannot parse costs from blob {FileName}", fileName);
                throw new BlobDownloadException($"Cannot parse costs from blob {fileName}");
            }

            return costs;
        }

        _logger.LogError("Cannot download costs from blob {FileName}", fileName);
        throw new BlobDownloadException($"Cannot download costs from blob {fileName}");
    }

    internal async Task<(bool, decimal)> UpdateWorkspaceCosts(List<DailyServiceCost> costs, string workspaceAcronym)
    {
        try
        {
            var (costRollover, spentAmount) =
                await workspaceCostMgmtService.UpdateWorkspaceCostsAsync(workspaceAcronym, costs);
            return (costRollover, spentAmount);
        }
        catch (Exception e)
        {
            _logger.LogError("Could not update workspace costs {Error}", e.Message);
            throw new CostUpdateException(e.Message);
        }
    }

    internal async Task<bool> RefreshWorkspaceCosts(string workspaceAcronym, List<DailyServiceCost> totals)
    {
        try
        {
            var refreshRequired =
                await workspaceCostMgmtService.VerifyAndRefreshWorkspaceCostsAsync(workspaceAcronym, totals);
            return refreshRequired;
        }
        catch (Exception e)
        {
            _logger.LogError("Could not refresh workspace costs {Error}", e.Message);
            throw new CostRefreshException(e.Message);
        }
    }

/*
    private async Task<decimal> UpdateWorkspaceBudgetSpent(string projectAcronym)
    {
        try
        {
            var spentAmount = await workspaceBudgetMgmtService.UpdateWorkspaceBudgetSpentAsync(projectAcronym);
            return spentAmount;
        }
        catch (Exception e)
        {
            throw new WorkspaceBudgetUpdateException(e.Message);
        }
    }
*/

    internal async Task Rollover(string workspaceAcronym, decimal spentAmount)
    {
        _logger.LogInformation($"Budget rollover initiated.");
        var currentBudget = await workspaceBudgetMgmtService.GetWorkspaceBudgetAmountAsync(workspaceAcronym);
        if (currentBudget == 0)
        {
            _logger.LogError("Cannot rollover budget, budget is 0");
            throw new RolloverException("Cannot rollover budget, budget is 0.");
        }

        _logger.LogInformation("Spend captured by cost management: {SpentAmount}", spentAmount);

        try
        {
            _logger.LogInformation("Executing rollover for {WorkspaceAcronym}", workspaceAcronym);
            await workspaceBudgetMgmtService.SetWorkspaceBudgetAmountAsync(workspaceAcronym,
                currentBudget - spentAmount, true);
            _logger.LogInformation($"Budget rollover completed.");
        }
        catch (Exception e)
        {
            _logger.LogError("Could not apply the budget rollover {Error}", e.Message);
            throw new RolloverException(e.Message);
        }
    }
}