using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Queues;
using Datahub.Application.Configuration;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Health;
using Datahub.Core.Utils;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services.Storage;
using Datahub.Shared.Clients;
using Datahub.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.WebApi;
using System.Text.Json;

namespace Datahub.Infrastructure.Services;

public class LocalMessageReaderService : BackgroundService
{
    private readonly ILogger<LocalMessageReaderService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private FileSystemWatcher _watcher; 

    private DatahubPortalConfiguration _portalConfiguration;
    private DatahubProjectDBContext _projectDBContext;
    private IConfiguration _configuration;
    private ProjectStorageConfigurationService _projectStorageConfigurationService;
    private AzureDevOpsConfiguration _config = new AzureDevOpsConfiguration();
    private IHttpClientFactory _httpClientFactory;

    private const string workspaceKeyCheck = "project-cmk";
    private const string coreKeyCheck = "datahubportal-client-id";

    public LocalMessageReaderService(
            ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = loggerFactory.CreateLogger<LocalMessageReaderService>();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    { 
        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "MessageFolder");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        _watcher = new FileSystemWatcher
        {
            Path = folderPath,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            Filter = "*.*"
        };

        _watcher.Created += OnCreated;
        _watcher.EnableRaisingEvents = true;

        return Task.CompletedTask;
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation($"New file created: {e.Name}");

        // Wait until the file is no longer locked by the other process
        while (IsFileLocked(e.FullPath))
        {
            Thread.Sleep(100);
        }

        // Create a new scope to retrieve scoped services
        using (var scope = _serviceProvider.CreateScope())
        { 
            _projectStorageConfigurationService = scope.ServiceProvider.GetRequiredService<ProjectStorageConfigurationService>();
            _projectDBContext = scope.ServiceProvider.GetRequiredService<DatahubProjectDBContext>(); 
            _portalConfiguration = scope.ServiceProvider.GetRequiredService<DatahubPortalConfiguration>(); ;
            _configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>(); ;
            _httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>(); ;
            _config = _configuration
                .GetSection("InfrastructureRepository:AzureDevOpsConfiguration")
                .Get<AzureDevOpsConfiguration>();
            string fileContents = File.ReadAllText(e.FullPath);

            // Deserialize the file contents into an InfrastructureHealthCheckMessage object
            InfrastructureHealthCheckMessage message = JsonSerializer.Deserialize<InfrastructureHealthCheckMessage>(fileContents);
            var request = new InfrastructureHealthCheckRequest(message.Type, message.Group, message.Name);
            if (message.Group == "all")
            {
                RunAllChecks(e.Name).SyncResult();
            }
            else
            {
                RunHealthCheck(request, e.Name).SyncResult();
            }

        }
        // Process the file here
    }

    private bool IsFileLocked(string filePath)
    {
        try
        {
            using (FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                return false;
            }
        }
        catch (IOException)
        {
            // If an IOException is thrown, it means the file is still locked
            return true;
        }
    }

    public override Task StopAsync(CancellationToken stoppingToken)
    {
        _watcher.Dispose();
        return base.StopAsync(stoppingToken);
    }

    //  copied and modified from \datahub-portal\ServerlessOperations\src\Datahub.Functions\CheckInfrastructureStatus.cs
    //  to facilitate local debugging

