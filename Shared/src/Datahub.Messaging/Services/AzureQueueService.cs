using Datahub.Shared.Entities;
using MassTransit;
using MediatR;

namespace Datahub.Shared.Messaging;
public class AzureQueueService
{
    private readonly IPublishEndpoint _publishEndpoint;
    public AzureQueueService(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishMessageAsync(WorkspaceDefinition messageContent)
    {
        //var message = new ForwardableAdapter<WorkspaceDefinition>(messageContent);
        var message = "";
        await _publishEndpoint.Publish<IForwardableMessage>(message);
    }
}

