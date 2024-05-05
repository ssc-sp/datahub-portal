using MassTransit;
using Microsoft.Extensions.Logging;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Application.Services.Storage;

namespace Datahub.Infrastructure.Services;

public class ProjectCapacityUpdateConsumer : IConsumer<ProjectCapacityUpdateMessage>
{
    private readonly ILogger _logger;
    private readonly IWorkspaceStorageManagementService _workspaceStorageMgmtService;

    public ProjectCapacityUpdateConsumer(ILoggerFactory loggerFactory, IWorkspaceStorageManagementService workspaceStorageMgmtService)
    {
        _logger = loggerFactory.CreateLogger<ProjectCapacityUpdateConsumer>();   
        _workspaceStorageMgmtService = workspaceStorageMgmtService;
    }
    public async Task Consume(ConsumeContext<ProjectCapacityUpdateMessage> context)
    {
        var message = context.Message;

        // update the capacity
        _logger.LogInformation("Querying storage capacity...");
        var capacityUsed = await _workspaceStorageMgmtService.UpdateStorageCapacity(message.ProjectAcronym);

        // log capacity found
        _logger.LogInformation($"Used storage capacity for: '{message.ProjectAcronym}' is {capacityUsed}.");
    }
}
