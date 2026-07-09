using System.Text.Json;

namespace Datahub.Functions.Extensions;

public static class QueueMessageExtensions
{
    public const string MessagePropertyName = "message";

    public static Task<T> DeserializeAndUnwrapMessageAsync<T>(this string queueMessage)
    {
        if (string.IsNullOrWhiteSpace(queueMessage))
        {
            throw new InvalidOperationException("Queue message is empty.");
        }

        using var document = JsonDocument.Parse(queueMessage);
        var root = document.RootElement;

        var payload = root.ValueKind == JsonValueKind.Object && root.TryGetProperty(MessagePropertyName, out var message)
            ? message
            : root;

        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        var result = payload.Deserialize<T>(deserializeOptions);
        return Task.FromResult(result ?? throw new InvalidOperationException("Failed to deserialize message"));
    }
}
