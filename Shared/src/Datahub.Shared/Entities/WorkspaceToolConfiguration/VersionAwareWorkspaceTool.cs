using System;
using System.Collections.Generic;
using System.Text;

namespace Datahub.Shared.Entities.WorkspaceToolConfiguration;

#nullable enable
public class VersionAwareWorkspaceToolInfo
{
    #region Tool Definitions
    public static readonly ISet<VersionAwareWorkspaceToolInfo> VERSION_AWARE_WORKSPACE_TOOLS = new HashSet<VersionAwareWorkspaceToolInfo>
    {
        new()
        {
            ToolName = TerraformTemplate.NewProjectTemplate,
            ToolLabel = "Azure Workspace Essentials",
            ToolCategory = "Core",
            ToolDescription =
                "The basic tools required to host your workspace. This includes Azure Key vault, some monitoring and a virtual network, among other things.",
            //ToolIcon = SidebarIcons.Workspace,
            MinAvailableVersion = ALWAYS!,
            CanBeDeleted = false
        },
        new()
        {
            ToolName = TerraformTemplate.AzureDatabricks,
            ToolLabel = "Azure Databricks",
            ToolCategory = "Compute & Analytics",
            ToolDescription =
                "Azure Databricks is a fast, easy, and collaborative Apache Spark-based analytics platform. Accelerate big data analytics and artificial intelligence (AI) solutions with Azure Databricks, a fast, easy and collaborative Apache Spark-based analytics service.",
            //ToolIcon = SidebarIcons.Databricks,
            ToolDependencies = [ TerraformTemplate.NewProjectTemplate, TerraformTemplate.AzureStorageBlob],
            MinAvailableVersion = ALWAYS !,
            CanBeDeleted = false,
            ConfigurationVersions =
            [
                new VersionAwareWorkspaceToolConfigInfo
                {
                    MinVersion = new Version(5, 2, 0)
                    //TODO config details
                }
            ]
        },
        new()
        {
            ToolName = TerraformTemplate.AzureStorageBlob,
            ToolLabel = "Azure Storage Blob",
            ToolCategory = "Storage",
            ToolDescription =
                "Azure Blob storage is Microsoft's object storage solution for the cloud. Blob storage is optimized for storing massive amounts of unstructured data, such as text or binary data.",
            //ToolIcon = SidebarIcons.Storage,
            ToolDependencies = [ TerraformTemplate.NewProjectTemplate],
            MinAvailableVersion = ALWAYS !,
            CanBeDeleted = false
        },
        new()
        {
            ToolName = TerraformTemplate.AzureAppService,
            ToolLabel = "Web Application",
            ToolCategory = "Web",
            ToolDescription =
                "Web Application is a fully managed web hosting service for building web apps, mobile back ends, and RESTful APIs. It offers auto-scaling and high availability, supports both Windows and Linux, and enables automated deployments from GitHub, Azure DevOps, or any Git repo.",
            //ToolIcon = SidebarIcons.WebApp,
            ToolDependencies = [ TerraformTemplate.NewProjectTemplate, TerraformTemplate.AzureStorageBlob],
            MinAvailableVersion = ALWAYS !,
        },
        new()
        {
            ToolName = TerraformTemplate.AzurePostgres,
            ToolLabel = "Azure Postgres",
            ToolCategory = "Databases",
            ToolDescription =
                "Azure Database for PostgreSQL is a relational database service based on the open-source Postgres database engine. It's a fully managed database-as-a-service offering that can handle mission-critical workloads with predictable performance, security, high availability, and dynamic scalability.",
            //ToolIcon = SidebarIcons.SqlDatabase,
            ToolDependencies = [ TerraformTemplate.NewProjectTemplate],
            MinAvailableVersion = ALWAYS !,
            ConfigurationVersions =
            [
                new VersionAwareWorkspaceToolConfigInfo
                {
                    MinVersion = ALWAYS !
                }
            ]
        },
        new()
        {
            ToolName = TerraformTemplate.AzureArcGis,
            ToolLabel = "Azure ArcGIS",
            ToolCategory = "Data & Analytics",
            ToolDescription =
                "ArcGIS is a geographic information system (GIS) for working with maps and geographic information. It is used for creating and using maps, compiling geographic data, analyzing mapped information, sharing and discovering geographic information, using maps and geographic information in a range of applications, and managing geographic information in a database.",
            //ToolIcon = SidebarIcons.ArcGis,
            MinAvailableVersion = FAR_FUTURE !,
        },
        new()
        {
            ToolName = TerraformTemplate.AzureAPI,
            ToolLabel = "Azure API Management",
            ToolCategory = "API",
            ToolDescription =
                "Azure API Management is a fully managed service that enables participants to publish, secure, transform, maintain, and monitor APIs. To use API Management, you must first create an Azure App Service.",
            //ToolIcon = SidebarIcons.Api,
            MinAvailableVersion = FAR_FUTURE !,
        },
    };
    #endregion

    public static readonly Version FAR_FUTURE = new(9999, 12, 31);
    public static readonly Version ALWAYS = new(1, 0, 0);

    public string ToolName { get; set; } = string.Empty;
    public string ToolLabel { get; set; } = string.Empty;
    public string ToolCategory { get; set; } = string.Empty;
    public string ToolDescription { get; set; } = string.Empty;
    public string ToolIcon { get; set; } = string.Empty;
    public IEnumerable<string> ToolDependencies { get; set; } = Array.Empty<string>();
    public Version MinAvailableVersion { get; set; } = ALWAYS;
    public bool CanBeDeleted { get; set; } = true;
    public IEnumerable<VersionAwareWorkspaceToolConfigInfo> ConfigurationVersions { get; set; } = Array.Empty<VersionAwareWorkspaceToolConfigInfo>();

    public bool IsAvailable(Version workspaceVersion) => MinAvailableVersion <= workspaceVersion;

    public bool IsConfigurable(Version workspaceVersion) => ConfigurationVersions != null && ConfigurationVersions.Any(c => c.MinVersion <= workspaceVersion);

    public VersionAwareWorkspaceToolConfigInfo? GetApplicableConfigInfo(Version workspaceVersion)
    {
        if (ConfigurationVersions == null || !ConfigurationVersions.Any())
        {
            return null;
        }
        // Get the config with the highest MinVersion that is less than or equal to the workspaceVersion
        return ConfigurationVersions
            .Where(c => c.MinVersion <= workspaceVersion)
            .OrderByDescending(c => c.MinVersion)
            .FirstOrDefault();
    }
}
public class VersionAwareWorkspaceToolConfigInfo
{
    public Version MinVersion { get; set; } = VersionAwareWorkspaceToolInfo.ALWAYS;
    // TODO config methods
}
