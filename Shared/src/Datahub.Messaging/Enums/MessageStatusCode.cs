using System.Text.Json.Serialization;

namespace Datahub.Shared.Messaging;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageStatusCode
{
    Success,
    Error,
    NoChangesDetected,
    Partial
}