using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Core.Data.ResourceProvisioner;
using Foundatio.Queues;

namespace Datahub.Infrastructure.Services;

public class ResourceRequestService : IResourceRequestService
{
    private readonly DatahubPortalConfiguration _datahubPortalConfiguration;

    public ResourceRequestService(DatahubPortalConfiguration datahubPortalConfiguration)
    {
        _datahubPortalConfiguration = datahubPortalConfiguration;
    }
    public async Task AddProjectToStorageQueue(CreateResourceData project)
    {
        using IQueue<CreateResourceData> queue = new AzureStorageQueue<CreateResourceData>(new AzureStorageQueueOptions<CreateResourceData>()
        {
            ConnectionString = _datahubPortalConfiguration.DatahubStorageQueue.ConnectionString,
            Name = _datahubPortalConfiguration.DatahubStorageQueue.QueueNames.ResourceRunRequest,
        });
        await queue.EnqueueAsync(project);
    }
}