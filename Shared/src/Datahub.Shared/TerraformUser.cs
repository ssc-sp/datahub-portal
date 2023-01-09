using Datahub.Shared.Enums;
using System.Collections.Generic;

namespace Datahub.Shared;

public class TerraformUser
{
    public string Guid { get; set; }
    public string Email { get; set; }
    public List<Role> Roles { get; set; }
}