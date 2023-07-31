using Datahub.Infrastructure.Queues.Messages;
using Microsoft.Extensions.Configuration;

namespace Datahub.Infrastructure.Queues.MessageHandlers;

public class ProjectCapacityUpdateSender : QueueMessageSender<ProjectCapacityUpdateMessage>
{
    public ProjectCapacityUpdateSender(IConfiguration configuration) : base(configuration)
    {
    }

    protected override string ConfigPathOrQueueName => "QueueProjectCapacityUpdate";
}
