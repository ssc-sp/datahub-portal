using Azure.Storage.Queues;
using MassTransit;
using MediatR; 
using Microsoft.Extensions.Configuration;


namespace Datahub.Infrastructure.Queues.MessageHandlers;

public class QueueMessageHandler 
{
    protected readonly IPublishEndpoint _publishEndpoint;

    public QueueMessageHandler(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task SendMessageAsync<T>(T message) where T : class
    {
        await _publishEndpoint.Publish<T>(message);
    }
}