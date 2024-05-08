using Datahub.Shared.Configuration;
using MassTransit;

namespace Datahub.Functions.Extensions;

public static class SendEndpointProviderExtensions
{
        public static async Task SendDatahubServiceBusMessage(this ISendEndpointProvider sendEndpointProvider, string queueName, object message)
        {
            var endpoint = await sendEndpointProvider.GetSendEndpoint(new Uri("queue:" + queueName));
            await endpoint.Send(message);
        }
}