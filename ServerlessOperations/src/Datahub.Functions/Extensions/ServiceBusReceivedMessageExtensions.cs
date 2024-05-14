using System.Text.Json;
using Azure.Messaging.ServiceBus;

namespace Datahub.Functions.Extensions;

public static class ServiceBusReceivedMessageExtensions
{
    public const string MessagePropertyName = "message";
    public static async Task<T> DeserializeAndUnwrapMessageAsync<T>(
        this ServiceBusReceivedMessage serviceBusReceivedMessage)
    {
        var messageEnvelope = await JsonDocument.ParseAsync(serviceBusReceivedMessage.Body.ToStream());
        messageEnvelope.RootElement.TryGetProperty(MessagePropertyName, out var message);

        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        return message.Deserialize<T>(deserializeOptions) ?? throw new InvalidOperationException("Failed to deserialize message");
    }
}