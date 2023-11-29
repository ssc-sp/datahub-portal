using System.Text.Json.Serialization;

namespace Datahub.Shared.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Role
{
    Admin,
    User,
    Owner,
    Guest,
    Removed
}