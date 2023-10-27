using Datahub.Infrastructure.Queues.Messages;
using Microsoft.Extensions.Configuration;

namespace Datahub.Infrastructure.Queues.MessageHandlers
{
    public class ProjectInactivityUpdateSender : QueueMessageSender<ProjectInactivityNotificationMessage>
    {
        public ProjectInactivityUpdateSender(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string ConfigPathOrQueueName => "QueueProjectInactivityUpdate";
    }
}