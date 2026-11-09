using System;
using System.Collections.Generic;
using Datahub.Portal.Layout;
using Datahub.Portal.Pages.Workspace.Toolbox.ConfigurationForms;
using Datahub.Shared.Entities;
using Datahub.Shared.Entities.WorkspaceToolConfiguration;

namespace Datahub.Portal.Pages.Workspace.Toolbox
{
    public static class WorkspaceToolCatalog
    {
        internal static readonly IDictionary<string, VersionAwareWorkspaceToolInfo> VersionAwareTools = new Dictionary<string, VersionAwareWorkspaceToolInfo>
        {
            { TerraformTemplate.AzureDatabricks, new VersionAwareWorkspaceToolInfo
                {
                    ToolName = TerraformTemplate.AzureDatabricks,
                    ToolLabel = "Azure Databricks",
                    ToolCategory = "Compute & Analytics",
                    ToolDescription =
                        "Azure Databricks is a fast, easy, and collaborative Apache Spark-based analytics platform. Accelerate big data analytics and artificial intelligence (AI) solutions with Azure Databricks, a fast, easy and collaborative Apache Spark-based analytics service.",
                    ToolIcon = SidebarIcons.Databricks,
                    MinAvailableVersion = VersionAwareWorkspaceToolInfo.ALWAYS,
                    CanBeDeleted = false,
                    ConfigurationVersions =
                    [
                        new VersionAwareWorkspaceToolConfigInfo
                        {
                            MinVersion = new Version(5, 2, 0),
                            ConfigClass = typeof(DatabricksConfiguration),
                            ConfigDialogClass = typeof(DatabricksConfigurationForm)
                        },
                    ],
                    ToolCostInformation = ("The cost of Databricks is completely dependent on your usage. The idle costs of Databricks when not using it at all are nearly $0. Small compute clusters will cost about {0:C2} to {1:C2} per hour of usage, regular compute clusters will cost about {2:C2} to {3:C2} per hour of usage and large compute clusters will cost {4:C2} to {5:C2} per hour of usage. Additional costs may be incurred by other usage (data catalog, compute creation, etc.) and prices mentioned refer to default configurations. Make sure to read the additional information below for more details on costs.", [0.80, 2.40, 1.60, 4.80, 3.20, 9.60]),
                    AdditionalLinks = [
                        ("Azure Databricks documentation", "https://docs.microsoft.com/en-us/azure/databricks/"),
                        ("Azure Databricks pricing", "https://azure.microsoft.com/en-us/pricing/details/databricks/")
                    ],
                    ToolCostSummaryFunction = (config) =>
                    {
                        if (config is not DatabricksConfiguration dbConfig) return ("N/A", []);
                        var (minCost, maxCost) = dbConfig.GetMinMaxSelectedHourlyCosts();
                        return ("~ {0:C2} to {1:C2}/hour, depending on usage", [minCost, maxCost]);
                    }
                }
            },
            { TerraformTemplate.AzureDatabricksEnhanced, new VersionAwareWorkspaceToolInfo
                {
                    ToolName = TerraformTemplate.AzureDatabricksEnhanced,
                    ToolLabel = "Azure Databricks",
                    ToolCategory = "Compute & Analytics",
                    ToolDescription =
                        "Azure Databricks is a fast, easy, and collaborative Apache Spark-based analytics platform. Accelerate big data analytics and artificial intelligence (AI) solutions with Azure Databricks, a fast, easy and collaborative Apache Spark-based analytics service.",
                    ToolIcon = SidebarIcons.Databricks,
                    MinAvailableVersion = VersionAwareWorkspaceToolInfo.ALWAYS,
                    CanBeDeleted = false,
                    EnhancedPB = true,
                    ConfigurationVersions =
                    [
                        new VersionAwareWorkspaceToolConfigInfo
                        {
                            MinVersion = new Version(5, 2, 0),
                            ConfigClass = typeof(DatabricksConfigurationEnhanced),
                            ConfigDialogClass = typeof(DatabricksConfigurationForm)
                        },
                    ],
                    ToolCostInformation = ("The cost of Databricks is completely dependent on your usage. The idle costs of Databricks when not using it at all are nearly $0. Small compute clusters will cost about {0:C2} to {1:C2} per hour of usage, regular compute clusters will cost about {2:C2} to {3:C2} per hour of usage and large compute clusters will cost {4:C2} to {5:C2} per hour of usage. Additional costs may be incurred by other usage (data catalog, compute creation, etc.) and prices mentioned refer to default configurations. Make sure to read the additional information below for more details on costs.", [0.80, 2.40, 1.60, 4.80, 3.20, 9.60]),
                    AdditionalLinks = [
                        ("Azure Databricks documentation", "https://docs.microsoft.com/en-us/azure/databricks/"),
                        ("Azure Databricks pricing", "https://azure.microsoft.com/en-us/pricing/details/databricks/")
                    ],
                    ToolCostSummaryFunction = (config) =>
                    {
                        if (config is not DatabricksConfigurationEnhanced dbConfig) return ("N/A", []);
                        var (minCost, maxCost) = dbConfig.GetMinMaxSelectedHourlyCosts();
                        return ("~ {0:C2} to {1:C2}/hour, depending on usage", [minCost, maxCost]);
                    }
                }
            },
            { TerraformTemplate.NewProjectTemplate, new VersionAwareWorkspaceToolInfo
                {
                    ToolName = TerraformTemplate.NewProjectTemplate,
                    ToolLabel = "Azure Workspace and Storage Blob",
                    ToolCategory = "Storage",
                    ToolDescription =
                        "Azure Blob storage is Microsoft's object storage solution for the cloud. Blob storage is optimized for storing massive amounts of unstructured data, such as text or binary data.",
                    ToolIcon = SidebarIcons.Storage,
                    MinAvailableVersion = VersionAwareWorkspaceToolInfo.ALWAYS,
                    CanBeDeleted = false,
                    ToolCostInformation = ("Storage costs are about {0:C2} per terabyte of hot storage per month. Uploading and downloading to cloud storage also incurs bandwidth costs. See additional information below for details on costing.", [30.0]),
                    AdditionalLinks = [
                        ("Introduction to Azure Storage", "https://learn.microsoft.com/en-us/azure/storage/common/storage-introduction"),
                        ("Azure Storage pricing", "https://azure.microsoft.com/en-us/pricing/details/storage/blobs/")
                    ],
                    ToolCostSummaryFunction = (config) => ("~ {0:C2}/month, more per download/upload", [30.0m])
                }
            },
            { TerraformTemplate.NewProjectTemplateEnhanced, new VersionAwareWorkspaceToolInfo
                {
                    ToolName = TerraformTemplate.NewProjectTemplateEnhanced,
                    ToolLabel = "Azure Workspace and Storage Blob",
                    ToolCategory = "Storage",
                    ToolDescription =
                        "Azure Blob storage is Microsoft's object storage solution for the cloud. Blob storage is optimized for storing massive amounts of unstructured data, such as text or binary data.",
                    ToolIcon = SidebarIcons.Storage,
                    MinAvailableVersion = VersionAwareWorkspaceToolInfo.ALWAYS,
                    CanBeDeleted = false,
                    EnhancedPB = true,
                    ToolCostInformation = ("Storage costs are about {0:C2} per terabyte of hot storage per month. Uploading and downloading to cloud storage also incurs bandwidth costs. See additional information below for details on costing.", [30.0]),
                    AdditionalLinks = [
                        ("Introduction to Azure Storage", "https://learn.microsoft.com/en-us/azure/storage/common/storage-introduction"),
                        ("Azure Storage pricing", "https://azure.microsoft.com/en-us/pricing/details/storage/blobs/")
                    ],
                    ToolCostSummaryFunction = (config) => ("~ {0:C2}/month, more per download/upload", [30.0m])
                }
            },
            { TerraformTemplate.AzureAppService, new VersionAwareWorkspaceToolInfo
                {
                    ToolName = TerraformTemplate.AzureAppService,
                    ToolLabel = "Web Application",
                    ToolCategory = "Web",
                    ToolDescription =
                        "Web Application is a fully managed web hosting service for building web apps, mobile back ends, and RESTful APIs. It offers auto-scaling and high availability, supports both Windows and Linux, and enables automated deployments from GitHub, Azure DevOps, or any Git repo.",
                    ToolIcon = SidebarIcons.WebApp,
                    MinAvailableVersion = VersionAwareWorkspaceToolInfo.ALWAYS,
                    ToolCostInformation = ("The default App Service offered costs about {0:C2} per month, regardless of usage. Changing configurations will affect the cost of this resource. Stopping the web application does not stop the costs. Read more about this resource below.", [60.0]),
                    AdditionalLinks = [
                        ("Azure App Service documentation", "https://docs.microsoft.com/en-us/azure/app-service/"),
                        ("Azure App Service pricing", "https://azure.microsoft.com/en-us/pricing/details/app-service/")
                    ],
                    ConfigurationVersions =
                    [
                        new VersionAwareWorkspaceToolConfigInfo
                        {
                            MinVersion = VersionAwareWorkspaceToolInfo.ALWAYS,
                            ConfigClass = typeof(AppServiceConfiguration),
                        }
                    ],
                    ToolCostSummaryFunction = (config) => ("~ {0:C2}/month", [60.0m])
                }
            },
            { TerraformTemplate.AzureAppServiceEnhanced, new VersionAwareWorkspaceToolInfo
                {
                    ToolName = TerraformTemplate.AzureAppServiceEnhanced,
                    ToolLabel = "Web Application",
                    ToolCategory = "Web",
                    ToolDescription =
                        "Web Application is a fully managed web hosting service for building web apps, mobile back ends, and RESTful APIs. It offers auto-scaling and high availability, supports both Windows and Linux, and enables automated deployments from GitHub, Azure DevOps, or any Git repo.",
                    ToolIcon = SidebarIcons.WebApp,
                    MinAvailableVersion = VersionAwareWorkspaceToolInfo.ALWAYS,
                    EnhancedPB = true,
                    ToolCostInformation = ("The default App Service offered costs about {0:C2} per month, regardless of usage. Changing configurations will affect the cost of this resource. Stopping the web application does not stop the costs. Read more about this resource below.", [60.0]),
                    AdditionalLinks = [
                        ("Azure App Service documentation", "https://docs.microsoft.com/en-us/azure/app-service/"),
                        ("Azure App Service pricing", "https://azure.microsoft.com/en-us/pricing/details/app-service/")
                    ],
                    ConfigurationVersions =
                    [
                        new VersionAwareWorkspaceToolConfigInfo
                        {
                            MinVersion = VersionAwareWorkspaceToolInfo.ALWAYS,
                            ConfigClass = typeof(AppServiceConfigurationEnhanced),
                        }
                    ],
                    ToolCostSummaryFunction = (config) => ("~ {0:C2}/month", [60.0m])
                }
            },
            { TerraformTemplate.AzurePostgres, new VersionAwareWorkspaceToolInfo
                {
                    ToolName = TerraformTemplate.AzurePostgres,
                    ToolLabel = "Azure Postgres",
                    ToolCategory = "Databases",
                    ToolDescription =
                        "Azure Database for PostgreSQL is a relational database service based on the open-source Postgres database engine. It's a fully managed database-as-a-service offering that can handle mission-critical workloads with predictable performance, security, high availability, and dynamic scalability.",
                    ToolIcon = SidebarIcons.SqlDatabase,
                    MinAvailableVersion = VersionAwareWorkspaceToolInfo.ALWAYS,
                    ConfigurationVersions =
                    [
                        new VersionAwareWorkspaceToolConfigInfo
                        {
                            MinVersion = VersionAwareWorkspaceToolInfo.ALWAYS,
                            ConfigClass = typeof(PostgresConfiguration),
                            ConfigDialogClass = typeof(PostgresConfigurationForm)
                        }
                    ],
                    ToolCostInformation = ("The default Postgres offered costs about {0:C2} per month plus {1:C2} per month per GB of storage, regardless of usage. Changing configurations will affect the cost of this resource. Read more about this resource below.", [20.0, 0.18] ),
                    AdditionalLinks = [
                        ("Azure Database for PostgreSQL documentation", "https://docs.microsoft.com/en-us/azure/postgresql/"),
                        ("Azure Database for PostgreSQL pricing", "https://azure.microsoft.com/en-us/pricing/details/postgresql/")
                    ],
                    ToolCostSummaryFunction = (config) =>
                    {
                        if (config is not PostgresConfiguration pgConfig) return ("N/A", []);
                        var tier = PostgresTier.GetPostgresTiers()
                            .FirstOrDefault(t => t.PSQL_SKU == pgConfig.PSQL_SKU);
                        if (tier == null) return ("N/A", []);
                        return ("~ {0:C2}/month plus {1:C2}/month per GB of storage", [tier.MonthlyCost, 0.18m]);
                    }
                }
            },
            { TerraformTemplate.AzurePostgresEnhanced, new VersionAwareWorkspaceToolInfo
                {
                    ToolName = TerraformTemplate.AzurePostgresEnhanced,
                    ToolLabel = "Azure Postgres",
                    ToolCategory = "Databases",
                    ToolDescription =
                        "Azure Database for PostgreSQL is a relational database service based on the open-source Postgres database engine. It's a fully managed database-as-a-service offering that can handle mission-critical workloads with predictable performance, security, high availability, and dynamic scalability.",
                    ToolIcon = SidebarIcons.SqlDatabase,
                    MinAvailableVersion = VersionAwareWorkspaceToolInfo.ALWAYS,
                    EnhancedPB = true,
                    ConfigurationVersions =
                    [
                        new VersionAwareWorkspaceToolConfigInfo
                        {
                            MinVersion = VersionAwareWorkspaceToolInfo.ALWAYS,
                            ConfigClass = typeof(PostgresConfigurationEnhanced),
                            ConfigDialogClass = typeof(PostgresConfigurationForm)
                        }
                    ],
                    ToolCostInformation = ("The default Postgres offered costs about {0:C2} per month plus {1:C2} per month per GB of storage, regardless of usage. Changing configurations will affect the cost of this resource. Read more about this resource below.", [20.0, 0.18] ),
                    AdditionalLinks = [
                        ("Azure Database for PostgreSQL documentation", "https://docs.microsoft.com/en-us/azure/postgresql/"),
                        ("Azure Database for PostgreSQL pricing", "https://azure.microsoft.com/en-us/pricing/details/postgresql/")
                    ],
                    ToolCostSummaryFunction = (config) =>
                    {
                        if (config is not PostgresConfigurationEnhanced pgConfig) return ("N/A", []);
                        var tier = PostgresTier.GetPostgresTiers()
                            .FirstOrDefault(t => t.PSQL_SKU == pgConfig.PSQL_SKU);
                        if (tier == null) return ("N/A", []);
                        return ("~ {0:C2}/month plus {1:C2}/month per GB of storage", [tier.MonthlyCost, 0.18m]);
                    }
                }
            },
            { TerraformTemplate.AzureAPI, new VersionAwareWorkspaceToolInfo
                {
                    ToolName = TerraformTemplate.AzureAPI,
                    ToolLabel = "Azure API Management",
                    ToolCategory = "API",
                    ToolDescription =
                        "Azure API Management is a fully managed service that enables participants to publish, secure, transform, maintain, and monitor APIs. To use API Management, you must first create an Azure App Service.",
                    ToolIcon = SidebarIcons.Api,
                    MinAvailableVersion = VersionAwareWorkspaceToolInfo.UNDER_DEVELOPMENT,
                }
            },
            { TerraformTemplate.AzureAPIEnhanced, new VersionAwareWorkspaceToolInfo
                {
                    ToolName = TerraformTemplate.AzureAPIEnhanced,
                    ToolLabel = "Azure API Management",
                    ToolCategory = "API",
                    ToolDescription =
                        "Azure API Management is a fully managed service that enables participants to publish, secure, transform, maintain, and monitor APIs. To use API Management, you must first create an Azure App Service.",
                    ToolIcon = SidebarIcons.Api,
                    MinAvailableVersion = VersionAwareWorkspaceToolInfo.UNDER_DEVELOPMENT,
                    EnhancedPB = true,
                }
            }
        };
    }
}
