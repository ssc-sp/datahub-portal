using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.ProjectTools.Services;
using Datahub.Shared.Entities;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
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
        await RunScheduler();
    }

#if DEBUG
    [Function("ProjectUsageSchedulerHttp")]
    public async Task RunHttp([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestData req)
    {
        await RunScheduler();
    }
#endif

    private async Task RunScheduler()
    {
        using var ctx = await _dbContextFactory.CreateDbContextAsync();

        // set to keep track of already schedule projects
        HashSet<int> scheduled = new();
        var timeout = 0;

        var resources = await GetProjectResources(ctx);
        foreach (var resource in resources)
        {
            if (scheduled.Contains(resource.ProjectId))
                continue;

            var message = TryDeserializeMessage(resource, timeout);
            if (message is null)
            {
                _logger.LogWarning($"Invalid resource json found in project {resource.ProjectId}:\n{resource.JsonContent}");
                continue;
            }

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

    private ProjectUsageUpdateMessage? TryDeserializeMessage(ProjectResourceData resource, int timeout)
    {
        try
        {
            var content = JsonSerializer.Deserialize<DBResourceContent>(resource.JsonContent);
            if (string.IsNullOrEmpty(content?.resource_group_name))
                return default;

            return new ProjectUsageUpdateMessage(resource.ProjectId, content.resource_group_name, resource.Databricks, timeout);
        }
        catch
        {
            _logger.LogInformation($"Found project {resource.ProjectId} with invalid JsonContent:\n{resource.JsonContent}");
            return default;
        }
    }    

    private async Task<List<ProjectResourceData>> GetProjectResources(DatahubProjectDBContext ctx)
    {
        var databrickProjects = new HashSet<int>();

        var terraformServiceType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureDatabricks);
        var storageBlobType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureStorageBlob);

        var projects = new List<Project_Resources2>();
        await foreach (var res in ctx.Project_Resources2.AsAsyncEnumerable())
        {
            if (res.ResourceType == terraformServiceType)
            {
                databrickProjects.Add(res.ProjectId);
            }
            if (res.ResourceType == storageBlobType)
            {
                projects.Add(res);
            }
        }
        return projects.Select(p => new ProjectResourceData(p.ProjectId, p.JsonContent, databrickProjects.Contains(p.ProjectId))).ToList();
    }

    static ProjectCapacityUpdateMessage ConvertToCapacityUpdateMessage(ProjectUsageUpdateMessage message, int timeout)
    {
        return new(message.ProjectId, message.ResourceGroup, message.Databricks, timeout);
    }

    record DBResourceContent(string resource_group_name);
    record ProjectResourceData(int ProjectId, string JsonContent, bool Databricks);
}

