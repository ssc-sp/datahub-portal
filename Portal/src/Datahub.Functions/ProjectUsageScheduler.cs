using Datahub.Core.Model.Datahub;
using Datahub.Infrastructure.Queues.Messages;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

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

        await foreach (var resource in ctx.Project_Resources2.AsAsyncEnumerable())
        {
            if (scheduled.Contains(resource.ProjectId))
                continue;

            var message = TryDeserializeMessage(resource);
            if (message is null)
                continue;

            // track project id
            scheduled.Add(message.ProjectId);

            // send/post the message
            await _mediator.Send(message);
        }

        _logger.LogInformation($"{scheduled.Count} projects scheduled!");
    }

    record DBResourceContent(string resource_group_name, string storage_account);

    private ProjectUsageUpdateMessage? TryDeserializeMessage(Project_Resources2 row)
    {
        try
        {
            var content = JsonSerializer.Deserialize<DBResourceContent>(row.JsonContent);
            if (string.IsNullOrEmpty(content?.resource_group_name) || string.IsNullOrEmpty(content?.storage_account))
                return default;

            return new ProjectUsageUpdateMessage(row.ProjectId, content.resource_group_name, content.storage_account);
        }
        catch
        {
            _logger.LogInformation($"Found project {row.ProjectId} with invalid JsonContent:\n{row.JsonContent}");
            return default;
        }
    }    
}

