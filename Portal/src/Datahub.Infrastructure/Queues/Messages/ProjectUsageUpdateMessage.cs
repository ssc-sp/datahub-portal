using Datahub.Infrastructure.Queues.MessageHandlers;
using MediatR;

namespace Datahub.Infrastructure.Queues.Messages;

public class ProjectUsageUpdateMessage : IRequest, IMessageTimeout
{
    public ProjectUsageUpdateMessage(int projectId, string resourceGroup, int timeout)
    {
        ProjectId = projectId;
        ResourceGroup = resourceGroup;
        Timeout = timeout;
    }

    public int ProjectId { get; }
    public string ResourceGroup { get; }
    public int Timeout { get; }
}

