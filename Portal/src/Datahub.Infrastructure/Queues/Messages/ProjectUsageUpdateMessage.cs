using Datahub.Infrastructure.Queues.MessageHandlers;
using MediatR;

namespace Datahub.Infrastructure.Queues.Messages;

public class ProjectUsageUpdateMessageBase
{
	public ProjectUsageUpdateMessageBase(int projectId, string resourceGroup, bool databricks, int timeout)
	{
		ProjectId = projectId;
		ResourceGroup = resourceGroup;
		Databricks = databricks;
		Timeout = timeout;
	}

	public int ProjectId { get; }
	public string ResourceGroup { get; }
	public bool Databricks { get; }
	public int Timeout { get; }
}

public class ProjectUsageUpdateMessage : ProjectUsageUpdateMessageBase, IRequest, IMessageTimeout
{
	public ProjectUsageUpdateMessage(int projectId, string resourceGroup, bool databricks, int timeout) : base(projectId, resourceGroup, databricks, timeout)
	{
	}
}

public class ProjectCapacityUpdateMessage : ProjectUsageUpdateMessageBase, IRequest, IMessageTimeout
{
	public ProjectCapacityUpdateMessage(int projectId, string resourceGroup, bool databricks, int timeout) : base(projectId, resourceGroup, databricks, timeout)
	{
	}
}
