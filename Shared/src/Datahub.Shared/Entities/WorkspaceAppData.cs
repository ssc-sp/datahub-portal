namespace Datahub.Shared.Entities;
public class WorkspaceAppData
{
        public string DatabricksHostUrl { get; set; } = string.Empty;
        public AppServiceConfiguration AppServiceConfiguration { get; set; } = null!;
}