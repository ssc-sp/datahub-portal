using Datahub.Infrastructure.Queues.Messages;
using Microsoft.Extensions.Configuration;

namespace Datahub.Infrastructure.Queues.MessageHandlers;

public class ProjectUsageUpdateSender : QueueMessageSender<ProjectUsageUpdateMessage>
{
	public ProjectUsageUpdateSender(IConfiguration configuration) : base(configuration)
	{
	}

	protected override string ConfigPathOrQueueName => "QueueProjectUsageUpdate";
}
