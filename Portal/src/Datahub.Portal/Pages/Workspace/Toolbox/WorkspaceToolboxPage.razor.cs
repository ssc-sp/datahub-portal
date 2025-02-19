using System.Linq.Dynamic.Core;
using System.Text.Json;
using Datahub.Application.Services.Toolbox;
using Datahub.Infrastructure.Services.Toolbox;
using Datahub.Portal.Layout;
using Datahub.Shared;
using Datahub.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace Datahub.Portal.Pages.Workspace.Toolbox
{
    public partial class WorkspaceToolboxPage
    {
        #region Tool utilities

        private readonly List<string> _toolList =
        [
            TerraformTemplate.NewProjectTemplate,
            TerraformTemplate.AzureDatabricks,
            TerraformTemplate.AzureStorageBlob,
            TerraformTemplate.AzureAppService,
            TerraformTemplate.AzurePostgres,
            TerraformTemplate.AzureArcGis,
            TerraformTemplate.AzureAPI
        ];

        private readonly List<string> _permanentToolList =
        [
            TerraformTemplate.AzureDatabricks,
            TerraformTemplate.AzureStorageBlob,
            TerraformTemplate.NewProjectTemplate
        ];

        private readonly List<string> _configurableToolList =
        [
            TerraformTemplate.AzurePostgres,
        ];

        private bool IsConfigurable(ToolboxTransaction transaction) =>
            _configurableToolList.Contains(transaction.Tool) && transaction.Type != ToolboxTransactionType.Remove;


        internal record struct AvailabilityStatus
        {
            public const string Available = "Available";
            public const string UnderDevelopment = "Under Development";
            public const string MetadataRequired = "Metadata Required";
            public const string Disabled = "Disabled";
        }

        private readonly Dictionary<string, string> _toolAvailabilityStatusMap = new()
        {
            { TerraformTemplate.NewProjectTemplate, AvailabilityStatus.Available },
            { TerraformTemplate.AzureDatabricks, AvailabilityStatus.Available },
            { TerraformTemplate.AzureStorageBlob, AvailabilityStatus.Available },
            { TerraformTemplate.AzureAppService, AvailabilityStatus.Available },
            { TerraformTemplate.AzurePostgres, AvailabilityStatus.Available },
            { TerraformTemplate.AzureArcGis, AvailabilityStatus.UnderDevelopment },
            { TerraformTemplate.AzureAPI, AvailabilityStatus.UnderDevelopment }
        };

        private readonly Dictionary<string, string> _toolDisplayStatusMap = new();

        private string ToolLabel(string tool)
        {
            return tool switch
            {
                TerraformTemplate.NewProjectTemplate => Localizer["Azure Workspace Essentials"],
                TerraformTemplate.AzureDatabricks => Localizer["Azure Databricks"],
                TerraformTemplate.AzureStorageBlob => Localizer["Azure Storage Blob"],
                TerraformTemplate.AzureAppService => Localizer["Web Application"],
                TerraformTemplate.AzurePostgres => Localizer["Azure Postgres"],
                TerraformTemplate.AzureArcGis => Localizer["Azure ArcGIS"],
                TerraformTemplate.AzureAPI => Localizer["Azure API Management"],
                _ => tool
            };
        }

        private string ToolCategory(string tool)
        {
            return tool switch
            {
                TerraformTemplate.NewProjectTemplate => Localizer["Core"],
                TerraformTemplate.AzureDatabricks => Localizer["Compute & Analytics"],
                TerraformTemplate.AzureStorageBlob => Localizer["Storage"],
                TerraformTemplate.AzureAppService => Localizer["Web"],
                TerraformTemplate.AzurePostgres => Localizer["Databases"],
                TerraformTemplate.AzureArcGis => Localizer["Data & Analytics"],
                TerraformTemplate.AzureAPI => Localizer["API"],
                _ => tool
            };
        }

        private string ToolDescription(string tool)
        {
            return tool switch
            {
                TerraformTemplate.NewProjectTemplate => Localizer[
                    "The basic tools required to host your workspace. This includes Azure Keyvault, some monitoring and a virtual network, among other things."],
                TerraformTemplate.AzureDatabricks => Localizer[
                    "Azure Databricks is a fast, easy, and collaborative Apache Spark-based analytics platform. Accelerate big data analytics and artificial intelligence (AI) solutions with Azure Databricks, a fast, easy and collaborative Apache Spark-based analytics service."],
                TerraformTemplate.AzureStorageBlob => Localizer[
                    "Azure Blob storage is Microsoft's object storage solution for the cloud. Blob storage is optimized for storing massive amounts of unstructured data, such as text or binary data."],
                TerraformTemplate.AzureAppService => Localizer[
                    "Web Application is a fully managed web hosting service for building web apps, mobile back ends, and RESTful APIs. It offers auto-scaling and high availability, supports both Windows and Linux, and enables automated deployments from GitHub, Azure DevOps, or any Git repo."],
                TerraformTemplate.AzurePostgres => Localizer[
                    "Azure Database for PostgreSQL is a relational database service based on the open-source Postgres database engine. It's a fully managed database-as-a-service offering that can handle mission-critical workloads with predictable performance, security, high availability, and dynamic scalability."],
                TerraformTemplate.AzureArcGis => Localizer[
                    "ArcGIS is a geographic information system (GIS) for working with maps and geographic information. It is used for creating and using maps, compiling geographic data, analyzing mapped information, sharing and discovering geographic information, using maps and geographic information in a range of applications, and managing geographic information in a database."],
                TerraformTemplate.AzureAPI => Localizer[
                    "Azure API Management is a fully managed service that enables participants to publish, secure, transform, maintain, and monitor APIs. To use API Management, you must first create an Azure App Service."],
                _ => tool
            };
        }

        private static string ToolIcon(string tool)
        {
            return tool switch
            {
                TerraformTemplate.NewProjectTemplate => SidebarIcons.Workspace,
                TerraformTemplate.AzureDatabricks => SidebarIcons.Databricks,
                TerraformTemplate.AzureStorageBlob => SidebarIcons.Storage,
                TerraformTemplate.AzureAppService => SidebarIcons.WebApp,
                TerraformTemplate.AzurePostgres => SidebarIcons.SqlDatabase,
                TerraformTemplate.AzureArcGis => SidebarIcons.ArcGis,
                TerraformTemplate.AzureAPI => SidebarIcons.Api,
                _ => SidebarIcons.Default
            };
        }

        #endregion

        #region Form methods

        private void AddTool(string tool)
        {
            Log($"Adding tool: {tool}");
            _transactions.AddTool(tool, OriginalData(tool));
            var dependencies = TerraformTemplate.GetDependenciesToCreate(tool);
            dependencies.ForEach(dependency =>
            {
                if (_workspaceDefinition.Templates.All(template => template.Name != dependency.Name) &&
                    _transactions.All(transaction => transaction.Tool != dependency.Name))
                {
                    Log($"Adding dependency: {dependency.Name}");
                    _transactions.AddTool(dependency.Name, OriginalData(tool));
                }
            });
        }

        private void RemoveTool(string tool)
        {
            Log($"Removing tool: {tool}");
            _transactions.RemoveTool(tool);
        }

        private void UpdateTool(string tool)
        {
            Log($"Updating tool: {tool}");
            _transactions.UpdateTool(tool, OriginalData(tool), UpdatedData(tool));
        }

        private void RevertTool(ToolboxTransaction transaction)
        {
            Log($"Reverting {transaction.Type.ToString().ToUpper()} of tool: {transaction.Tool}");

            // If the tool that is being reverted is the dependency of another tool that is being added, also revert that tool
            var dependentTools = _transactions.Where(tr => tr.Type == ToolboxTransactionType.Add && TerraformTemplate
                .GetDependenciesToCreate(tr.Tool).Any(dependency => dependency.Name == transaction.Tool)).ToList();
            dependentTools.ForEach(tool =>
            {
                Log(
                    $"Reverting {tool.Type.ToString().ToUpper()} tool: {tool.Tool} as it depends on {transaction.Tool}");
                _transactions.Revert(tool);
            });

            _transactions.Revert(transaction);
        }


        private object OriginalData(string tool)
        {
            switch (tool)
            {
                case TerraformTemplate.AzurePostgres:
                    if (_workspaceDefinition.AppData.PostgresConfiguration == null)
                    {
                        Log("No original configuration found for Azure Postgres. Creating new configuration.");
                        return new PostgresConfiguration();
                    }

                    return _workspaceDefinition.AppData.PostgresConfiguration;
                default:
                    return null;
            }
        }

        private object UpdatedData(string tool)
        {
            switch (tool)
            {
                case TerraformTemplate.AzurePostgres:
                    if (_workspaceDefinition.AppData.PostgresConfiguration?.PSQL_SKU == null)
                    {
                        Log("No original configuration found for Azure Postgres. Creating new configuration.");
                        return new PostgresConfiguration();
                    }

                    return new PostgresConfiguration
                    {
                        PSQL_SKU = _workspaceDefinition.AppData.PostgresConfiguration.PSQL_SKU
                    };
                default:
                    return null;
            }
        }


        private void PopulateCatalog()
        {
            _toolList.ForEach(tool =>
            {
                if (_workspaceDefinition.Templates.All(template => template.Name != tool))
                {
                    _toolCatalog.Add(tool);
                }
            });
        }

        private async Task ShowInfoSheet(string tool)
        {
            var infoParams = new DialogParameters
            {
                { "Title", ToolLabel(tool) },
                { "Description", ToolDescription(tool) },
                { "Icon", ToolIcon(tool) },
                { "Category", ToolCategory(tool) },
                { "Dependencies", ToolDependencies(tool) },
                { "Instances", await ToolInstances(tool) }
            };

            var infoOptions = new DialogOptions
            {
                FullWidth = true,
                CloseOnEscapeKey = true,
                CloseButton = true,
                MaxWidth = MaxWidth.Large
            };
            _dialogService.Show<InfoSheet>(ToolLabel(tool), infoParams, infoOptions);
        }

        private async Task<int> ToolInstances(string tool)
        {
            var ctx = await _contextFactory.CreateDbContextAsync();
            return ctx.Project_Resources2
                .AsNoTracking()
                .Count(r => r.ResourceType == TerraformTemplate.GetTerraformServiceType(tool));
        }

        private (string Icon, string Name)[] ToolDependencies(string tool)
        {
            var dependencies = TerraformTemplate.GetDependenciesToCreate(tool);
            return dependencies.Select(dependency => (ToolIcon(dependency.Name), ToolLabel(dependency.Name))).ToArray();
        }

        private string DisplayDiff(Dictionary<string, (object Original, object Updated)> diff)
        {
            var diffString = "";
            foreach (var (key, value) in diff)
            {
                var originalValue = value.Original;
                var updatedValue = value.Updated;
                if (originalValue == null)
                {
                    diffString += Localizer["Added {0}: {1}\n", PropertyLabel(key), updatedValue];
                }
                else
                {
                    diffString += Localizer["Updated {0}: {1} -> {2}\n", PropertyLabel(key), originalValue,
                        updatedValue];
                }
            }

            return diffString;
        }

        private string PropertyLabel(string propertyName)
        {
            return propertyName switch
            {
                "PSQL_SKU" => Localizer["Database tier"],
                _ => propertyName
            };
        }

        #endregion

        #region Admin utils

        private void Log(string message, string type = "info")
        {
            var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{type.ToUpper()}] {message}";
            switch (type)
            {
                case "info":
                    _adminEventLogs.Add(logMessage);
                    _logger.LogInformation(message);
                    break;
                case "warn":
                    _adminEventLogs.Add(logMessage);
                    _logger.LogWarning(message);
                    break;
                case "error":
                    _adminEventLogs.Add(logMessage);
                    _logger.LogError(message);
                    break;
            }
        }

        private string WorkspaceDefinitionMarkdown(WorkspaceDefinition workspaceDefinition)
        {
            var workspaceDefinitionJsonString = JsonSerializer.Serialize(workspaceDefinition,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                });

            return $"```json\n{workspaceDefinitionJsonString}\n```";
        }

        private string LinkRewriter(string link)
        {
            return link;
        }

        #endregion
    }
}