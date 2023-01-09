using Datahub.Shared.Enums;
using System.Collections.Generic;

namespace Datahub.Shared.Entities;

public class TerraformUser
{
    public string Guid { get; set; }
    public string Email { get; set; }
    public IEnumerator<Role> Roles { get; set; }
}