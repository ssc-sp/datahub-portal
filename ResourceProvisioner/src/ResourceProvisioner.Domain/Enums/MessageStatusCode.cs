using System.Text.Json.Serialization;

namespace ResourceProvisioner.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageStatusCode
{
    Success,
    Error,
    NoChangesDetected,
    Partial
}