    /// <summary>
    /// Checks the infrastructure health of a specific resource. Creates a bug report if the resource is unhealthy. Stores all results in a blob.
    /// </summary>
    /// <param name="request">the request containing the resource type, group, and name</param>
    /// <returns>An OkObjectResult containing the result for a specific infrastructure test if the request is formatted properly. A BadRequestObjectResult is returned for poorly formatted requests.</returns>
    private async Task RunHealthCheck(InfrastructureHealthCheckRequest request, string requestName)
    {
        InfrastructureHealthCheckResponse result =
            new InfrastructureHealthCheckResponse(new InfrastructureHealthCheck(), new List<string>());

        switch (request?.Type)
        {
            case InfrastructureHealthResourceType.AzureSqlDatabase:
                result = await CheckAzureSqlDatabase(request);
                break;
            case InfrastructureHealthResourceType.AzureStorageAccount: // Name is the project acronym to check for
                result = await CheckAzureStorageAccount(request);
                break;
            case InfrastructureHealthResourceType.AzureKeyVault: // Group is which key vault to check
                result = await CheckAzureKeyVault(request);
                break;
            case InfrastructureHealthResourceType.AzureDatabricks: // Name is project acronym to check
                result = await CheckAzureDatabricksHealth(request);
                break;
            case InfrastructureHealthResourceType.AzureStorageQueue
                : // Name is the Azure Storage Queue name. Group == 1 for poison
                result = await CheckAzureStorageQueue(request);
                break;
            case InfrastructureHealthResourceType.AsureServiceBus:
                result = await CheckAzureServiceBusQueue(request);
                var poison = new InfrastructureHealthCheckRequest(InfrastructureHealthResourceType.AsureServiceBus, "1", request.Name);
                await CheckAzureServiceBusQueue(poison);
                break;
            case InfrastructureHealthResourceType.AzureWebApp:
                result = await CheckWebApp(request);
                break;
            case InfrastructureHealthResourceType.AzureFunction: // Name is the Azure Function location
                result = await CheckAzureFunctions(request);
                break;
            default:
                return;
        }

        //If the result is unhealthy, create a bug in DevOps
        if (result.Check.Status == InfrastructureHealthStatus.Unhealthy)
        {
            var errorDetails = result.Errors?.Count > 0
                ? string.Join(", ", result.Errors) : "Please investigate.";
            var bugReport = new BugReportMessage(
                UserName: "Datahub Portal",
                UserEmail: "",
                UserOrganization: "",
                PortalLanguage: "",
                PreferredLanguage: "",
                Timezone: "",
                Workspaces: "",
                Topics: $"{request.Name} {request.Type}",
                URL: "",
                UserAgent: "",
                Resolution: "",
                LocalStorage: "",
                BugReportType: BugReportTypes.InfrastructureError,
                Description: $"The infrastructure health check for {request.Name} {request.Type} failed. {errorDetails}"
            );
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "MessageFolder");
            var fileName = "_bug_report";
            var filePath = Path.Combine(folderPath, $"{requestName}{fileName}.txt");
            var message = JsonSerializer.Serialize(bugReport);
            await File.WriteAllTextAsync(filePath, message);
            //await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.BugReportQueueName, bugReport);
        }

        await StoreResult(result);          // operational data
        await StoreHealthCheckRun(result);  // historical data

