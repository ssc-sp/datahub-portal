namespace Datahub.Shared.Entities;

[Serializable]
public class WorkspaceDefinition
{
    public required List<TerraformTemplate> Templates { get; set; }
    public required TerraformWorkspace Workspace { get; set; }

    public required WorkspaceAppData AppData { get; set; } = new();

    public required string RequestingUserEmail { get; set; }

    public string? ResourceGroupName { get; set; }
    public string? CBRID { get; set; } = string.Empty;

    public bool UpdateWorkspaceVersion { get; set; } = false;
}
