using Datahub.Shared.Enums;

namespace Datahub.Shared.Entities;

public class TerraformUser
{
    public string ObjectId { get; set; }
    public string Email { get; set; }
    public Role Role { get; set; }
}