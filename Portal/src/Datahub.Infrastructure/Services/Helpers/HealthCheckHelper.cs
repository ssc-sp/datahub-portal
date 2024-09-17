using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.ResourceManager;
using Azure.ResourceManager.AppService;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Queues;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Health;
using Datahub.Core.Utils;
using Datahub.Infrastructure.Extensions;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services.Storage;
using Datahub.Shared.Clients;
using Datahub.Shared.Configuration;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services.Helpers
{
    public static class InfrastructureHealthCheckConstants
    {
        public const string CoreRequestGroup = "core";
        public const string WorkspacesRequestGroup = "workspaces";
        public const string AllRequestGroup = "all";
        public const string MainQueueRequestGroup = "0";
        public const string PoisonQueueRequestGroup = "1";

        public const string PoisonQueueSuffix = "-poison";

        public const string FSDHFunctionPrefix = "fsdh-func-dotnet";

        public const string WorkspaceKeyVaultCheckKeyName = "project-cmk";
        public const string CoreKeyVaultCheckSecretName = "datahubportal-client-id";

        public const string TerraformAzureDatabricksResourceType = "terraform:azure-databricks";

        public const string DatahubStorageQueueConnectionStringConfigKey = "DatahubStorageQueue:ConnectionString";
        public const string DatahubServiceBusConnectionStringConfigKey = "DatahubServiceBus:ConnectionString";

        public const string AzureTenantIdEnvKey = "AZURE_TENANT_ID";
        public const string AzureClientIdEnvKey = "AZURE_CLIENT_ID";
        public const string AzureClientSecretEnvKey = "AZURE_CLIENT_SECRET";

        public const string BugReportUsername = "Datahub Portal";
    }

    public class HealthCheckHelper(IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
        IProjectStorageConfigurationService projectStorageConfigurationService,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory,
        ISendEndpointProvider sendEndpointProvider,
        IResourceMessagingService resourceMessagingService,
        DatahubPortalConfiguration portalConfiguration)
    {
        private readonly ILogger<HealthCheckHelper> logger = loggerFactory.CreateLogger<HealthCheckHelper>();

        private string AzureTenantId => portalConfiguration.AzureAd.TenantId;
        private string DevopsClientId => portalConfiguration.AzureAd.InfraClientId;
        private string DevopsClientSecret => portalConfiguration.AzureAd.InfraClientSecret;

        private AzureDevOpsConfiguration BuildDevopsConfig() => new()
        {
            ClientId = DevopsClientId,
            ClientSecret = DevopsClientSecret,
            TenantId = AzureTenantId
        };

        public static List<InfrastructureHealthResourceType> CoreHealthChecks { get; } =
        [
            InfrastructureHealthResourceType.AzureSqlDatabase,
            InfrastructureHealthResourceType.AzureKeyVault,
            InfrastructureHealthResourceType.AzureFunction
        ];

        public static List<InfrastructureHealthResourceType> WorkspaceHealthChecks { get; } =
        [
            InfrastructureHealthResourceType.AzureSqlDatabase,
            InfrastructureHealthResourceType.AzureStorageAccount,
            InfrastructureHealthResourceType.AzureWebApp,
            InfrastructureHealthResourceType.WorkspaceSync,
            InfrastructureHealthResourceType.AzureKeyVault
        ];

        public static List<string> ServiceBusQueueHealthChecks { get; } =
        [
            QueueConstants.PongQueueName,
            QueueConstants.BugReportQueueName,
            QueueConstants.EmailNotificationQueueName,
            QueueConstants.InfrastructureHealthCheckQueueName,
            QueueConstants.InfrastructureHealthCheckResultsQueueName,
            QueueConstants.ProjectCapacityUpdateQueueName,
            QueueConstants.ProjectInactivityNotificationQueueName,
            QueueConstants.ProjectUsageNotificationQueueName,
            QueueConstants.ProjectUsageUpdateQueueName,
            QueueConstants.UserInactivityNotification,
            QueueConstants.TerraformOutputHandlerQueueName,
            QueueConstants.WorkspaceAppServiceConfigurationQueueName,
            QueueConstants.DatabricksSyncOutputQueueName,
            QueueConstants.KeyvaultSyncOutputQueueName,
            QueueConstants.StorageSyncOutputQueueName,
            QueueConstants.ResourceRunRequestQueueName,
            QueueConstants.UserRunRequestQueueName,
            QueueConstants.ProjectInactiveQueueName
        ];

        /// <summary>
        /// Function that checks the health of an Azure SQL Database.
        /// </summary>
        /// <param name="request"></param>>
        /// <returns>An IntermediateHealthCheckResult indicating the result of the check.</returns>
        public async Task<IntermediateHealthCheckResult> CheckAzureSqlDatabase(InfrastructureHealthCheckMessage request)
        {
            // TODO: workspace specific databases

            var errors = new List<string>();
            var status = InfrastructureHealthStatus.Healthy;

            await using var ctx = await dbContextFactory.CreateDbContextAsync();

            bool connectable = await ctx.Database.CanConnectAsync();
            if (!connectable)
            {
                status = InfrastructureHealthStatus.Unhealthy;
                errors.Add("Cannot connect to the database.");
            }
            else
            {
                var test = await ctx.Projects.FirstOrDefaultAsync();
                if (test == null)
                {
                    status = InfrastructureHealthStatus.Degraded;
                    errors.Add("Cannot retrieve from the database.");
                }
            }

            return new(status, errors);
        }


        // TODO: Verify correct key vault addresses
        /// <summary>
        /// Function that gets the Azure Key Vault URL based on the request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The URL for the group given.</returns>
        private Uri GetAzureKeyVaultUrl(InfrastructureHealthCheckMessage request) => request.Group == InfrastructureHealthCheckConstants.CoreRequestGroup ?
            new Uri($"https://fsdh-key-{CurrentEnvironment}.vault.azure.net/") :
            new Uri($"https://fsdh-proj-{request.Name}-{CurrentEnvironment}-kv.vault.azure.net/");

        /// <summary>
        /// Function that checks the health of an Azure Key Vault.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>An IntermediateHealthCheckResult indicating the result of the check.</returns>
        public async Task<IntermediateHealthCheckResult> CheckAzureKeyVault(InfrastructureHealthCheckMessage request)
        {
            var errors = new List<string>();
            var status = InfrastructureHealthStatus.Healthy;

            try
            {
                Environment.SetEnvironmentVariable(InfrastructureHealthCheckConstants.AzureTenantIdEnvKey, AzureTenantId);
                Environment.SetEnvironmentVariable(InfrastructureHealthCheckConstants.AzureClientIdEnvKey, DevopsClientId);
                Environment.SetEnvironmentVariable(InfrastructureHealthCheckConstants.AzureClientSecretEnvKey, DevopsClientSecret);

                var kvUrl = GetAzureKeyVaultUrl(request);
                var credential = new DefaultAzureCredential();

                var secretClient = new SecretClient(kvUrl, credential);
                var keyClient = new KeyClient(kvUrl, credential);

                if (request.Group == InfrastructureHealthCheckConstants.CoreRequestGroup) // Key check for core
                {
                    var _ = await secretClient.GetSecretAsync(InfrastructureHealthCheckConstants.CoreKeyVaultCheckSecretName);
                }
                else // Key check for workspaces (to verify)
                {
                    var _ = await keyClient.GetKeyAsync(InfrastructureHealthCheckConstants.WorkspaceKeyVaultCheckKeyName);
                }

                try
                {
                    // Iterate through the secrets in the key vault and check if they are expired
                    await foreach (var secretProperties in secretClient.GetPropertiesOfSecretsAsync())
                    {
                        if (secretProperties.ExpiresOn < DateTime.UtcNow)
                        {
                            errors.Add($"The secret {secretProperties.Name} has expired.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add("Unable to retrieve the secrets from the key vault." + ex.GetType().ToString());
                    errors.Add($"Details: {ex.Message}");
                }

                try
                {
                    // Iterate through the keys in the key vault and check if they are expired
                    await foreach (var keyProperties in keyClient.GetPropertiesOfKeysAsync())
                    {
                        if (keyProperties.ExpiresOn < DateTime.UtcNow)
                        {
                            errors.Add($"The key {keyProperties.Name} has expired.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add("Unable to retrieve the keys from the key vault." + ex.GetType().ToString());
                    errors.Add($"Details: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                errors.Add("Unable to connect and retrieve a key or secret. " + ex.GetType().ToString());
                errors.Add($"Details: {ex.Message}");
            }

            if (errors.Count > 0)
            {
                status = InfrastructureHealthStatus.Unhealthy;
            }

            return new(status, errors);
        }

        /// <summary>
        /// Function that checks the health of an Azure Storage Account.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>An IntermediateHealthCheckResult indicating the result of the check.</returns>
        public async Task<IntermediateHealthCheckResult> CheckAzureStorageAccount(InfrastructureHealthCheckMessage request)
        {
            var errors = new List<string>();
            var status = InfrastructureHealthStatus.Healthy;

            // Get the projects that match the request.Name
            try
            {
                string accountName = projectStorageConfigurationService.GetProjectStorageAccountName(request.Name);
                string accountKey = await projectStorageConfigurationService.GetProjectStorageAccountKey(request.Name);

                var projectStorageManager = new AzureCloudStorageManager(accountName, accountKey);

                if (projectStorageManager is null)
                {
                    status = InfrastructureHealthStatus.Degraded;
                    errors.Add("Unable to find the data container.");
                }
            }
            catch (Exception ex)
            {
                status = InfrastructureHealthStatus.Unhealthy;
                errors.Add("Unable to retrieve project. " + ex.GetType().ToString());
                errors.Add($"Details: {ex.Message}");
            }

            return new(status, errors);
        }

        public async Task<IntermediateHealthCheckResult?> TriggerWorkspaceSync(InfrastructureHealthCheckMessage request)
        {
            try
            {
                var workspaceDefinition = await resourceMessagingService.GetWorkspaceDefinition(request.Name);
                await resourceMessagingService.SendToUserQueue(workspaceDefinition);

                logger.LogInformation("Triggered workspace sync");
                return null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error while setting up workspace sync for {request.Name}");
                return new(InfrastructureHealthStatus.Unhealthy, [$"Error running workspace sync: {ex.Message}"]);
            }
        }

        /// <summary>
        /// Function that checks the health of an Azure Databricks workspace, ACL and cluster
        /// </summary>
        /// <param name="request"></param>
        /// <returns>An IntermediateHealthCheckResult indicating the result of the check.</returns>
        public async Task<IntermediateHealthCheckResult> CheckAzureDatabricksHealth(InfrastructureHealthCheckMessage request)
        {
            var errors = new List<string>();
            var status = InfrastructureHealthStatus.Healthy;

            await using var ctx = await dbContextFactory.CreateDbContextAsync();

            var project = await ctx.Projects
                .AsNoTracking()
                .Include(p => p.Resources)
                .FirstOrDefaultAsync(p => p.Project_Acronym_CD == request.Name);

            // If the project is null, the project does not exist or there was an error retrieving it
            if (project == null)
            {
                status = InfrastructureHealthStatus.Unhealthy;
                errors.Add("Failed to retrieve project.");
            }
            else
            {
                // We check if the project has a databricks resource. If not, we return a create status.
                var hasDatabricksResource = project.Resources.Any(r => r.ResourceType == InfrastructureHealthCheckConstants.TerraformAzureDatabricksResourceType);

                if (!hasDatabricksResource)
                {
                    status = InfrastructureHealthStatus.Create;
                }
                else
                {
                    // We attempt to retrieve the databricks URL. If we cannot, we return an unhealthy status.
                    var databricksUrl = TerraformVariableExtraction.ExtractDatabricksUrl(project);

                    if (databricksUrl == null)
                    {
                        status = InfrastructureHealthStatus.Unhealthy;
                        errors.Add("Failed to retrieve Databricks URL.");
                    }
                    else
                    {
                        try
                        {
                            var azureDevOpsClient = new AzureDevOpsClient(BuildDevopsConfig());
                            var accessToken = await azureDevOpsClient.AccessTokenAsync();

                            var databricksClient = new DatabricksClientUtils(databricksUrl, accessToken.Token);

                            // Verify Instance Availability
                            var instanceRunning = await databricksClient.IsDatabricksInstanceRunning();
                            if (!instanceRunning)
                            {
                                status = InfrastructureHealthStatus.Unhealthy;
                                errors.Add("Databricks instance is not available.");
                            }
                            else
                            {
                                // Verify ACL Status
                                var aclStatus = await databricksClient.VerifyACLStatus();
                                if (!aclStatus)
                                {
                                    status = InfrastructureHealthStatus.Unhealthy;
                                    errors.Add("Failed to verify ACL status.");
                                }
                                else
                                {
                                    // Check Cluster Status
                                    var clusterStatus = await databricksClient.GetClusterStatus("");
                                    if (string.IsNullOrEmpty(clusterStatus) || clusterStatus != "Running")
                                    {
                                        status = InfrastructureHealthStatus.Unhealthy;
                                        errors.Add("Cluster is not in the running state.");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            status = InfrastructureHealthStatus.Unhealthy;
                            errors.Add($"Error while checking Databricks health: {ex.Message}");
                        }
                    }
                }
            }

            return new(status, errors);
        }

        private string CurrentEnvironment => configuration["DataHub_ENVNAME"] ?? GetEnvironmentNameFailsafe();

        private string GetEnvironmentNameFailsafe()
        {
            logger.LogError("DataHub_ENVNAME not configured. Defaulting to dev");
            return "dev";
        }

        private async Task<string> GetAzureFunctionDefaultKey()
        {
            try
            {
                var credential = new ClientSecretCredential(AzureTenantId, DevopsClientId, DevopsClientSecret);

                var armClient = new ArmClient(credential);
                var subscription = await armClient.GetDefaultSubscriptionAsync();
                var resourceGroup = await subscription.GetResourceGroupAsync($"fsdh-{CurrentEnvironment}-rg");
                var functionApp = await resourceGroup.Value.GetWebSiteAsync($"{InfrastructureHealthCheckConstants.FSDHFunctionPrefix}-{CurrentEnvironment}");
                var hostKeys = await functionApp.Value.GetHostKeysAsync();
                var defaultKey = hostKeys.Value.FunctionKeys["default"];

                return defaultKey;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error obtaining Azure Function default key");
                throw;
            }
        }


        public async Task<IntermediateHealthCheckResult> CheckAzureFunctions(InfrastructureHealthCheckMessage request)
        {
            var errors = new List<string>();
            var status = InfrastructureHealthStatus.Healthy;

            // TODO this should come from request.Name
            var functionAppAddress = $"https://{InfrastructureHealthCheckConstants.FSDHFunctionPrefix}-{CurrentEnvironment}.azurewebsites.net";

            try
            {
                var functionKey = await GetAzureFunctionDefaultKey();
                var azureFunctionUrl = $"{functionAppAddress}/api/FunctionsHealthCheck?code={functionKey}";

                using var httpClient = httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync(azureFunctionUrl);

                if (!response.IsSuccessStatusCode)
                {
                    errors.Add($"Azure Function returned an unhealthy status code: {response.StatusCode}.");
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error while checking Azure Function health: {ex.Message}");
            }

            if (errors.Count > 0)
            {
                status = InfrastructureHealthStatus.Unhealthy;
            }

            return new(status, errors);
        }

        /// <summary>
        /// Function that returns the queue name for the given request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>[name]-poison if the request group is "1" (poison queue), or [name] otherwise</returns>
        public static string GetRequestQueueName(InfrastructureHealthCheckMessage request) => request.Group == InfrastructureHealthCheckConstants.PoisonQueueRequestGroup ?
            request.Name + InfrastructureHealthCheckConstants.PoisonQueueSuffix :
            request.Name;

        /// <summary>
        /// Function that checks the health of an Azure Storage Queue. Group == 1 for poison queue.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>An IntermediateHealthCheckResult indicating the result of the check.</returns>
        public async Task<IntermediateHealthCheckResult> CheckAzureStorageQueue(InfrastructureHealthCheckMessage request)
        {
            var errors = new List<string>();
            var status = InfrastructureHealthStatus.Healthy;

            var storageConnectionString = configuration[InfrastructureHealthCheckConstants.DatahubStorageQueueConnectionStringConfigKey];

            string queueName = GetRequestQueueName(request);

            try
            {
                QueueClient queueClient = new(storageConnectionString, queueName);

                if (queueClient is null)
                {
                    errors.Add("Unable to connect to the queue.");
                }
                else
                {
                    bool queueExists = await queueClient.ExistsAsync();
                    if (!queueExists)
                    {
                        errors.Add("The queue does not exist.");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error while checking Azure Storage Queue: {ex.Message}");
            }

            if (errors.Count > 0)
            {
                status = InfrastructureHealthStatus.Unhealthy;
            }

            return new(status, errors);
        }

        /// <summary>
        /// Function that checks the health of the Azure Service Bus
        /// </summary>
        /// <param name="request"></param>
        /// <returns>An IntermediateHealthCheckResult indicating the result of the check.</returns>
        public async Task<IntermediateHealthCheckResult> CheckAzureServiceBusQueue(InfrastructureHealthCheckMessage request)
        {
            var errors = new List<string>();
            var status = InfrastructureHealthStatus.Healthy;

            var serviceBusConnectionString = configuration[InfrastructureHealthCheckConstants.DatahubServiceBusConnectionStringConfigKey];

            var queueName = request.Name;
            var isDeadLetter = request.Group == InfrastructureHealthCheckConstants.PoisonQueueRequestGroup;

            try
            {
                ServiceBusClient serviceBusClient = new(serviceBusConnectionString);

                ServiceBusReceiverOptions options = new()
                {
                    SubQueue = isDeadLetter ? SubQueue.DeadLetter : SubQueue.None
                };

                ServiceBusReceiver receiver = serviceBusClient.CreateReceiver(queueName, options);

                if (receiver is null)
                {
                    errors.Add("Unable to connect to the queue.");
                }
                else
                {
                    // attempt to read message to check if queue exists; receiver is created with no errors for non-existing queue
                    ServiceBusReceivedMessage message = await receiver.PeekMessageAsync();
                    if (message != null && isDeadLetter)
                    {
                        if (string.IsNullOrEmpty(message.DeadLetterReason))
                        {
                            errors.Add("Dead letter reason is empty.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error while checking Azure Service Bus Queue {queueName}: {ex.Message.Replace(",", ".")}");
            }

            if (errors.Count > 0)
            {
                status = InfrastructureHealthStatus.Unhealthy;
            }

            return new(status, errors);
        }

        /// <summary>
        /// Function that checks the health of the Azure Web App, if enabled.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>An IntermediateHealthCheckResult indicating the result of the check.</returns>
        public async Task<IntermediateHealthCheckResult> CheckWebApp(InfrastructureHealthCheckMessage request)
        {
            var errors = new List<string>();
            var status = InfrastructureHealthStatus.Healthy;

            await using var ctx = await dbContextFactory.CreateDbContextAsync();

            var project = await ctx.Projects
                .AsNoTracking()
                .Include(p => p.Resources)
                .FirstOrDefaultAsync(p => p.Project_Acronym_CD == request.Name);

            // If the project is null, the project does not exist or there was an error retrieving it
            if (project == null)
            {
                errors.Add("Unable to retrieve project.");
                status = InfrastructureHealthStatus.Create;
            }
            else
            {
                // We check if the project has a web app resource. If not, we return a create status.
                if (project.WebAppEnabled == null || project.WebAppEnabled == false)
                {
                    status = InfrastructureHealthStatus.Create;
                }
                else
                {
                    string url = project.WebApp_URL;

                    // Validate if the URL is valid
                    if (!Uri.TryCreate(url, UriKind.Absolute, out var result))
                    {
                        status = InfrastructureHealthStatus.Unhealthy;
                        errors.Add("Invalid Web App URL.");
                        if (!string.IsNullOrEmpty(url) && !url.ToLower().StartsWith("http"))
                        {
                            url = "https://" + url;  // add https if not present
                        }
                    }

                    try
                    {
                        // We attempt to connect to the URL. If we cannot, we return an unhealthy status.
                        using var httpClient = httpClientFactory.CreateClient();
                        var response = await httpClient.GetAsync(url);

                        if (!response.IsSuccessStatusCode)
                        {
                            status = InfrastructureHealthStatus.Unhealthy;
                            errors.Add($"Web App returned an unhealthy status code: {response.StatusCode}. {response.ReasonPhrase}");
                        }
                        else
                        {
                            var content = await response.Content.ReadAsStringAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        status = InfrastructureHealthStatus.Unhealthy;
                        errors.Add($"Error while checking Web App health: {ex.Message}");
                    }
                }
            }

            return new(status, errors);
        }

        /// <summary>
        /// For queue-based checks, updates the name depending on whether the request is for the poison queue.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The correct health check name</returns>
        private static string GenerateHealthCheckName(InfrastructureHealthCheckMessage request) => request.Type switch
        {
            InfrastructureHealthResourceType.AzureStorageQueue => GetRequestQueueName(request),
            _ => request.Name
        };

        private bool IsUnhealthyStatus(InfrastructureHealthStatus status) => status == InfrastructureHealthStatus.Unhealthy
            || status == InfrastructureHealthStatus.Degraded;

        /// <summary>
        /// Checks the infrastructure health of a specific resource.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>An InfrastructureHealthCheckResponse, containing InfrastructureHealthCheck record and list of errors.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<InfrastructureHealthCheckResponse> RunHealthCheck(InfrastructureHealthCheckMessage request)
        {
            var intermediateResult = request?.Type switch
            {
                InfrastructureHealthResourceType.AzureSqlDatabase => await CheckAzureSqlDatabase(request),
                InfrastructureHealthResourceType.AzureStorageAccount => await CheckAzureStorageAccount(request),
                InfrastructureHealthResourceType.AzureKeyVault => await CheckAzureKeyVault(request),
                InfrastructureHealthResourceType.AzureDatabricks => await CheckAzureDatabricksHealth(request),
                InfrastructureHealthResourceType.AzureStorageQueue => await CheckAzureStorageQueue(request),
                InfrastructureHealthResourceType.AsureServiceBus => await CheckAzureServiceBusQueue(request),
                InfrastructureHealthResourceType.AzureWebApp => await CheckWebApp(request),
                InfrastructureHealthResourceType.AzureFunction => await CheckAzureFunctions(request),
                InfrastructureHealthResourceType.WorkspaceSync => await TriggerWorkspaceSync(request),
                InfrastructureHealthResourceType.DatabricksSync => await TriggerWorkspaceSync(request),
                _ => throw new InvalidOperationException()
            };

            if (intermediateResult != null)
            {
                // if status is unhealthy or degraded but no errors are listed, there is some unknown problem
                var details = intermediateResult.Errors.Count == 0 ?
                    (IsUnhealthyStatus(intermediateResult.Status) ? "Please investigate." : default) :
                    string.Join("; ", intermediateResult.Errors);

                var result = new InfrastructureHealthCheck()
                {
                    Group = request.Group,
                    Name = GenerateHealthCheckName(request),
                    ResourceType = request.Type,
                    Status = intermediateResult.Status,
                    HealthCheckTimeUtc = DateTime.UtcNow,
                    Details = details,
                };

                await StoreHealthCheck(result);
                await StoreHealthCheckRun(result);

                return new(result, intermediateResult.Errors);
            }
            else
            {
                return new(default, []);
            }
        }

        public async Task<IEnumerable<InfrastructureHealthCheckResponse>> ProcessHealthCheckRequest(InfrastructureHealthCheckMessage request)
        {
            if (request.Group == InfrastructureHealthCheckConstants.AllRequestGroup)
            {
                return await RunAllChecks();
            }
            else
            {
                var result = await RunHealthCheck(request);
                return [result];
            }
        }

        private bool IsLocalEnvironment => string.IsNullOrEmpty(CurrentEnvironment) || CurrentEnvironment == "local";

        private string DefaultFunctionUrl => $"https://{InfrastructureHealthCheckConstants.FSDHFunctionPrefix}-{CurrentEnvironment}.azurewebsites.net";

        /// <summary>
        /// Function that runs all infrastructure health checks.
        /// </summary>
        /// <returns>Results of all health checks</returns>
        public async Task<IEnumerable<InfrastructureHealthCheckResponse>> RunAllChecks()
        {
            var functionAppAddress = IsLocalEnvironment ?
                "http://localhost:7071" :
                DefaultFunctionUrl;

            var coreChecks = CoreHealthChecks.Select(c => c switch
            {
                InfrastructureHealthResourceType.AzureFunction => new InfrastructureHealthCheckMessage(c, InfrastructureHealthCheckConstants.CoreRequestGroup, functionAppAddress),
                _ => new InfrastructureHealthCheckMessage(c, InfrastructureHealthCheckConstants.CoreRequestGroup, InfrastructureHealthCheckConstants.CoreRequestGroup)
            });

            await using var ctx = await dbContextFactory.CreateDbContextAsync();

            // TODO check AzureSqlDatabase first and exclude these checks if it fails
            var projects = await ctx.Projects
                .AsNoTracking()
                .ToListAsync();
            var workspaceChecks = projects
                .SelectMany(p => WorkspaceHealthChecks
                    .Select(c => new InfrastructureHealthCheckMessage(c, InfrastructureHealthCheckConstants.WorkspacesRequestGroup, p.Project_Acronym_CD)));

            var queueChecks = ServiceBusQueueHealthChecks.SelectMany(q => new List<InfrastructureHealthCheckMessage>()
            {
                new(InfrastructureHealthResourceType.AsureServiceBus, InfrastructureHealthCheckConstants.MainQueueRequestGroup, q),
                new(InfrastructureHealthResourceType.AsureServiceBus, InfrastructureHealthCheckConstants.PoisonQueueRequestGroup, q)
            });

            var allChecks = coreChecks.Concat(workspaceChecks).Concat(queueChecks);

            var results = await Task.WhenAll(allChecks.Select(RunHealthCheck));

            return results;
        }

        private static InfrastructureHealthCheck CloneWithoutId(InfrastructureHealthCheck healthCheck) => new()
        {
            Details = healthCheck.Details,
            Group = healthCheck.Group,
            HealthCheckTimeUtc = healthCheck.HealthCheckTimeUtc,
            Name = healthCheck.Name,
            ResourceType = healthCheck.ResourceType,
            Status = healthCheck.Status,
            Url = healthCheck.Url,
        };

        public async Task StoreHealthCheck(InfrastructureHealthCheck check)
        {
            if (string.IsNullOrEmpty(check.Name) || string.IsNullOrEmpty(check.Group))
            {
                logger.LogWarning("Got a health check with empty identifier");
                return;
            }

            await using var ctx = await dbContextFactory.CreateDbContextAsync();

            var existingChecks = await ctx.InfrastructureHealthChecks
                .Where(c => c.Group == check.Group && c.Name == check.Name && c.ResourceType == check.ResourceType)
                .ToListAsync();

            if (existingChecks?.Count > 0)
            {
                ctx.InfrastructureHealthChecks.RemoveRange(existingChecks);
            }

            ctx.InfrastructureHealthChecks.Add(CloneWithoutId(check));

            try
            {
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error saving health check (type: {check.ResourceType}; group: {check.Group}; name: {check.Name})");
            }

        }

        public async Task StoreHealthCheckRun(InfrastructureHealthCheck check)
        {
            if (string.IsNullOrEmpty(check.Name) || string.IsNullOrEmpty(check.Group))
            {
                logger.LogWarning("Got a health check run with empty identifier");
                return;
            }

            await using var ctx = await dbContextFactory.CreateDbContextAsync();

            ctx.InfrastructureHealthCheckRuns.Add(CloneWithoutId(check));

            try
            {
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error saving health check run (type: {check.ResourceType}; group: {check.Group}; name: {check.Name})");
            }
        }

        public BugReportMessage? CreateBugReportMessage(InfrastructureHealthCheck result)
        {
            if (result is not null && IsUnhealthyStatus(result.Status))
            {
                return new BugReportMessage(
                    UserName: InfrastructureHealthCheckConstants.BugReportUsername,
                    UserEmail: string.Empty,
                    UserOrganization: string.Empty,
                    PortalLanguage: string.Empty,
                    PreferredLanguage: string.Empty,
                    Timezone: string.Empty,
                    Workspaces: result.Group == InfrastructureHealthCheckConstants.WorkspacesRequestGroup ? result.Name : string.Empty,
                    Topics: $"{result.ResourceType} {result.Name}",
                    URL: string.Empty,
                    UserAgent: string.Empty,
                    Resolution: string.Empty,
                    LocalStorage: string.Empty,
                    BugReportType: BugReportTypes.InfrastructureError,
                    Description: $"The infrastructure health check for {result.ResourceType} {result.Name} failed. {result.Details}"
                    );
            }
            else
            {
                return null;
            }
        }

        public async Task SendBugReportMessagesToQueue(IEnumerable<BugReportMessage?> messages) => await sendEndpointProvider.SendDatahubServiceBusMessages(QueueConstants.BugReportQueueName, messages);
    }

    public record IntermediateHealthCheckResult(InfrastructureHealthStatus Status, List<string> Errors);
    public record InfrastructureHealthCheckResponse(InfrastructureHealthCheck? Check, List<string>? Errors);
}
