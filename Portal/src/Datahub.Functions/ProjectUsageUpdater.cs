using System.Text.Json;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Projects;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions;

public class ProjectUsageUpdater
{
    private readonly ILogger<ProjectUsageUpdater> _logger;
    private readonly ProjectUsageService _usageService;
    private readonly IMediator _mediator;
    private readonly QueuePongService _pongService;

    public ProjectUsageUpdater(ILoggerFactory loggerFactory, ProjectUsageService usageService, IMediator mediator, QueuePongService pongService)
    {
        _logger = loggerFactory.CreateLogger<ProjectUsageUpdater>();
        _usageService = usageService;
        _mediator = mediator;
        _pongService = pongService;
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

        // get resource groups
        var resourceGroups = EnumerateResourceGroups(message).ToArray();

        // update the usage
        if (!await _usageService.UpdateProjectUsage(message.ProjectId, resourceGroups, cancellationToken))
            return;

        // queue the usage notification message
        await _mediator.Send(new ProjectUsageNotificationMessage(message.ProjectId), cancellationToken);
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

        // get resource groups
        var resourceGroups = EnumerateResourceGroups(message).ToArray();

        // update the usage
        var capacityUsed = await _usageService.UpdateProjectCapacity(message.ProjectId, resourceGroups, cancellationToken);

        // log capacity found
        var groups = string.Join(", ", resourceGroups);
        _logger.LogInformation($"Used storage capacity for: '{groups}' is {capacityUsed}.");
    }

    static ProjectUsageUpdateMessage DeserializeQueueMessage(string text)
    {
        var message = JsonSerializer.Deserialize<ProjectUsageUpdateMessage>(text);
        
        // verify message 
        if (message is null || message.ProjectId <= 0 || string.IsNullOrEmpty(message.ResourceGroup))
        {
            throw new Exception($"Invalid queue message:\n{text}");
        }

        return message;
    }

    /// <summary>
    /// Given: fsdh_proj_die1_dev_rg
    /// </summary>
    /// <returns>[fsdh_proj_die1_dev_rg, fsdh-dbk-die1-dev-rg]</returns>
    static IEnumerable<string> EnumerateResourceGroups(ProjectUsageUpdateMessageBase message)
    {
        yield return message.ResourceGroup;
        if (message.Databricks)
        {
            var parts = message.ResourceGroup.Split('_').Select((s, idx) => idx == 1 ? "dbk" : s);
            yield return string.Join("-", parts);
        }
    }
}
