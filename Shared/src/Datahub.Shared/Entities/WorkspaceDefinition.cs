using System.Collections.Generic;

namespace Datahub.Shared.Entities;

public class WorkspaceDefinition
{
    public List<TerraformTemplate> Templates { get; set; }
    public TerraformWorkspace Workspace { get; set; }

    public string RequestingUserEmail { get; set; }
}