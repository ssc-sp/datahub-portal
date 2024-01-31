namespace Datahub.Shared.Entities;
#nullable enable
public class WorkspaceAppData
{
        public string DatabricksHostUrl { get; set; } = string.Empty;
        public AppServiceConfiguration? AppServiceConfiguration { get; set; }
}