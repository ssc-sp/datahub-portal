using System.Text.Json;
using Datahub.Application.Services.Budget;
using Datahub.Application.Services.Storage;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions;

public class ProjectUsageUpdater
{
    private readonly ILogger<ProjectUsageUpdater> _logger;
    private readonly IMediator _mediator;
    private readonly QueuePongService _pongService;
    private readonly IWorkspaceCostManagementService _workspaceCostMgmtService;
    private readonly IWorkspaceBudgetManagementService _workspaceBudgetMgmtService;
    private readonly IWorkspaceStorageManagementService _workspaceStorageMgmtService;
    

    public ProjectUsageUpdater(ILoggerFactory loggerFactory, IMediator mediator, QueuePongService pongService, IWorkspaceCostManagementService workspaceCostMgmtService, IWorkspaceBudgetManagementService workspaceBudgetMgmtService, IWorkspaceStorageManagementService workspaceStorageMgmtService)
    {
        _logger = loggerFactory.CreateLogger<ProjectUsageUpdater>();
        _mediator = mediator;
        _pongService = pongService;
        _workspaceCostMgmtService = workspaceCostMgmtService;
        _workspaceBudgetMgmtService = workspaceBudgetMgmtService;
        _workspaceStorageMgmtService = workspaceStorageMgmtService;
    }

    [Function("ProjectUsageUpdater")]
    public async Task Run([QueueTrigger("%QueueProjectUsageUpdate%", Connection = "DatahubStorageConnectionString")] string queueItem, 
        CancellationToken cancellationToken)
    {
        // test for ping
        if (await _pongService.Pong(queueItem))
            return;

        // deserialize message
        var message = DeserializeQueueMessage(queueItem);

        _logger.LogInformation("Querying cost management...");
        var (costRollover, spentAmount) =
            await _workspaceCostMgmtService.UpdateWorkspaceCostAsync(message.SubscriptionCosts, message.ProjectAcronym);
        _logger.LogInformation("Querying budget...");
        var budgetSpentAmount = await _workspaceBudgetMgmtService.UpdateWorkspaceBudgetSpentAsync(message.ProjectAcronym);

        // The query to cost checks if the last update was outside of the current fiscal year, if so that means we are in a new fiscal year
        // The query to budget checks if the amount spent captured by the budget is less than previously. If so, that means the budget was renewed.
        if (message.ForceRollover || costRollover)
        {
            _logger.LogInformation($"Budget rollover initiated.");
            var currentBudget = await _workspaceBudgetMgmtService.GetWorkspaceBudgetAmountAsync(message.ProjectAcronym);
            _logger.LogInformation($"Spend captured by cost management: {spentAmount}");
            _logger.LogInformation($"Spend captured by budget : {budgetSpentAmount}");
            
            // Checking if the two captured costs are within 5% of each other to ensure that the rollover is valid
            var relativeDifference = (budgetSpentAmount - spentAmount) / (budgetSpentAmount);
            if ( relativeDifference <= (decimal)0.05)
            {
                _logger.LogInformation($"Executing rollover for {message.ProjectAcronym}");
                await _workspaceBudgetMgmtService.SetWorkspaceBudgetAmountAsync(message.ProjectAcronym,
                    currentBudget - budgetSpentAmount);
                // Generate fiscal year report here?
            }
            else
            {
                _logger.LogWarning($"Aborted rollover due to large difference between captured spend and captured costs ({relativeDifference:P})");
            }
        }

        // queue the usage notification message
        await _mediator.Send(new ProjectUsageNotificationMessage(message.ProjectAcronym), cancellationToken);
    }

    [Function("ProjectCapacityUsageUpdater")]
    public async Task UpdateProjectCapacity([QueueTrigger("%QueueProjectCapacityUpdate%", Connection = "DatahubStorageConnectionString")] string queueItem,
    CancellationToken cancellationToken)
    {
        // test for ping
        if (await _pongService.Pong(queueItem))
            return;

        // deserialize message
        var message = DeserializeQueueMessage(queueItem);

        // update the capacity
        _logger.LogInformation("Querying storage capacity...");
        var capacityUsed = await _workspaceStorageMgmtService.UpdateStorageCapacity(message.ProjectAcronym);

        // log capacity found
        _logger.LogInformation($"Used storage capacity for: '{message.ProjectAcronym}' is {capacityUsed}.");
    }

    static ProjectUsageUpdateMessage DeserializeQueueMessage(string text)
    {
        var message = JsonSerializer.Deserialize<ProjectUsageUpdateMessage>(text);
        
        // verify message 
        if (message is null || string.IsNullOrEmpty(message.ProjectAcronym) )
        {
            throw new Exception($"Invalid queue message:\n{text}");
        }

        return message;
    }
}
