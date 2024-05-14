using MassTransit;

namespace Datahub.Infrastructure.Extensions;

public static class SendEndpointProviderExtensions
{
        public static async Task SendDatahubServiceBusMessage(this ISendEndpointProvider sendEndpointProvider, string queueName, object message, CancellationToken cancellationToken = default)
        {
            var endpoint = await sendEndpointProvider.GetSendEndpoint(new Uri("queue:" + queueName));
            await endpoint.Send(message, cancellationToken);
        }
}