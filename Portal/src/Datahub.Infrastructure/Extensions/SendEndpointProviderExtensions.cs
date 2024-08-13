using MassTransit;

namespace Datahub.Infrastructure.Extensions;

public static class SendEndpointProviderExtensions
{

    private static async Task<ISendEndpoint> GetEndpoint(this ISendEndpointProvider provider, string queueName) => await provider.GetSendEndpoint(new Uri($"queue:{queueName}"));

    public static async Task SendDatahubServiceBusMessage(this ISendEndpointProvider sendEndpointProvider, string queueName, object message, CancellationToken cancellationToken = default)
    {
        var endpoint = await sendEndpointProvider.GetEndpoint(queueName);
        await endpoint.Send(message, cancellationToken);
    }

    public static async Task SendDatahubServiceBusMessages(this ISendEndpointProvider sendEndpointProvider, string queueName, IEnumerable<object?> messages, CancellationToken cancellationToken = default)
    {
        var endpoint = await sendEndpointProvider.GetEndpoint(queueName);
        var nonNullMessages = messages.Where(m => m is not null).Cast<object>();
        if (nonNullMessages.Any())
        {
            await endpoint.SendBatch(nonNullMessages, cancellationToken);
        }
    }
}