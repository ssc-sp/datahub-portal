using Datahub.Infrastructure.Queues.Messages;
using Microsoft.Extensions.Configuration;

namespace Datahub.Infrastructure.Queues.MessageHandlers
{
    public class ProjectInactivityNotificationSender : QueueMessageSender<ProjectInactivityNotificationMessage>
    {
        public ProjectInactivityNotificationSender(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string ConfigPathOrQueueName => "QueueProjectInactivityNotification";
    }
}