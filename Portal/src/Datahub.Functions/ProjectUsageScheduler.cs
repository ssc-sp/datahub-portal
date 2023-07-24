using Datahub.Core.Model.Datahub;
using Datahub.Infrastructure.Queues.Messages;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Datahub.Core.Model.Projects;

namespace Datahub.Functions;

public class ProjectUsageScheduler
{
    private readonly ILogger<ProjectUsageScheduler> _logger;
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
    private readonly IMediator _mediator;

    public ProjectUsageScheduler(ILoggerFactory loggerFactory, IDbContextFactory<DatahubProjectDBContext> dbContextFactory, IMediator mediator)
	{
        _logger = loggerFactory.CreateLogger<ProjectUsageScheduler>();
        _dbContextFactory = dbContextFactory;
        _mediator = mediator;
    }

    [Function("ProjectUsageScheduler")]
    public async Task Run([TimerTrigger("%ProjectUsageCRON%")] TimerInfo timerInfo)
    {
        using var ctx = await _dbContextFactory.CreateDbContextAsync();
        
        // set to keep track of already schedule projects
        HashSet<int> scheduled = new();
        var timeout = 0;

        await foreach (var resource in ctx.Project_Resources2.AsAsyncEnumerable())
        {
            if (scheduled.Contains(resource.ProjectId))
                continue;

            var message = TryDeserializeMessage(resource, timeout);
            if (message is null)
                continue;

            timeout += 10; // add 10 seconds

            // track project id
            scheduled.Add(message.ProjectId);

            // send/post the message
            await _mediator.Send(message);

            timeout += 5;

            var capacityMessage = ConvertToCapacityUpdateMessage(message, timeout);

            // send/post the message
            await _mediator.Send(capacityMessage);
        }

        _logger.LogInformation($"{scheduled.Count} projects scheduled!");
    }

    record DBResourceContent(string resource_group_name);

    private ProjectUsageUpdateMessage? TryDeserializeMessage(Project_Resources2 row, int timeout)
    {
        try
        {
            var content = JsonSerializer.Deserialize<DBResourceContent>(row.JsonContent);
            if (string.IsNullOrEmpty(content?.resource_group_name))
                return default;

            return new ProjectUsageUpdateMessage(row.ProjectId, content.resource_group_name, timeout);
        }
        catch
        {
            _logger.LogInformation($"Found project {row.ProjectId} with invalid JsonContent:\n{row.JsonContent}");
            return default;
        }
    }    

    static ProjectCapacityUpdateMessage ConvertToCapacityUpdateMessage(ProjectUsageUpdateMessage message, int timeout)
    {
        return new(message.ProjectId, message.ResourceGroup, timeout);
    }
}

