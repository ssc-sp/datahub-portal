using ResourceProvisioner.Domain.Enums;

namespace ResourceProvisioner.Domain.Entities;

public class TerraformWorkspaceUser
{
    public string Guid { get; set; }
    public string Email { get; set; }
    public List<Role> Roles { get; set; }
}