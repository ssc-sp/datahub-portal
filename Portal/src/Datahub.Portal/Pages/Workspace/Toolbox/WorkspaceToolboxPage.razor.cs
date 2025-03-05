using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Datahub.Application.Services;
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

        // List of possible tools
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

        // List of tools that cannot be deleted
        private readonly List<string> _permanentToolList =
        [
            TerraformTemplate.AzureDatabricks,
            TerraformTemplate.AzureStorageBlob,
            TerraformTemplate.NewProjectTemplate
        ];

        // List of tools that can be configured
        private readonly List<string> _configurableToolList =
        [
            TerraformTemplate.AzurePostgres,
        ];

        /// <summary>
        /// Checks if a transaction concerns a configurable tool and that the transaction is to configure or add.
        /// </summary>
        /// <param name="transaction">The transaction to check.</param>
        /// <returns>True if the tool is configurable and the transaction is not a removal, otherwise false.</returns>
        private bool IsConfigurable(ToolboxTransaction transaction) =>
            _configurableToolList.Contains(transaction.Tool) && transaction.Type != ToolboxTransactionType.Remove;

        // Availability status options for our tools
        internal record struct AvailabilityStatus
        {
            public const string Available = "Available";
            public const string UnderDevelopment = "Under Development";
            public const string MetadataRequired = "Metadata Required";
            public const string Disabled = "Disabled";
        }

        /// <summary>
        /// Gets the label associated with each availability status.
        /// </summary>
        /// <param name="status">The availability status.</param>
        /// <returns>The localized label for the status.</returns>
        private string AvailabilityLabel(string status) => status switch
        {
            AvailabilityStatus.Available => Localizer["Available"],
            AvailabilityStatus.UnderDevelopment => Localizer["Under Development"],
            AvailabilityStatus.MetadataRequired => Localizer["Metadata Required"],
            AvailabilityStatus.Disabled => Localizer["Disabled"],
            _ => status
        };

        // Map of tools to their availability status
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

        /// <summary>
        /// Gets the display label for each tool.
        /// </summary>
        /// <param name="tool">The tool identifier.</param>
        /// <returns>The localized label for the tool.</returns>
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

        /// <summary>
        /// Gets the category for each tool.
        /// </summary>
        /// <param name="tool">The tool identifier.</param>
        /// <returns>The localized category for the tool.</returns>
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

        /// <summary>
        /// Gets the description for each tool.
        /// </summary>
        /// <param name="tool">The tool identifier.</param>
        /// <returns>The localized description for the tool.</returns>
        private string ToolDescription(string tool)
        {
            return tool switch
            {
                TerraformTemplate.NewProjectTemplate => Localizer[
                    "The basic tools required to host your workspace. This includes Azure Key vault, some monitoring and a virtual network, among other things."],
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

        /// <summary>
        /// Gets the icon for each tool.
        /// </summary>
        /// <param name="tool">The tool identifier.</param>
        /// <returns>The icon identifier for the tool.</returns>
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

        /// <summary>
        /// Calculates how many instances of a tool are currently in use.
        /// </summary>
        /// <param name="tool">The tool identifier.</param>
        /// <returns>The number of instances of the tool in use.</returns>
        private async Task<int> ToolInstances(string tool)
        {
            var ctx = await ContextFactory.CreateDbContextAsync();
            return ctx.Project_Resources2
                .AsNoTracking()
                .Count(r => r.ResourceType == TerraformTemplate.GetTerraformServiceType(tool));
        }

        /// <summary>
        /// Gets the dependencies for each tool.
        /// </summary>
        /// <param name="tool">The tool identifier.</param>
        /// <returns>An array of tuples containing the icon and name of each dependency.</returns>
        private (string Icon, string Name)[] ToolDependencies(string tool)
        {
            try
            {
                var dependencies = TerraformTemplate.GetDependenciesToCreate(tool);
                return dependencies.Select(dependency => (ToolIcon(dependency.Name), ToolLabel(dependency.Name)))
                    .ToArray();
            }
            catch
            {
                return [];
            }
        }

        /// <summary>
        /// Long form cost information for each resource
        /// </summary>
        /// <param name="tool">The tool to get cost information for</param>
        /// <returns>A localized string providing cost information</returns>
        private string ToolCostInformation(string tool)
        {
            return tool switch
            {
                TerraformTemplate.NewProjectTemplate => Localizer[
                    "Workspace essentials are the backbone of your workspace in the cloud, and costs related to the various resources this includes sum up to less than {0:C2} per month.",
                    1.0],
                TerraformTemplate.AzureDatabricks => Localizer[
                    "The cost of Databricks is completely dependent on your usage. The idle costs of Databricks when not using it at all are nearly $0. Small compute clusters will cost about {0:C2} to {1:C2} per hour of usage, regular compute clusters will cost about {2:C2} to {3:C2} per hour of usage and large compute clusters will cost {4:C2} to {5:C2} per hour of usage. Additional costs may be incurred by other usage (data catalog, compute creation, etc.) and prices mentioned refer to default configurations. Make sure to read the additional information below for more details on costs.",
                    0.80, 2.40, 1.60, 4.80, 3.20, 9.60],
                TerraformTemplate.AzureStorageBlob => Localizer[
                    "Storage costs are about {0:C2} per terabyte of hot storage per month. Uploading and downloading to cloud storage also incurs bandwidth costs. See additional information below for details on costing.",
                    30.0],
                TerraformTemplate.AzurePostgres => Localizer[
                    "The default Postgres offered costs about {0:C2} per month plus {1:C2} per month per GB of storage, regardless of usage. Changing configurations will affect the cost of this resource. Read more about this resource below.",
                    20.0, 0.18],
                TerraformTemplate.AzureAppService => Localizer[
                    "The default App Service offered costs about {0:C2} per month, regardless of usage. Changing configurations will affect the cost of this resource. Stopping the web application does not stop the costs. Read more about this resource below.",
                    60.0],
                _ => Localizer["No cost information available for this resource."]
            };
        }

        /// <summary>
        /// Short form cost calculation for each transaction
        /// </summary>
        /// <param name="transaction">The transaction to calculate the costs for</param>
        /// <returns>A localized summary for costs for the tool</returns>
        private string ToolCostSummary(ToolboxTransaction transaction)
        {
            if (transaction.Type == ToolboxTransactionType.Remove) return string.Empty;

            switch (transaction.Tool)
            {
                case TerraformTemplate.NewProjectTemplate:
                    return Localizer["< {0:C2}/month", 1.0m];
                case TerraformTemplate.AzureStorageBlob:
                    return Localizer["~ {0:C2}/month, more per download/upload", 30.0m];
                case TerraformTemplate.AzurePostgres:
                    var postgresConfig = (PostgresConfiguration)transaction.UpdatedData;
                    var postgresCost = PostgresTier.GetPostgresTiers()
                        .First(t => t.PSQL_SKU == postgresConfig!.PSQL_SKU)
                        .Cost;
                    return Localizer["~ {0:C2} plus {1:C2}/month per GB of storage", postgresCost, 0.18m];
                case TerraformTemplate.AzureAppService:
                    return Localizer["~ {0:C2}/month", 60.0m];
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// List of additional links for each tool
        /// </summary>
        /// <param name="tool">The tool to get additional links for</param>
        /// <returns>A list of tuples of text/URL for additional info on each tool</returns>
        private (string Text, string URL)[] ToolAdditionalLinks(string tool)
        {
            return tool switch
            {
                TerraformTemplate.AzureStorageBlob =>
                [
                    (Localizer["Introduction to Azure Storage"],
                        Localizer["https://learn.microsoft.com/en-us/azure/storage/common/storage-introduction"]),
                    (Localizer["Azure Storage pricing"],
                        Localizer["https://azure.microsoft.com/en-us/pricing/details/storage/blobs/"]),
                ],
                TerraformTemplate.AzurePostgres =>
                [
                    (Localizer["Azure Database for PostgreSQL documentation"],
                        Localizer["https://docs.microsoft.com/en-us/azure/postgresql/"]),
                    (Localizer["Azure Database for PostgreSQL pricing"],
                        Localizer["https://azure.microsoft.com/en-us/pricing/details/postgresql/"]),
                ],
                TerraformTemplate.AzureDatabricks =>
                [
                    (Localizer["Azure Databricks documentation"],
                        Localizer["https://docs.microsoft.com/en-us/azure/databricks/"]),
                    (Localizer["Azure Databricks pricing"],
                        Localizer["https://azure.microsoft.com/en-us/pricing/details/databricks/"]),
                ],
                TerraformTemplate.AzureAppService =>
                [
                    (Localizer["Azure App Service documentation"],
                        Localizer["https://docs.microsoft.com/en-us/azure/app-service/"]),
                    (Localizer["Azure App Service pricing"],
                        Localizer["https://azure.microsoft.com/en-us/pricing/details/app-service/"]),
                ],
                _ => Array.Empty<(string, string)>()
            };
        }

        /// <summary>
        /// Converts a difference dictionary into a human-readable string.
        /// </summary>
        /// <param name="diff">The difference dictionary.</param>
        /// <returns>A human-readable string representing the differences.</returns>
        private string DisplayDiff(Dictionary<string, (object Original, object Updated)> diff)
        {
            var diffString = "";
            foreach (var (key, value) in diff)
            {
                var originalValue = value.Original;
                var updatedValue = value.Updated;
                if (originalValue == null)
                {
                    diffString += Localizer["Selected {0}: {1}\n", PropertyLabel(key).ToLower(), updatedValue];
                }
                else
                {
                    diffString += Localizer["Updated {0}: {1} -> {2}\n", PropertyLabel(key).ToLower(), originalValue,
                        updatedValue];
                }
            }

            return diffString;
        }

        /// <summary>
        /// Converts a configuration property name into a human-readable string.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The localized label for the property.</returns>
        private string PropertyLabel(string propertyName)
        {
            return propertyName switch
            {
                "PSQL_SKU" => Localizer["Database tier"],
                _ => propertyName
            };
        }

        #endregion

        #region Form methods

        /// <summary>
        /// Goes to the next step in the given stepper.
        /// </summary>
        /// <param name="stepper">The MudStepper to advance to the next step.</param>
        private async Task NextStep(MudStepper stepper)
        {
            if (stepper.ActiveStep == stepper.Steps.Last())
            {
                Log("Completing request");
                _completed = true;
                StateHasChanged();
                await CompleteRequest();
            }
            else
            {
                Log("Next step");
                await stepper.NextStepAsync();
            }
        }

        /// <summary>
        /// Goes to the previous step in the given stepper.
        /// </summary>
        /// <param name="stepper">The MudStepper to go back in.</param>
        private async Task PreviousStep(MudStepper stepper)
        {
            Log("Previous step");
            await stepper.PreviousStepAsync();
        }

        /// <summary>
        /// Completes the request by going through the completion steps provided and automatically logs and profiles each step.
        /// If any step fails, the process is halted and the request is not completed.
        /// </summary>
        private async Task CompleteRequest()
        {
            _completionSteps =
            [
                new CompletionStep { Label = Localizer["Verifying request"], State = "", Task = VerifyRequest },
                new CompletionStep { Label = Localizer["Creating local records"], State = "", Task = LocalRecords },
                new CompletionStep
                    { Label = Localizer["Requesting cloud provisioning"], State = "", Task = CloudRequest }
            ];

            _context = await ContextFactory.CreateDbContextAsync();
            _builtWorkspaceDefinition = ToolboxService.ApplyTransaction(_workspaceDefinition, _transactions);

            foreach (var step in _completionSteps)
            {
                await Task.Delay(1000);
                Log($"Beginning completion step: {step.Label}");
                var timer = new Stopwatch();
                timer.Start();
                try
                {
                    step.State = ActiveState;
                    StateHasChanged();
                    await step.Task();
                    Log($"Completed step: {step.Label} in {timer.ElapsedMilliseconds}ms");
                    step.State = CompletedState;
                }
                catch (Exception e)
                {
                    Log($"Failed step: {step.Label} in {timer.ElapsedMilliseconds}ms", "error");
                    Log(e.Message, "error");
                    step.State = FailedState;
                    break;
                }
                finally
                {
                    timer.Stop();
                    step.Time = timer.ElapsedMilliseconds;
                    StateHasChanged();
                }
            }

            if (_completionSteps.Any(step => step.State == FailedState))
            {
                Log("Request failed", "error");
                await _context.DisposeAsync();
                return;
            }

            Log("Request completed successfully");
            await Task.Delay(4000);

            if (!_mockRequest)
            {
                Log("Saving changes to database");
                await _context.TrackSaveChangesAsync(AuditingService);
            }
            else
            {
                Log("Mock request enabled. Disposing database changes");
            }

            await _context.DisposeAsync();
            if (_redirectOnCompletion)
            {
                NavigationManager.NavigateTo($"/{PageRoutes.WorkspacePrefix}/{WorkspaceAcronym}");
            }
        }

        /// <summary>
        /// Verifies the request by checking the workspace state, existing resources, and built workspace definition.
        /// </summary>
        private async Task VerifyRequest()
        {
            var workspace = await _context
                .Projects
                .AsNoTracking()
                .Include(p => p.Resources)
                .Include(p => p.Credits)
                .Include(p => p.Users)
                .FirstAsync(p => p.Project_Acronym_CD == WorkspaceAcronym);

            Log("Checking workspace state");
            if (workspace.IsDeleted) throw new Exception("Workspace has been deleted");
            if (workspace.Users.Count == 0) throw new Exception("Workspace has no users");
            if (workspace.IsOverBudget) throw new Exception("Workspace is over budget");

            Log("Checking workspace for existing resources");
            if (_transactions.Where(tr => tr.Type == ToolboxTransactionType.Add).Any(tr =>
                    workspace.Resources.Any(r => r.ResourceType == TerraformTemplate.GetTerraformServiceType(tr.Tool))))
            {
                Log("Workspace already has one or more of the requested resources", "warn");
            }

            Log("Checking resources to delete");
            var resourceToDelete = _transactions.Where(tr => tr.Type == ToolboxTransactionType.Remove)
                .Select(tr => workspace.Resources.First(r =>
                    r.ResourceType == TerraformTemplate.GetTerraformServiceType(tr.Tool))).ToList();
            if (resourceToDelete.Any(r => r.CreatedAt is null || r.Status != TerraformStatus.Completed))
                throw new Exception("One or more resources to delete are not yet created");

            Log("Checking resources to update");
            var resourceToUpdate = _transactions.Where(tr => tr.Type == ToolboxTransactionType.Update)
                .Select(tr => workspace.Resources.First(r =>
                    r.ResourceType == TerraformTemplate.GetTerraformServiceType(tr.Tool))).ToList();
            if (resourceToUpdate.Any(r => r.CreatedAt is null || r.Status != TerraformStatus.Completed))
                throw new Exception("One or more resources to update are not yet created");

            Log("Checking built workspace definition");
            if (_builtWorkspaceDefinition == null)
                throw new Exception("Built workspace definition is null");

            _transactions.ForEach(tr =>
            {
                switch (tr.Type)
                {
                    case ToolboxTransactionType.Add when !_builtWorkspaceDefinition.Templates.Any(template =>
                        template.Name == tr.Tool && template.Status == TerraformStatus.CreateRequested):
                        throw new Exception("Built workspace definition does not contain the added tool");
                    case ToolboxTransactionType.Update when !_builtWorkspaceDefinition.Templates.Any(template =>
                        template.Name == tr.Tool && template.Status == TerraformStatus.Completed):
                        throw new Exception("Built workspace definition does not contain the updated tool");
                    case ToolboxTransactionType.Remove when !_builtWorkspaceDefinition.Templates.Any(template =>
                        template.Name == tr.Tool && template.Status == TerraformStatus.DeleteRequested):
                        throw new Exception("Built workspace definition does not contain the removed tool");
                }
            });
        }

        /// <summary>
        /// Creates local records for the built workspace definition.
        /// </summary>
        private async Task LocalRecords()
        {
            foreach (var template in _builtWorkspaceDefinition.Templates)
            {
                Log($"Scaffolding local changes for {template.Name}");
                // Create project resource records for each template
                await RequestManagementService.ScaffoldLocalChanges(_workspace, _viewedPortalUser, template, _context);

                // Apply tool specific changes
                switch (template.Name)
                {
                    case TerraformTemplate.AzurePostgres:
                        Log("Applying postgres configuration to database");
                        _context.Projects.Attach(_workspace);

                        await _context.Entry(_workspace)
                            .Collection(p => p.Resources)
                            .LoadAsync();

                        var postgresResource = _workspace.Resources.First(r =>
                            r.ResourceType == TerraformTemplate.GetTerraformServiceType(template.Name) &&
                            r.ProjectId == _workspace.Project_ID);

                        var inputJson = new JsonObject
                        {
                            ["postgres_sku"] = _builtWorkspaceDefinition.AppData.PostgresConfiguration.PSQL_SKU
                        };
                        postgresResource.InputJsonContent = inputJson.ToString();
                        _context.Update(postgresResource);
                        break;
                }
            }
        }

        /// <summary>
        /// Sends the workspace definition to the Terraform queue for cloud provisioning.
        /// </summary>
        private async Task CloudRequest()
        {
            Log("Sending workspace definition to Terraform queue");
            if (!_mockRequest)
            {
                await ResourceMessagingService.SendToTerraformQueue(_builtWorkspaceDefinition);
            }
            else
            {
                Log("Mock request enabled. Skipping cloud request");
            }
        }

        /// <summary>
        /// Adds a tool to the workspace definition and its dependencies if they are not already present.
        /// </summary>
        /// <param name="tool">The tool identifier.</param>
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

        /// <summary>
        /// Removes a tool from the workspace definition.
        /// </summary>
        /// <param name="tool">The tool identifier.</param>
        private void RemoveTool(string tool)
        {
            Log($"Removing tool: {tool}");
            _transactions.RemoveTool(tool);
        }

        /// <summary>
        /// Updates a tool in the workspace definition.
        /// </summary>
        /// <param name="tool">The tool identifier.</param>
        private void UpdateTool(string tool)
        {
            Log($"Updating tool: {tool}");
            _transactions.UpdateTool(tool, OriginalData(tool), UpdatedData(tool));
        }

        /// <summary>
        /// Reverts a tool transaction in the workspace definition.
        /// </summary>
        /// <param name="transaction">The transaction to revert.</param>
        private void RevertTool(ToolboxTransaction transaction)
        {
            Log($"Reverting {transaction.Type.ToString().ToUpper()} of tool: {transaction.Tool}");

            var dependentTools =
                _transactions
                    .Where(tr => tr.Type == ToolboxTransactionType.Add &&
                                 TerraformTemplate.GetDependenciesToCreate(tr.Tool)
                                     .Any(dependency =>
                                         dependency.Name == transaction.Tool)).ToList();
            dependentTools.ForEach(tool =>
            {
                Log(
                    $"Reverting {tool.Type.ToString().ToUpper()} tool: {tool.Tool} as it depends on {transaction.Tool}");
                _transactions.Revert(tool);
            });

            _transactions.Revert(transaction);
        }

        /// <summary>
        /// Gets the original data for a tool.
        /// </summary>
        /// <param name="tool">The tool identifier.</param>
        /// <returns>The original data for the tool.</returns>
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

        /// <summary>
        /// Gets the updated data for a tool.
        /// </summary>
        /// <param name="tool">The tool identifier.</param>
        /// <returns>The updated data for the tool.</returns>
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

        /// <summary>
        /// Populates the tool catalog with tools that are not already in the workspace definition.
        /// </summary>
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

        /// <summary>
        /// Shows the information sheet for a tool.
        /// </summary>
        /// <param name="tool">The tool identifier.</param>
        private async Task ShowInfoSheet(string tool)
        {
            var infoParams = new DialogParameters
            {
                { "Title", ToolLabel(tool) },
                { "Description", ToolDescription(tool) },
                { "Icon", ToolIcon(tool) },
                { "Category", ToolCategory(tool) },
                { "Dependencies", ToolDependencies(tool) },
                { "Instances", await ToolInstances(tool) },
                { "Availability", AvailabilityLabel(_toolAvailabilityStatusMap[tool]) },
                { "CostInformation", ToolCostInformation(tool) },
                { "AdditionalLinks", ToolAdditionalLinks(tool) }
            };

            var infoOptions = new DialogOptions
            {
                FullWidth = true,
                CloseOnEscapeKey = true,
                CloseButton = true,
                MaxWidth = MaxWidth.Large
            };

            await DialogService.ShowAsync<InfoSheet>(ToolLabel(tool), infoParams, infoOptions);
        }

        #endregion

        #region Admin utils

        /// <summary>
        /// Logs a message to the console and adds it to the admin event logs.
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="type">The type of message, either "info", "warn", or "error"</param>
        private void Log(string message, string type = "info")
        {
            // ReSharper disable TemplateIsNotCompileTimeConstantProblem
            var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{type.ToUpper()}] {message}";
            switch (type)
            {
                case "info":
                    _adminEventLogs.Add(logMessage);
                    Logger.LogInformation(message);
                    break;
                case "warn":
                    _adminEventLogs.Add(logMessage);
                    Logger.LogWarning(message);
                    break;
                case "error":
                    _adminEventLogs.Add(logMessage);
                    Logger.LogError(message);
                    break;
            }
        }

        /// <summary>
        /// Converts a workspace definition into a markdown string
        /// </summary>
        /// <param name="workspaceDefinition">The workspace definition</param>
        /// <returns>The markdown string</returns>
        private string WorkspaceDefinitionMarkdown(WorkspaceDefinition workspaceDefinition)
        {
            var workspaceDefinitionJsonString = JsonSerializer.Serialize(workspaceDefinition,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                });

            return $"```json\n{workspaceDefinitionJsonString}\n```";
        }

        // Quick method to allow us to use DHMarkdown
        private string LinkRewriter(string link)
        {
            return link;
        }

        #endregion
    }
}