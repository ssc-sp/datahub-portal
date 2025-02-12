using Datahub.Infrastructure.Services.Toolbox;
using Datahub.Portal.Layout;
using Datahub.Shared;
using Datahub.Shared.Entities;
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
            TerraformTemplate.AzureAppService,
            TerraformTemplate.AzurePostgres,
        ];


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

        private string GetLabel(string tool)
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

        private string GetCategory(string tool)
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

        private string GetDescription(string tool)
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

        private static string GetIcon(string tool)
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
        #endregion

        private void ShowInfoSheet(string tool)
        {
            var infoParams = new DialogParameters
            {
                { "Title", GetLabel(tool) },
                { "Description", GetDescription(tool) },
                { "Icon", GetIcon(tool) },
                { "Category", GetCategory(tool) },
            };

            var infoOptions = new DialogOptions
            {
                FullWidth = true,
                CloseOnEscapeKey = true,
                CloseButton = true,
                NoHeader = false,
                MaxWidth = MaxWidth.Large
            };
            _dialogService.Show<InfoSheet>(GetLabel(tool), infoParams, infoOptions);
        }
    }
}