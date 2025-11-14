using System.Diagnostics;
using System.Text.Json;
using Datahub.Application.Services;
using Datahub.Application.Services.Toolbox;
using Datahub.Infrastructure.Services.Toolbox;
using Datahub.Portal.Layout;
using Datahub.Portal.Pages.Workspace.Toolbox.ConfigurationForms;
using Datahub.Shared;
using Datahub.Shared.Entities;
using Datahub.Shared.Entities.WorkspaceToolConfiguration;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using Random = System.Random;

namespace Datahub.Portal.Pages.Workspace.Toolbox
{
    public partial class WorkspaceToolboxPage
    {
        #region Tool utilities

        private readonly IDictionary<string, VersionAwareWorkspaceToolInfo> _versionAwareTools = new Dictionary<string, VersionAwareWorkspaceToolInfo>()
        {
            { TerraformTemplate.NewProjectTemplate, new VersionAwareWorkspaceToolInfo
                {
                    ToolName = TerraformTemplate.NewProjectTemplate,
                    ToolLabel = "Azure Workspace Essentials",
                    ToolCategory = "Core",
                    ToolDescription =
                        "The basic tools required to host your workspace. This includes Azure Key vault, some monitoring and a virtual network, among other things.",
                    ToolIcon = SidebarIcons.Workspace,
                    MinAvailableVersion = VersionAwareWorkspaceToolInfo.ALWAYS,
                    CanBeDeleted = false,
                    ToolCostInformation = ("Workspace essentials are the backbone of your workspace in the cloud, and costs related to the various resources this includes sum up to less than {0:C2} per month.", [1.0]),
                    ToolCostSummaryFunction = (config) => ("< {0:C2}/month", [1.0m])
                }
            },
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
            { TerraformTemplate.AzureStorageBlob, new VersionAwareWorkspaceToolInfo
                {
                    ToolName = TerraformTemplate.AzureStorageBlob,
                    ToolLabel = "Azure Storage Blob",
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
            { TerraformTemplate.AzureArcGis, new VersionAwareWorkspaceToolInfo
                {
                    ToolName = TerraformTemplate.AzureArcGis,
                    ToolLabel = "Azure ArcGIS",
                    ToolCategory = "Data & Analytics",
                    ToolDescription =
                        "ArcGIS is a geographic information system (GIS) for working with maps and geographic information. It is used for creating and using maps, compiling geographic data, analyzing mapped information, sharing and discovering geographic information, using maps and geographic information in a range of applications, and managing geographic information in a database.",
                    ToolIcon = SidebarIcons.ArcGis,
                    MinAvailableVersion = VersionAwareWorkspaceToolInfo.UNDER_DEVELOPMENT,
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
            }
        };

        /// <summary>
        /// Retrieves information about a version-aware workspace tool by its name.
        /// </summary>
        /// <param name="toolName">The name of the tool to retrieve information for. This value cannot be <see langword="null"/> or empty.</param>
        /// <returns>A <see cref="VersionAwareWorkspaceToolInfo"/> object containing information about the specified tool.</returns>
        /// <exception cref="ArgumentException">Thrown if the specified <paramref name="toolName"/> is not defined.</exception>
        private VersionAwareWorkspaceToolInfo GetToolInfo(string toolName)
        {
            if (_versionAwareTools.TryGetValue(toolName, out var toolInfo))
            {
                return toolInfo;
            }
            throw new ArgumentException($"Tool '{toolName}' not found.");
        }

        /// <summary>
        /// Determines whether the specified tool is configurable for the current workspace version.
        /// </summary>
        /// <param name="toolInfo">The tool information, including version compatibility details.</param>
        /// <returns><see langword="true"/> if the tool is configurable for the current workspace version; otherwise, <see
        /// langword="false"/>.</returns>
        private bool IsConfigurable(VersionAwareWorkspaceToolInfo toolInfo) =>
            toolInfo.IsConfigurable(_workspaceVersion);

        /// <summary>
        /// Determines whether the specified transaction is configurable.
        /// </summary>
        /// <remarks>A transaction is considered configurable if the associated tool information meets the
        /// configurability criteria and the transaction type is not <see
        /// cref="ToolboxTransactionType.Remove"/>.</remarks>
        /// <param name="transaction">The transaction to evaluate.</param>
        /// <returns><see langword="true"/> if the transaction is configurable; otherwise, <see langword="false"/>.</returns>
        private bool IsConfigurable(ToolboxTransaction transaction) =>
            IsConfigurable(GetToolInfo(transaction.Tool)) &&
            transaction.Type != ToolboxTransactionType.Remove;

        /// <summary>
        /// Determines whether the specified tool is configurable in a more recent workspace version.
        /// </summary>
        /// <param name="toolInfo">The tool information containing versioning details.</param>
        /// <returns><see langword="true"/> if the tool is configurable in a more recent workspace version; otherwise, <see
        /// langword="false"/>.</returns>
        private bool IsConfigurableInFutureVersion(VersionAwareWorkspaceToolInfo toolInfo) => 
            toolInfo.IsConfigurableInFutureVersion(_workspaceVersion);

        internal enum AvailabilityStatus
        {
            Available,
            UnderDevelopment,
            Disabled,
            UpgradeWorkspace
        }

        /// <summary>
        /// Determines whether the specified tool can be deleted.
        /// </summary>
        /// <param name="toolInfo">The tool information containing the deletion status.</param>
        /// <returns><see langword="true"/> if the tool can be deleted; otherwise, <see langword="false"/>.</returns>
        private static bool IsDeletable(VersionAwareWorkspaceToolInfo toolInfo) => toolInfo.CanBeDeleted;

        /// <summary>
        /// Gets the label associated with each availability status.
        /// </summary>
        /// <param name="status">The availability status.</param>
        /// <returns>The localized label for the status.</returns>
        private string AvailabilityLabel(AvailabilityStatus status) => status switch
        {
            AvailabilityStatus.Available => Localizer["Available"],
            AvailabilityStatus.UnderDevelopment => Localizer["Under Development"],
            AvailabilityStatus.Disabled => Localizer["Disabled"],
            AvailabilityStatus.UpgradeWorkspace => Localizer["Upgrade Workspace"],
            _ => throw new NotImplementedException()
        };

        /// <summary>
        /// Get the availability status for a given tool based on its configuration and the current workspace version.
        /// </summary>
        /// <param name="toolInfo">The tool to get availability status for</param>
        /// <returns>Availability status for the given tool</returns>
        private AvailabilityStatus GetAvailabilityStatus(VersionAwareWorkspaceToolInfo toolInfo)
        {
            if (toolInfo.IsDisabled)
            {
                return AvailabilityStatus.Disabled;
            }

            if (toolInfo.MinAvailableVersion == VersionAwareWorkspaceToolInfo.UNDER_DEVELOPMENT)
            {
                return AvailabilityStatus.UnderDevelopment;
            }

            if (toolInfo.MinAvailableVersion > _workspaceVersion)
            {
                return AvailabilityStatus.UpgradeWorkspace;
            }

            return AvailabilityStatus.Available;
        }

        /// <summary>
        /// Gets the display label for each tool.
        /// </summary>
        /// <param name="toolInfo">The tool to get the label for.</param>
        /// <returns>The localized label for the tool.</returns>
        private string ToolLabel(VersionAwareWorkspaceToolInfo toolInfo) => Localizer[toolInfo.ToolLabel];

        /// <summary>
        /// Gets the category for each tool.
        /// </summary>
        /// <param name="toolInfo">The tool to get the category for.</param>
        /// <returns>The localized category for the tool.</returns>
        private string ToolCategory(VersionAwareWorkspaceToolInfo toolInfo) => Localizer[toolInfo.ToolCategory];

        /// <summary>
        /// Gets the description for each tool.
        /// </summary>
        /// <param name="toolInfo">The tool to get the description for.</param>
        /// <returns>The localized description for the tool.</returns>
        private string ToolDescription(VersionAwareWorkspaceToolInfo toolInfo) => Localizer[toolInfo.ToolDescription];

        /// <summary>
        /// Gets the icon for each tool.
        /// </summary>
        /// <param name="toolInfo">The tool to get the icon for.</param>
        /// <returns>The icon identifier for the tool.</returns>
        private static string ToolIcon(VersionAwareWorkspaceToolInfo toolInfo) => toolInfo.ToolIcon;
        
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
        /// <param name="toolInfo">The tool to get dependencies for.</param>
        /// <returns>An array of tuples containing the icon and name of each dependency.</returns>
        private (string Icon, string Name)[] ToolDependencies(VersionAwareWorkspaceToolInfo toolInfo) =>
            toolInfo.ToolDependencies
                .Select(GetToolInfo)
                .Select(dependencyInfo => (ToolIcon(dependencyInfo), ToolLabel(dependencyInfo)))
                .ToArray();
        
        /// <summary>
        /// Long form cost information for each resource
        /// </summary>
        /// <param name="toolInfo">The tool to get cost information for</param>
        /// <returns>A localized string providing cost information</returns>
        private string ToolCostInformation(VersionAwareWorkspaceToolInfo toolInfo)
        {
            var (info, args) = toolInfo.ToolCostInformation;
            return Localizer[info, args];
        }
        
        /// <summary>
        /// Short form cost calculation for each transaction
        /// </summary>
        /// <param name="transaction">The transaction to calculate the costs for</param>
        /// <returns>A localized summary for costs for the tool</returns>
        private string ToolCostSummary(ToolboxTransaction transaction)
        {
            if (transaction.Type == ToolboxTransactionType.Remove) return string.Empty;

            var toolInfo = GetToolInfo(transaction.Tool);
            var (info, args) = toolInfo.ToolCostSummaryFunction(transaction.UpdatedData);
            return Localizer[info, args];
        }

        /// <summary>
        /// List of additional links for each tool
        /// </summary>
        /// <param name="toolInfo">The tool to get additional links for</param>
        /// <returns>An array of tuples of text/URL for additional info on each tool</returns>
        private (string Text, string URL)[] ToolAdditionalLinks(VersionAwareWorkspaceToolInfo toolInfo) => toolInfo.AdditionalLinks
            .Select(linkInfo => (Localizer[linkInfo.Text].ToString(), Localizer[linkInfo.URL].ToString()))
            .ToArray();
        

        /// <summary>
        /// Converts a difference dictionary into a human-readable string.
        /// </summary>
        /// <param name="diff">The difference dictionary.</param>
        /// <returns>A human-readable string representing the differences.</returns>
        private string DisplayDiff(Dictionary<string, (object Original, object Updated)> diff, string toolName)
        {
            var toolInfo = GetToolInfo(toolName);
            var configInfo = toolInfo.GetApplicableConfigInfo(_workspaceVersion) ?? 
                throw new InvalidOperationException($"No applicable configuration found for tool '{toolName}' in workspace version {_workspaceVersion}.");
            var diffStrings = diff.Select(kv =>
            {
                var key = kv.Key;
                var propertyLabel = Localizer[configInfo.GetPropertyLabel(key)].ToString().ToLower();
                var originalValue = DisplayValue(kv.Value.Original);
                var updatedValue = DisplayValue(kv.Value.Updated);
                return originalValue == null
                    ? Localizer["Selected {0}: {1}", propertyLabel, updatedValue]
                    : Localizer["Updated {0}: {1} → {2}", propertyLabel, originalValue, updatedValue];
            });

            return string.Join(" / ", diffStrings);
        }

        /// <summary>
        /// Converts the specified value to a localized string representation.
        /// </summary>
        /// <remarks>This method is useful for displaying boolean values in a user-friendly, localized
        /// format.</remarks>
        /// <param name="value">The value to be displayed. If the value is a <see langword="bool"/>, it is converted to a localized "Yes" or
        /// "No" string; otherwise, the value is returned as-is.</param>
        /// <returns>A localized string representation of the value if it is a <see langword="bool"/>; otherwise, the original
        /// value.</returns>
        private object DisplayValue(object value)
        {
            if (value is bool booleanValue) return booleanValue ? Localizer["Yes"] : Localizer["No"];
            return value;
        }

        private IEnumerable<ToolboxTransaction> AllUpdateTransactions => _transactions.Where(tr => tr.Type == ToolboxTransactionType.Update);
        private IEnumerable<ToolboxTransaction> AllAddTransactions => _transactions.Where(tr => tr.Type == ToolboxTransactionType.Add);
        private IEnumerable<ToolboxTransaction> AllRemoveTransactions => _transactions.Where(tr => tr.Type == ToolboxTransactionType.Remove);

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
                if (!_disableSubmissionDelays) await Task.Delay(1000);
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
            Log("Request completed successfully");
            await Task.Delay(4000);
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
            if (!_disableSubmissionDelays)
                await Task.Delay(TimeSpan.FromSeconds(new Random().Next(1, 2))); // Small delay to make it look better

            var workspace = await _context
                .Projects
                .AsNoTracking()
                .Include(p => p.Resources)
                .Include(p => p.Credits)
                .Include(p => p.UserRoles)
                .FirstAsync(p => p.Project_Acronym_CD == WorkspaceAcronym);

            Log("Checking workspace state");
            if (workspace.IsDeleted) throw new Exception("Workspace has been deleted");
            if (workspace.UserRoles.Count == 0) throw new Exception("Workspace has no users");
            if (workspace.IsOverBudget) throw new Exception("Workspace is over budget");

            Log("Checking workspace for existing resources");
            if (AllAddTransactions.Any(tr =>
                    workspace.Resources.Any(r => r.ResourceType == TerraformTemplate.GetTerraformServiceType(tr.Tool))))
            {
                Log("Workspace already has one or more of the requested resources", "warn");
            }

            Log("Checking resources to delete");
            var resourceToDelete = AllRemoveTransactions
                .Select(tr => workspace.Resources.First(r =>
                    r.ResourceType == TerraformTemplate.GetTerraformServiceType(tr.Tool))).ToList();
            if (resourceToDelete.Any(r => r.CreatedAt is null || r.Status != TerraformStatus.Completed))
                throw new Exception("One or more resources to delete are not yet created");

            Log("Checking resources to update");
            var resourceToUpdate = AllUpdateTransactions
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
            if (!_disableSubmissionDelays)
                await Task.Delay(TimeSpan.FromSeconds(new Random().Next(1, 2))); // Random delay to make it look better
            var workspace = await _context
                .Projects
                .Include(p => p.Resources)
                .FirstAsync(p => p.Project_Acronym_CD == WorkspaceAcronym);

            var requestTime = DateTime.UtcNow;
            foreach (var template in _builtWorkspaceDefinition.Templates)
            {
                Log($"Scaffolding local changes for {template.Name}");
                // Create project resource records for each template
                await RequestManagementService.ScaffoldLocalChanges(workspace, _viewedPortalUser, template, _context, requestTime);

                // Apply tool specific changes
                var resource = workspace.Resources.First(r => r.ResourceType == TerraformTemplate.GetTerraformServiceType(template.Name));
                var toolInfo = GetToolInfo(template.Name);
                var configInfo = toolInfo.GetApplicableConfigInfo(_workspaceVersion);
                if (configInfo != null)
                {
                    var configuration = configInfo.GetConfigurationFromWorkspaceDefinition(_builtWorkspaceDefinition);
                    if (configuration != null)
                    {
                        resource.InputJsonContent = configuration.GenerateResourceInputJson();
                    }
                }
            }
        }

        /// <summary>
        /// Sends the workspace definition to the Terraform queue for cloud provisioning.
        /// </summary>
        private async Task CloudRequest()
        {
            if (!_disableSubmissionDelays) await Task.Delay(TimeSpan.FromSeconds(new Random().Next(1, 2)));
            Log("Sending workspace definition to Terraform queue");
            if (!_mockRequest)
            {
                if (!_disableSubmissions) await ResourceMessagingService.SendToTerraformQueue(_builtWorkspaceDefinition);
                _sentToTerraform = true;
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

            var dependencyNames = GetToolInfo(tool).ToolDependencies;
            foreach (var dependency in dependencyNames)
            {
                if (_workspaceDefinition.Templates.All(template => template.Name != dependency) &&
                    _transactions.DoesNotContainTool(dependency))
                {
                    Log($"Adding dependency: {dependency}");
                    _transactions.AddTool(dependency, OriginalData(dependency));
                }
            }
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

#nullable enable
        private IWorkspaceToolConfiguration? GetToolData(string tool, bool clone = false)
        {
            var configInfo = GetToolInfo(tool).GetApplicableConfigInfo(_workspaceVersion);
            if (configInfo == null)
            {
                return null;
            }

            var config = configInfo.GetConfigurationFromWorkspaceDefinition(_workspaceDefinition);

            if (config is IWorkspaceToolWithSuffix configWithSuffix)
            {
                configWithSuffix.ResourceNameSuffix = GetResourceNameSuffix(tool);
            }

            return clone? config.Clone() : config;
        }

        /// <summary>
        /// Gets the original data for a tool.
        /// </summary>
        /// <param name="tool">The tool identifier.</param>
        /// <returns>The original data for the tool.</returns>
        /// 
        private IWorkspaceToolConfiguration? OriginalData(string tool) => GetToolData(tool);

        /// <summary>
        /// Gets the updated data for a tool.
        /// </summary>
        /// <param name="tool">The tool identifier.</param>
        /// <returns>The updated data for the tool.</returns>
        /// 
        private IWorkspaceToolConfiguration? UpdatedData(string tool) => GetToolData(tool, clone: true);
        
#nullable disable

        /// <summary>
        /// Returns the resource name suffix that will be appended to the resource name for a given template type on the cloud
        /// </summary>
        /// <param name="tool">The tool identifier.</param>
        /// <returns>The resource name suffix for the tool.</returns>
        private string GetResourceNameSuffix(string tool)
        {
            //get total resources of template type
            var resourceNumber = _workspace.Resources.Count(r => r.ResourceType.Equals(TerraformTemplate.GetTerraformServiceType(tool)));
            
            //get next iteration for suffix
            resourceNumber++;

            // format resourceNumber to three digits
            return resourceNumber.ToString("D3");

        }

        private HashSet<string> GetExistingWorkspaceTools() => _workspaceDefinition.Templates.Select(t => t.Name).ToHashSet();

        /// <summary>
        /// Populates the tool catalog with tools that are not already in the workspace definition.
        /// </summary>
        private void PopulateCatalog()
        {
            var existingTools = GetExistingWorkspaceTools();
            _toolCatalog.AddRange(_versionAwareTools.Keys.Where(t => !existingTools.Contains(t)));
        }

        /// <summary>
        /// Shows the information sheet for a tool.
        /// </summary>
        /// <param name="tool">The tool.</param>
        /// <param name="id">The HTML id to use in the dialog</param>
        private async Task ShowInfoSheet(string tool, string id)
        {
            var toolInfo = GetToolInfo(tool);
            var infoParams = new DialogParameters
            {
                { "Title", ToolLabel(toolInfo) },
                { "Description", ToolDescription(toolInfo) },
                { "Icon", ToolIcon(toolInfo) },
                { "Category", ToolCategory(toolInfo) },
                { "Dependencies", ToolDependencies(toolInfo) },
                { "Instances", await ToolInstances(tool) },
                { "Availability", AvailabilityLabel(GetAvailabilityStatus(toolInfo)) },
                { "CostInformation", ToolCostInformation(toolInfo) },
                { "AdditionalLinks", ToolAdditionalLinks(toolInfo) },
                { "Id", id }
            };

            var infoOptions = new DialogOptions
            {
                FullWidth = true,
                CloseOnEscapeKey = true,
                CloseButton = true,
                MaxWidth = MaxWidth.Large
            };

            await DialogService.ShowAsync<InfoSheet>(ToolLabel(toolInfo), infoParams, infoOptions);
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