namespace Datahub.Shared.Entities;

[Serializable]
public class WorkspaceDefinition
{
    public List<TerraformTemplate> Templates { get; set; }
    public TerraformWorkspace Workspace { get; set; }

    public WorkspaceAppData AppData { get; set; } = new();

    public string RequestingUserEmail { get; set; }

    public string ResourceGroupName { get; set; } = string.Empty;
    public string CBRID { get; set; } = string.Empty;

    public bool UpdateWorkspaceVersion { get; set; } = false;
}