        //return new OkObjectResult(result);
    }

    /// <summary>
    /// Function that runs all infrastructure health checks. Called by the timer trigger.
    /// </summary>
    /// <returns>A list of ObjectResult objects with the results of all health checks.</returns>
    private async Task RunAllChecks(string requestName)
    {
        // Core checks
        await RunHealthCheck(
            new InfrastructureHealthCheckRequest(InfrastructureHealthResourceType.AzureSqlDatabase, "core", "core"), requestName);
        await RunHealthCheck(
            new InfrastructureHealthCheckRequest(InfrastructureHealthResourceType.AzureKeyVault, "core", "core"), requestName);
        await RunHealthCheck(
            new InfrastructureHealthCheckRequest(InfrastructureHealthResourceType.AzureKeyVault, "workspaces",
                "workspaces"), requestName);
        await RunHealthCheck(
            new InfrastructureHealthCheckRequest(InfrastructureHealthResourceType.AzureFunction, "core",
                "localhost:7071"), requestName);

        // Workspace checks (Storage, Databricks, eventually Web App)
        var projects = _projectDBContext.Projects.AsNoTracking().Include(p => p.Resources).ToList();
        foreach (var project in projects)
        {
            await RunHealthCheck(new InfrastructureHealthCheckRequest(
                InfrastructureHealthResourceType.AzureSqlDatabase, "core", project.Project_Acronym_CD.ToString()), requestName);
            await RunHealthCheck(new InfrastructureHealthCheckRequest(
                InfrastructureHealthResourceType.AzureStorageAccount, "workspaces",
                project.Project_Acronym_CD.ToString()), requestName);
            await RunHealthCheck(new InfrastructureHealthCheckRequest(
                InfrastructureHealthResourceType.AzureDatabricks, "workspaces", project.Project_Acronym_CD), requestName);
            await RunHealthCheck(new InfrastructureHealthCheckRequest(
                InfrastructureHealthResourceType.AzureWebApp, "workspaces", project.Project_Acronym_CD), requestName);
        }

        // Queues checks
        string[] queues = new string[]
        {
            "delete-run-request", "email-notification", "pong-queue", "project-capacity-update",
            "project-inactivity-notification", "project-usage-notification",
            "project-usage-update", "resource-run-request", "storage-capacity", "terraform-output",
            "user-inactivity-notification", "user-run-request"
        };
        foreach (var queue in queues)
        {
            await RunHealthCheck(
                new InfrastructureHealthCheckRequest(InfrastructureHealthResourceType.AzureStorageQueue, "0",
                    queue.ToString()), requestName);
            await RunHealthCheck(
                new InfrastructureHealthCheckRequest(InfrastructureHealthResourceType.AzureStorageQueue, "1",
                    queue.ToString()), requestName);
        }

        return;
    }

    /// <summary>
    /// Function that stores the result of an infrastructure health check in the database.
    /// </summary>
    /// <param name="result">the result of the health check result to upload</param>
    /// <returns></returns>
    private async Task StoreResult(InfrastructureHealthCheckResponse result)
    {
        var check = result.Check;
        if (check.Group == null || check.Name == null) { return; }
        var existingChecks = _projectDBContext.InfrastructureHealthChecks.Where(c =>
            c.Group == check.Group && c.Name == check.Name && c.ResourceType == check.ResourceType);

        if (existingChecks != null)
        {
            foreach(var item in existingChecks)
            {
                _projectDBContext.InfrastructureHealthChecks.Remove(item);
            }
        }

        // Add the check without specifying the ID to allow the database to generate it
        _projectDBContext.InfrastructureHealthChecks.Add(new InfrastructureHealthCheck
        {
            Group = check.Group,
            Name = check.Name,
            ResourceType = check.ResourceType,
            Status = check.Status,
            HealthCheckTimeUtc = check.HealthCheckTimeUtc,
            Details = check.Details,
        });
        try
        {
            await _projectDBContext.SaveChangesAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex.Message, result);
        }
    }

    /// <summary>
    /// Function that stores the result of an infrastructure health check in the database.
    /// </summary>
    /// <param name="result">the result of the health check result to upload</param>
    /// <returns></returns>
    private async Task StoreHealthCheckRun(InfrastructureHealthCheckResponse result)
    {
        var check = result.Check;
        if (check.Group == null || check.Name == null) { return; }

        // Add the check run without specifying the ID to allow the database to generate it
        _projectDBContext.InfrastructureHealthCheckRuns.Add(new InfrastructureHealthCheck
        {
            Group = check.Group,
            Name = check.Name,
            ResourceType = check.ResourceType,
            Status = check.Status,
            HealthCheckTimeUtc = check.HealthCheckTimeUtc,
            Details = check.Details,
        });
        try
        {
            await _projectDBContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, result);
        }
    }

    /// <summary>
    /// Function that checks the health of an Azure SQL Database.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>An InfrastructureHealthCheckResponse indicating the result of the check.</returns>
    private async Task<InfrastructureHealthCheckResponse> CheckAzureSqlDatabase(
        InfrastructureHealthCheckRequest request)
    {
        var errors = new List<string>();
        var check = new InfrastructureHealthCheck()
        {
            Group = request.Group,
            Name = request.Name,
            ResourceType = request.Type,
            Status = InfrastructureHealthStatus.Unhealthy,
            HealthCheckTimeUtc = DateTime.UtcNow
        };

        bool connectable = await _projectDBContext.Database.CanConnectAsync();
        if (!connectable)
        {
            check.Status = InfrastructureHealthStatus.Unhealthy;
            errors.Add("Cannot connect to the database.");
        }
        else
        {
            var test = _projectDBContext.Projects.First();
            if (test == null)
            {
                check.Status = InfrastructureHealthStatus.Degraded;
                errors.Add("Cannot retrieve from the database.");
            }
        }

        if (!errors.Any())
        {
            check.Status = InfrastructureHealthStatus.Healthy;
        }
        else
        {
            check.Details = string.Join(", ", errors);
        }

        return new InfrastructureHealthCheckResponse(check, errors);
    }

    /// <summary>
    /// Function that gets the Azure Key Vault URL based on the request.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>The URL for the group given.</returns>
    private Uri GetAzureKeyVaultUrl(InfrastructureHealthCheckRequest request)
    {
        if (request.Group != "core")
        {
            return new Uri($"https://fsdh-proj-{request.Name}-dev-kv.vault.azure.net/");
        }

        return new Uri($"https://{request.Name}.vault.azure.net/");
    }

    /// <summary>
    /// Function that checks the health of an Azure Key Vault.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>An InfrastructureHealthCheckResponse indicating the result of the check.</returns>
    private async Task<InfrastructureHealthCheckResponse> CheckAzureKeyVault(InfrastructureHealthCheckRequest request)
    {
        var errors = new List<string>();
        var check = new InfrastructureHealthCheck()
        {
            Group = request.Group,
            Name = request.Name,
            ResourceType = request.Type,
            Status = InfrastructureHealthStatus.Unhealthy,
            HealthCheckTimeUtc = DateTime.UtcNow
        };

        try
        {
            Environment.SetEnvironmentVariable("AZURE_TENANT_ID", _config.TenantId);
            Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", _config.ClientId);
            Environment.SetEnvironmentVariable("AZURE_CLIENT_SECRET", _config.ClientSecret);

            var client =
                new SecretClient(GetAzureKeyVaultUrl(request),
                    new DefaultAzureCredential()); // Authenticates with Azure AD and creates a SecretClient object for the specified key vault

            KeyVaultSecret secret;
            if (request.Group == "core") // Key check for core
            {
                secret = await client.GetSecretAsync(coreKeyCheck);
            }
            else // Key check for workspaces (to verify)
            {
                secret = await client.GetSecretAsync(workspaceKeyCheck);
            }

            try
            {
                // Iterate through the keys in the key vault and check if they are expired
                await foreach (var secretProperties in client.GetPropertiesOfSecretsAsync())
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
            }
        }
        catch (Exception ex)
        {
            errors.Add("Unable to connect and retrieve a secret. " + ex.GetType().ToString());
        }

        if (!errors.Any())
        {
            check.Status = InfrastructureHealthStatus.Healthy;
        }
        else
        {
            check.Details = string.Join(", ", errors);
        }
        return new InfrastructureHealthCheckResponse(check, errors);
    }

    /// <summary>
    /// Function that checks the health of an Azure Storage Account.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>An InfrastructureHealthCheckResponse indicating the result of the check.</returns>
    private async Task<InfrastructureHealthCheckResponse> CheckAzureStorageAccount(
        InfrastructureHealthCheckRequest request)
    {
        var errors = new List<string>();
        var check = new InfrastructureHealthCheck()
        {
            Group = request.Group,
            Name = request.Name,
            ResourceType = request.Type,
            Status = InfrastructureHealthStatus.Unhealthy,
            HealthCheckTimeUtc = DateTime.UtcNow
        };

        // Get the projects that match the request.Name
        try
        {
            string accountName = _projectStorageConfigurationService.GetProjectStorageAccountName(request.Name);
            string accountKey = await _projectStorageConfigurationService.GetProjectStorageAccountKey(request.Name);

            var projectStorageManager = new AzureCloudStorageManager(accountName, accountKey);

            if (projectStorageManager == null)
            {
                check.Status = InfrastructureHealthStatus.Degraded;
                errors.Add("Unable to find the data container.");
            }
        }
        catch (Exception ex)
        {
            check.Status = InfrastructureHealthStatus.Unhealthy;
            errors.Add("Unable to retrieve project. " + ex.GetType().ToString());
        }

        if (!errors.Any())
        {
            check.Status = InfrastructureHealthStatus.Healthy;
        }
        else
        {
            check.Details = string.Join(", ", errors);
        }

        return new InfrastructureHealthCheckResponse(check, errors);
    }

    private async Task<InfrastructureHealthCheckResponse> CheckAzureDatabricksHealth(
    InfrastructureHealthCheckRequest request)
    {
        var errors = new List<string>();
        var check = new InfrastructureHealthCheck()
        {
            Group = request.Group,
            Name = request.Name,
            ResourceType = request.Type,
            Status = InfrastructureHealthStatus.Unhealthy,
            HealthCheckTimeUtc = DateTime.UtcNow
        };

        var project = _projectDBContext.Projects.AsNoTracking().Include(p => p.Resources)
            .FirstOrDefault(p => p.Project_Acronym_CD == request.Name);

        // If the project is null, the project does not exist or there was an error retrieving it
        if (project == null)
        {
            check.Status = InfrastructureHealthStatus.Unhealthy;
            errors.Add("Failed to retrieve project.");
        }
        else
        {
            // We check if the project has a databricks resource. If not, we return a create status.
            bool checkForDatabricks = false;
            var resources = project.Resources.ToArray();
            foreach (var resource in resources)
            {
                if (resource.ResourceType == "terraform:azure-databricks")
                {
                    checkForDatabricks = true;
                    break;
                }
            }

            if (!checkForDatabricks)
            {
                check.Status = InfrastructureHealthStatus.Create;
            }
            else
            {
                // We attempt to retrieve the databricks URL. If we cannot, we return an unhealthy status.
                var databricksUrl = TerraformVariableExtraction.ExtractDatabricksUrl(project);

                if (databricksUrl == null)
                {
                    check.Status = InfrastructureHealthStatus.Unhealthy;
                    errors.Add("Failed to retrieve Databricks URL.");
                }
                else
                {
                    try
                    {
                        var azureDevOpsClient = new AzureDevOpsClient(_config);
                        var accessToken = await azureDevOpsClient.AccessTokenAsync();

                        var databricksClient = new DatabricksClientUtils(databricksUrl, accessToken.Token);

                        // Verify Instance Availability
                        var instanceRunning = await databricksClient.IsDatabricksInstanceRunning();
                        if (!instanceRunning)
                        {
                            check.Status = InfrastructureHealthStatus.Unhealthy;
                            errors.Add("Databricks instance is not available.");
                        }
                        else
                        {
                            // Verify ACL Status
                            var aclStatus = await databricksClient.VerifyACLStatus();
                            if (!aclStatus)
                            {
                                check.Status = InfrastructureHealthStatus.Unhealthy;
                                errors.Add("Failed to verify ACL status.");
                            }
                            else
                            {
                                // Check Cluster Status
                                var clusterStatus = await databricksClient.GetClusterStatus("");
                                if (string.IsNullOrEmpty(clusterStatus) || clusterStatus != "Running")
                                {
                                    check.Status = InfrastructureHealthStatus.Unhealthy;
                                    errors.Add("Cluster is not in the running state.");
                                }
                                else
                                {
                                    check.Status = InfrastructureHealthStatus.Healthy;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        check.Status = InfrastructureHealthStatus.Unhealthy;
                        errors.Add($"Error while checking Databricks health: {ex.Message}");
                    }
                }
            }
        }


        if (errors.Any())
        {
            check.Details = string.Join(", ", errors);
        }
        return new InfrastructureHealthCheckResponse(check, errors);
    }
  
    /// <summary>
    /// Function that checks the health of an Azure Databricks workspace.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>An InfrastructureHealthCheckResponse indicating the result of the check.</returns>
    private async Task<InfrastructureHealthCheckResponse> CheckAzureDatabricks(
        InfrastructureHealthCheckRequest request)
    {
        var errors = new List<string>();
        var check = new InfrastructureHealthCheck()
        {
            Group = request.Group,
            Name = request.Name,
            ResourceType = request.Type,
            Status = InfrastructureHealthStatus.Unhealthy,
            HealthCheckTimeUtc = DateTime.UtcNow
        };

        var project = _projectDBContext.Projects.AsNoTracking().Include(p => p.Resources)
            .FirstOrDefault(p => p.Project_Acronym_CD == request.Name);

        // If the project is null, the project does not exist or there was an error retrieving it
        if (project == null)
        {
            check.Status = InfrastructureHealthStatus.Unhealthy;
            errors.Add("Failed to retrieve project.");
        }

        {
            // We check if the project has a databricks resource. If not, we return a create status.
            bool checkForDatabricks = false;
            var resources = project.Resources.ToArray();
            foreach (var resource in resources)
            {
                if (resource.ResourceType == "terraform:azure-databricks")
                {
                    checkForDatabricks = true;
                }
            }

            if (!checkForDatabricks)
            {
                check.Status = InfrastructureHealthStatus.Create;
            }
            else
            {
                // We attempt to retrieve the databricks URL. If we cannot, we return an unhealthy status.
                var databricksUrl = TerraformVariableExtraction.ExtractDatabricksUrl(project);

                if (databricksUrl == null)
                {
                    check.Status = InfrastructureHealthStatus.Unhealthy;
                    errors.Add("Failed to retrieve Databricks URL.");
                }
                else
                {
                    try
                    {
                        // We attempt to connect to the databricks URL. If we cannot, we return an unhealthy status.
                        using var httpClient = _httpClientFactory.CreateClient();
                        var response = await httpClient.GetAsync(databricksUrl);

                        if (!response.IsSuccessStatusCode)
                        {
                            check.Status = InfrastructureHealthStatus.Unhealthy;
                            errors.Add($"Databricks returned an unhealthy status code: {response.StatusCode}.");
                        }
                        else
                        {
                            var content = await response.Content.ReadAsStringAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        check.Status = InfrastructureHealthStatus.Unhealthy;
                        errors.Add($"Error while checking Databricks health: {ex.Message}");
                    }
                }
            }
        }

        if (!errors.Any())
        {
            check.Status = InfrastructureHealthStatus.Healthy;
        }
        else
        {
            check.Details = string.Join(", ", errors);
        }

        return new InfrastructureHealthCheckResponse(check, errors);
    }

    /// <summary>
    /// Function that checks the health of the Azure Function App.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>An InfrastructureHealthCheckResponse indicating the result of the check.</returns>
    private async Task<InfrastructureHealthCheckResponse> CheckAzureFunctions(InfrastructureHealthCheckRequest request)
    {
        string azureFunctionUrl = $"http://{request.Name}/api/FunctionsHealthCheck";
        var errors = new List<string>();
        var check = new InfrastructureHealthCheck()
        {
            Group = request.Group,
            Name = request.Name,
            ResourceType = request.Type,
            Status = InfrastructureHealthStatus.Unhealthy,
            HealthCheckTimeUtc = DateTime.UtcNow
        };

        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
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

        // check and see if the function app exists
        if (!errors.Any())
        {
            check.Status = InfrastructureHealthStatus.Healthy;
        }
        else
        {
            check.Details = string.Join(", ", errors);
        }

        return new InfrastructureHealthCheckResponse(check, errors);
    }

    /// <summary>
    /// Function that checks the health of an Azure Storage Queue. Group == 1 for poison queue.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>An InfrastructureHealthCheckResponse indicating the result of the check.</returns>
    private async Task<InfrastructureHealthCheckResponse> CheckAzureStorageQueue(
        InfrastructureHealthCheckRequest request)
    {
        var errors = new List<string>();
        var check = new InfrastructureHealthCheck()
        {
            Group = request.Group,
            Name = request.Name,
            ResourceType = request.Type,
            Status = InfrastructureHealthStatus.Unhealthy,
            HealthCheckTimeUtc = DateTime.UtcNow
        };

        var storageConnectionString = _configuration["DatahubStorageQueue:ConnectionString"];

        string queueName = request.Name;
        if (request.Group == "1")
        {
            queueName += "-poison";
        }

        check.Name = queueName;

        try
        {
            QueueClient queueClient = new QueueClient(storageConnectionString, queueName);


            if (queueClient is null)
            {
                errors.Add("Unable to connect to the queue.");
            }
            else
            {
                bool queueExists = queueClient.Exists();
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

        if (!errors.Any())
        {
            check.Status = InfrastructureHealthStatus.Healthy;
        }
        else
        {
            check.Details = string.Join(", ", errors);
        }

        return new InfrastructureHealthCheckResponse(check, errors);
    }

    /// <summary>
    /// Function that checks the health of the Azure Service Bus
    /// </summary>
    /// <param name="request"></param>
    /// <returns>An InfrastructureHealthCheckResponse indicating the result of the check.</returns>
    private async Task<InfrastructureHealthCheckResponse> CheckAzureServiceBusQueue(
       InfrastructureHealthCheckRequest request)
    {
        var errors = new List<string>();
        var check = new InfrastructureHealthCheck()
        {
            Group = request.Group,
            Name = request.Name,
            ResourceType = request.Type,
            Status = InfrastructureHealthStatus.Unhealthy,
            HealthCheckTimeUtc = DateTime.UtcNow
        };

        var serviceBusConnectionString = _configuration["DatahubServiceBus:ConnectionString"];

        string queueName = request.Name;
        if (request.Group == "1")
        {
            queueName += "-poison";
        }

        check.Name = queueName;

        try
        {
            ServiceBusClient serviceBusClient = new ServiceBusClient(serviceBusConnectionString);
            ServiceBusReceiver receiver = serviceBusClient.CreateReceiver(queueName);

            if (receiver is null)
            {
                errors.Add("Unable to connect to the queue.");
            }
            else
            {
                // attempt to read message to check if queue exists; receiver is created with no errors for non-existing queue
                ServiceBusReceivedMessage message = await receiver.PeekMessageAsync();
                if (message != null && request.Group == "1")
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
            errors.Add($"Error while checking Azure Service Bus Queue: {ex.Message.Replace(",",".")}");
        }

        if (!errors.Any())
        {
            check.Status = InfrastructureHealthStatus.Healthy;
        }
        else
        {
            check.Details = string.Join(", ", errors);
        }

        return new InfrastructureHealthCheckResponse(check, errors);
    }

    /// <summary>
    /// Function that checks the health of the Azure Web App, if enabled.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>An InfrastructureHealthCheckResponse indicating the result of the check.</returns>
    private async Task<InfrastructureHealthCheckResponse> CheckWebApp(InfrastructureHealthCheckRequest request)
    {
        var errors = new List<string>();
        var check = new InfrastructureHealthCheck()
        {
            Group = request.Group,
            Name = request.Name,
            ResourceType = request.Type,
            Status = InfrastructureHealthStatus.Unhealthy,
            HealthCheckTimeUtc = DateTime.UtcNow
        };

        var project = _projectDBContext.Projects.AsNoTracking().Include(p => p.Resources)
            .FirstOrDefault(p => p.Project_Acronym_CD == request.Name);

        // If the project is null, the project does not exist or there was an error retrieving it
        if (project == null)
        {
            errors.Add("Unable to retrieve project.");
            check.Status = InfrastructureHealthStatus.Create;
        }
        else
        {
            // We check if the project has a web app resource. If not, we return a create status.
            if (project.WebAppEnabled == null || project.WebAppEnabled == false)
            {
                check.Status = InfrastructureHealthStatus.Create;
            }
            else
            {
                string url = project.WebApp_URL;

                try
                {
                    // We attempt to connect to the URL. If we cannot, we return an unhealthy status.
                    using var httpClient = _httpClientFactory.CreateClient();
                    var response = await httpClient.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        check.Status = InfrastructureHealthStatus.Unhealthy;
                        errors.Add($"Web App returned an unhealthy status code: {response.StatusCode}.");
                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                    }
                }
                catch (Exception ex)
                {
                    check.Status = InfrastructureHealthStatus.Unhealthy;
                    errors.Add($"Error while checking Web App health: {ex.Message}");
                }
            }
        }
        if (errors.Any())
        {
            check.Details = string.Join(", ", errors);
        }

        return new InfrastructureHealthCheckResponse(check, errors);
    }

    public record InfrastructureHealthCheckRequest(InfrastructureHealthResourceType Type, string Group, string Name);

    public record InfrastructureHealthCheckResponse(InfrastructureHealthCheck Check, List<string>? Errors);

}
