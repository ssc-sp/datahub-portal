using System.Text.Json.Serialization;

namespace ResourceProvisioner.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Role
{
    Admin,
    Member,
    Guest
}