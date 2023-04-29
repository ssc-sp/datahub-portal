using System.Text.Json;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Projects;
using MediatR;
using Microsoft.Azure.Functions.Worker;

namespace Datahub.Functions
{
    public class ProjectUsageUpdater
    {
        private readonly ProjectUsageService _usageService;
        private readonly IMediator _mediator;
        private readonly QueuePongService _pongService;

        public ProjectUsageUpdater(ProjectUsageService usageService, IMediator mediator, QueuePongService pongService)
        {
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

            // verify message 
            if (message is null || message.ProjectId <= 0 || string.IsNullOrEmpty(message.ResourceGroup))
            {
                throw new Exception($"Invalid queue message:\n{queueItem}");
            }

            var resourceGroups = GetResourceGroups(message.ResourceGroup).ToArray();

            // update the usage
            if (!await _usageService.UpdateProjectUsage(message.ProjectId, resourceGroups, cancellationToken))
                return;

            // queue the usage notification message
            await _mediator.Send(new ProjectUsageNotificationMessage(message.ProjectId), cancellationToken);
        }

        static ProjectUsageUpdateMessage? DeserializeQueueMessage(string message)
        {
            return JsonSerializer.Deserialize<ProjectUsageUpdateMessage>(message);
        }

        static IEnumerable<string> GetResourceGroups(string name)
        {
            yield return name;
            var parts = name.Split('_').Select((s, idx) => idx == 1 ? "dbk" : s);
            yield return string.Join("-", parts);
        }
    }
}
