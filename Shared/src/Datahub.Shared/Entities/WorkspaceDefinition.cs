namespace Datahub.Shared.Entities;

public class WorkspaceDefinition
{
    public List<TerraformTemplate> Templates { get; set; }
    public TerraformWorkspace Workspace { get; set; }

    public WorkspaceAppData AppData { get; set; } = new();

    public string RequestingUserEmail { get; set; }
}