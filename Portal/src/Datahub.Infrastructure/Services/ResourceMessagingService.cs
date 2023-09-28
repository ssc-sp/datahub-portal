using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Core.Data.ResourceProvisioner;
using Datahub.Shared.Entities;
using Foundatio.Queues;

namespace Datahub.Infrastructure.Services;

public class ResourceMessagingService : IResourceMessagingService
{
    private readonly DatahubPortalConfiguration _datahubPortalConfiguration;

    public ResourceMessagingService(DatahubPortalConfiguration datahubPortalConfiguration)
    {
        _datahubPortalConfiguration = datahubPortalConfiguration;
    }
    public async Task SendToTerraformQueue(CreateResourceData project)
    {
        using IQueue<CreateResourceData> queue = new AzureStorageQueue<CreateResourceData>(new AzureStorageQueueOptions<CreateResourceData>()
        {
            ConnectionString = _datahubPortalConfiguration.DatahubStorageQueue.ConnectionString,
            Name = _datahubPortalConfiguration.DatahubStorageQueue.QueueNames.ResourceRunRequest,
        });
        await queue.EnqueueAsync(project);
    }

    public async Task SendToUserQueue(WorkspaceDefinition workspaceDefinition)
    {
        using IQueue<WorkspaceDefinition> queue = new AzureStorageQueue<WorkspaceDefinition>(
            new AzureStorageQueueOptions<WorkspaceDefinition>()
            {
                ConnectionString = _datahubPortalConfiguration.DatahubStorageQueue.ConnectionString,
                Name = _datahubPortalConfiguration.DatahubStorageQueue.QueueNames.UserRunRequest,
            });
        await queue.EnqueueAsync(workspaceDefinition);
    }
}