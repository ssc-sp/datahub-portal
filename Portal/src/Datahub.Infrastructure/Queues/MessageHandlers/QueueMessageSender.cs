using Azure.Storage.Queues;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using Datahub.Infrastructure.Extensions;
using MassTransit;

namespace Datahub.Infrastructure.Queues.MessageHandlers;

public abstract class QueueMessageSender<T>(ISendEndpointProvider sendEndpointProvider)
    : IRequestHandler<T>
    where T : IRequest
{

    public async Task Handle(T request, CancellationToken cancellationToken)
    {
        if(string.IsNullOrWhiteSpace(ConfigPathOrQueueName))
        {
            throw new Exception("ConfigPathOrQueueName is not set");
        }
        
        await sendEndpointProvider.SendDatahubServiceBusMessage(ConfigPathOrQueueName, request, cancellationToken);
    }

    protected abstract string ConfigPathOrQueueName { get; }
}