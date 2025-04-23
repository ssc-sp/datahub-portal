namespace Datahub.Shared.Entities;
public class WorkspaceAppData
{
    public string DatabricksHostUrl { get; set; } = string.Empty;
    public string ResourceNameSuffix { get; set; } = string.Empty;
    public AppServiceConfiguration AppServiceConfiguration { get; set; } = null!;
    public PostgresConfiguration PostgresConfiguration { get; set; } = null!;
}