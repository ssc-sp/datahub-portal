using System.Text.Json;
using Azure.Messaging.ServiceBus;

namespace Datahub.Functions.Extensions;

public static class ServiceBusReceivedMessageExtensions
{
    public const string MessagePropertyName = "message";

    /// <summary>
    /// Deserializes the message body of a ServiceBusReceivedMessage and unwraps the message from the envelope.
    /// Supports both MassTransit-style wrapped messages and plain message payloads.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="serviceBusReceivedMessage"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static async Task<T> DeserializeAndUnwrapMessageAsync<T>(
        this ServiceBusReceivedMessage serviceBusReceivedMessage)
    {
        var messageEnvelope = await JsonDocument.ParseAsync(serviceBusReceivedMessage.Body.ToStream());
        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        if (messageEnvelope.RootElement.ValueKind == JsonValueKind.Object &&
            messageEnvelope.RootElement.TryGetProperty(MessagePropertyName, out var message) &&
            message.ValueKind != JsonValueKind.Undefined)
        {
            return message.Deserialize<T>(deserializeOptions) ?? throw new InvalidOperationException("Failed to deserialize message");
        }

        return messageEnvelope.RootElement.Deserialize<T>(deserializeOptions) ?? throw new InvalidOperationException("Failed to deserialize message");
    }

    /// <summary>
    /// Deserializes the message body of a ServiceBusReceivedMessage and unwraps the root element from the envelope.
    /// This is necessary for ClamAV where the message is forwarded with a LogicApp
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="serviceBusReceivedMessage"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static async Task<T> DeserializeAndUnwrapRootAsync<T>(
        this ServiceBusReceivedMessage serviceBusReceivedMessage)
    {
        var messageEnvelope = await JsonDocument.ParseAsync(serviceBusReceivedMessage.Body.ToStream());

        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        return messageEnvelope.RootElement.Deserialize<T>(deserializeOptions) ?? throw new InvalidOperationException("Failed to deserialize message");
    }
}
