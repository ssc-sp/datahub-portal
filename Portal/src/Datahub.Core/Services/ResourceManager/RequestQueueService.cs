using Datahub.Core.Data.ResourceProvisioner;
using Foundatio.Queues;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Datahub.Core.Services.ResourceManager
{
    public class RequestQueueService
    {
        private readonly IConfiguration configuration;

        public RequestQueueService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public async Task AddProjectToStorageQueue(CreateResourceData project)
        {
            using IQueue<CreateResourceData> queue = new AzureStorageQueue<CreateResourceData>(new AzureStorageQueueOptions<CreateResourceData>()
            {
                ConnectionString = configuration["ProjectCreationQueue:ConnectionString"],
                Name = configuration["ProjectCreationQueue:Name"],
            });
            await queue.EnqueueAsync(project);
        }
    }
}
