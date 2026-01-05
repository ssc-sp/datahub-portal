using Datahub.Shared.Enums;

namespace Datahub.Shared.Entities;

public class TerraformUser
{
    public required string ObjectId { get; set; }
    public required string Email { get; set; }
    public required Role Role { get; set; }
}
