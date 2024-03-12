using Datahub.Infrastructure.Queues.Messages;
using Microsoft.Extensions.Configuration;

namespace Datahub.Infrastructure.Queues.MessageHandlers;

public class ProjectUsageNotificationSender : QueueMessageSender<ProjectUsageNotificationMessage>
{
	public ProjectUsageNotificationSender(IConfiguration configuration) : base(configuration)
	{
	}

	protected override string ConfigPathOrQueueName => "QueueProjectUsageNotification";
}
