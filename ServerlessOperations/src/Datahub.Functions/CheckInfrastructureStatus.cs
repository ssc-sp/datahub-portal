using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Queues;
using Datahub.Application.Configuration;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Health;
using Datahub.Core.Utils;
using Datahub.Functions.Extensions;
using Datahub.Infrastructure.Extensions;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Storage;
using Datahub.Shared.Configuration;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Datahub.Functions;

public class CheckInfrastructureStatus(
    ILoggerFactory loggerFactory,
    DatahubProjectDBContext dbProjectContext,
    AzureConfig azureConfig,
    IHttpClientFactory httpClientFactory,
    DatahubPortalConfiguration portalConfiguration,
    IConfiguration configuration,
    ISendEndpointProvider sendEndpointProvider,
    ProjectStorageConfigurationService projectStorageConfigurationService)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<CheckInfrastructureStatus>();
    private readonly DatahubPortalConfiguration _portalConfiguration = portalConfiguration;
    private const string workspaceKeyCheck = "project-cmk";
    private const string coreKeyCheck = "datahubportal-client-id";

    /// <summary>
    /// Azure Function that runs on a timer to check the infrastructure health of all infrastructure.
    /// </summary>
    /// <param name="timerInfo"></param>
    /// <returns>An OkObjectResult containing the results for all infrastructure tests.</returns>
    [Function("CheckInfrastructureScheduled")]
    public async Task<IActionResult> RunCheckTimer([TimerTrigger("%ProjectUsageCRON%")] TimerInfo timerInfo)
    {
        _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        ObjectResult[] results = await RunAllChecks();
        return new OkObjectResult(results);
    }

    /// <summary>
    /// Azure Function that can be called to check a specific infrastructure resource with a POST request.
    /// </summary>
    /// <param name="req"></param>
    /// <returns>An OkObjectResult containing the result for a specific infrastructure test.</returns>
    [Function("CheckInfrastructureStatusHttp")]
    public async Task<IActionResult> RunHealthCheckHttp(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var request = System.Text.Json.JsonSerializer.Deserialize<InfrastructureHealthCheckRequest>(requestBody);

        if (request.Group == "all")
        {
            return new OkObjectResult(await RunAllChecks());
        }

        IActionResult result = await RunHealthCheck(request);
        return result;
    }

    /// <summary>
    /// Azure Function that consumes messages from the infrastructure health check queue and performs health checks on the specified infrastructure resources.
    /// </summary>
    /// <param name="message">The ServiceBusReceivedMessage containing the infrastructure health check request.</param>
    /// <returns>
    /// An IActionResult containing the results of the health check. If the request group is "all", it returns an OkObjectResult containing the results of all infrastructure checks.
    /// Otherwise, it returns the result of the specific health check associated with the request.
    /// </returns>
    /// <remarks>
    /// The method retrieves the InfrastructureHealthCheckRequest object from the message body and checks the request group. If the group is "all", it calls the RunAllChecks method
    /// to perform health checks on all infrastructure resources. If the group is not "all", it calls the RunHealthCheck method to perform the specific health check associated with
    /// the request. The method logs the processed message using the ILogger instance.
    /// </remarks>
    [Function("CheckInfrastructureStatusQueue")]
    public async Task<IActionResult> RunHealthCheckQueue(
        [ServiceBusTrigger(QueueConstants.InfrastructureHealthCheckQueueName,
            Connection = "DatahubServiceBus:ConnectionString")]
        ServiceBusReceivedMessage message)
    {
        _logger.LogInformation($"C# Queue trigger function processed: {message.Body}");
        
        var request = await message.DeserializeAndUnwrapMessageAsync<InfrastructureHealthCheckRequest>();

        if (request?.Group == "all")
        {
            return new OkObjectResult(await RunAllChecks());
        }

        return await RunHealthCheck(request);
    }

    /// <summary>
    /// Checks the infrastructure health of a specific resource. Creates a bug report if the resource is unhealthy. Stores all results in a blob.
    /// </summary>
    /// <param name="request">the request containing the resource type, group, and name</param>
    /// <returns>An OkObjectResult containing the result for a specific infrastructure test if the request is formatted properly. A BadRequestObjectResult is returned for poorly formatted requests.</returns>
    private async Task<IActionResult> RunHealthCheck(InfrastructureHealthCheckRequest request)
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
                result = await CheckAzureDatabricks(request);
                break;
            case InfrastructureHealthResourceType.AzureStorageQueue
                : // Name is the Azure Storage Queue name. Group == 1 for poison
                result = await CheckAzureStorageQueue(request);
                break;
            case InfrastructureHealthResourceType.AzureWebApp:
                result = await CheckWebApp(request);
                break;
            case InfrastructureHealthResourceType.AzureFunction: // Name is the Azure Function location
                result = await CheckAzureFunctions(request);
                break;
            default:
                return new BadRequestObjectResult("Please pass a valid request body");
        }

        //If the result is unhealthy, create a bug in DevOps
        if (result.Check.Status == InfrastructureHealthStatus.Unhealthy)
        {
            var bugReport = new BugReportMessage(
                UserName: "Datahub Portal",
                UserEmail: "",
                UserOrganization: "",
                PortalLanguage: "",
                PreferredLanguage: "",
                Timezone: "",
                Workspaces: "",
                Topics: "",
                URL: "",
                UserAgent: "",
                Resolution: "",
                LocalStorage: "",
                BugReportType: BugReportTypes.InfrastructureError,
                Description: $"The infrastructure health check for {request.Name} failed. Please investigate."
            );

            await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.BugReportQueueName, bugReport);
        }

        await StoreResult(result);          // operational data
        await StoreHealthCheckRun(result);  // historical data

        return new OkObjectResult(result);
    }

    /// <summary>
    /// Function that runs all infrastructure health checks. Called by the timer trigger.
    /// </summary>
    /// <returns>A list of ObjectResult objects with the results of all health checks.</returns>
    private async Task<ObjectResult[]> RunAllChecks()
    {
        ObjectResult[] objectResults = Array.Empty<ObjectResult>();

        // Core checks
        objectResults.Append(await RunHealthCheck(
            new InfrastructureHealthCheckRequest(InfrastructureHealthResourceType.AzureSqlDatabase, "core", "core")));
        objectResults.Append(await RunHealthCheck(
            new InfrastructureHealthCheckRequest(InfrastructureHealthResourceType.AzureKeyVault, "core", "core")));
        objectResults.Append(await RunHealthCheck(
            new InfrastructureHealthCheckRequest(InfrastructureHealthResourceType.AzureKeyVault, "workspaces",
                "workspaces")));
        objectResults.Append(await RunHealthCheck(
            new InfrastructureHealthCheckRequest(InfrastructureHealthResourceType.AzureFunction, "core",
                "localhost:7071")));

        // Workspace checks (Storage, Databricks, eventually Web App)
        var projects = dbProjectContext.Projects.AsNoTracking().Include(p => p.Resources).ToList();
        foreach (var project in projects)
        {
            objectResults.Append(await RunHealthCheck(new InfrastructureHealthCheckRequest(
                InfrastructureHealthResourceType.AzureSqlDatabase, "core", project.Project_Acronym_CD.ToString())));
            objectResults.Append(await RunHealthCheck(new InfrastructureHealthCheckRequest(
                InfrastructureHealthResourceType.AzureStorageAccount, "workspaces",
                project.Project_Acronym_CD.ToString())));
            objectResults.Append(await RunHealthCheck(new InfrastructureHealthCheckRequest(
                InfrastructureHealthResourceType.AzureDatabricks, "workspaces", project.Project_Acronym_CD)));
            objectResults.Append(await RunHealthCheck(new InfrastructureHealthCheckRequest(
                InfrastructureHealthResourceType.AzureWebApp, "workspaces", project.Project_Acronym_CD)));
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
            objectResults.Append(await RunHealthCheck(
                new InfrastructureHealthCheckRequest(InfrastructureHealthResourceType.AzureStorageQueue, "0",
                    queue.ToString())));
            objectResults.Append(await RunHealthCheck(
                new InfrastructureHealthCheckRequest(InfrastructureHealthResourceType.AzureStorageQueue, "1",
                    queue.ToString())));
        }

        return objectResults;
    }

    /// <summary>
    /// Function that stores the result of an infrastructure health check in the database.
    /// </summary>
    /// <param name="result">the result of the health check result to upload</param>
    /// <returns></returns>
    private async Task StoreResult(InfrastructureHealthCheckResponse result)
    {
        var check = result.Check;

        var existingCheck = await dbProjectContext.InfrastructureHealthChecks.FirstOrDefaultAsync(c =>
            c.Group == check.Group && c.Name == check.Name && c.ResourceType == check.ResourceType);

        if (existingCheck != null)
        {
            dbProjectContext.InfrastructureHealthChecks.Remove(existingCheck);
        }

        dbProjectContext.InfrastructureHealthChecks.Add(check);
        await dbProjectContext.SaveChangesAsync();
    }

    /// <summary>
    /// Function that stores the result of an infrastructure health check in the database.
    /// </summary>
    /// <param name="result">the result of the health check result to upload</param>
    /// <returns></returns>
    private async Task StoreHealthCheckRun(InfrastructureHealthCheckResponse result)
    {
        var check = result.Check;

        dbProjectContext.InfrastructureHealthCheckRuns.Add(check);
        await dbProjectContext.SaveChangesAsync();
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

        bool connectable = await dbProjectContext.Database.CanConnectAsync();
        if (!connectable)
        {
            check.Status = InfrastructureHealthStatus.Unhealthy;
            errors.Add("Cannot connect to the database.");
        }
        else
        {
            var test = dbProjectContext.Projects.First();
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
            Environment.SetEnvironmentVariable("AZURE_TENANT_ID", azureConfig.TenantId);
            Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", azureConfig.ClientId);
            Environment.SetEnvironmentVariable("AZURE_CLIENT_SECRET", azureConfig.ClientSecret);

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
            string accountName = projectStorageConfigurationService.GetProjectStorageAccountName(request.Name);
            string accountKey = await projectStorageConfigurationService.GetProjectStorageAccountKey(request.Name);

            var projectStorageManager = new AzureCloudStorageManager(accountName, accountKey);

            if (projectStorageManager.IsNull())
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

        var project = dbProjectContext.Projects.AsNoTracking().Include(p => p.Resources)
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
                        using var httpClient = httpClientFactory.CreateClient();
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

        // check and see if the function app exists
        if (!errors.Any())
        {
            check.Status = InfrastructureHealthStatus.Healthy;
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

        var storageConnectionString = configuration["DatahubStorageQueue:ConnectionString"];

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

        var project = dbProjectContext.Projects.AsNoTracking().Include(p => p.Resources)
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
                    using var httpClient = httpClientFactory.CreateClient();
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

        return new InfrastructureHealthCheckResponse(check, errors);
    }

    record InfrastructureHealthCheckRequest(InfrastructureHealthResourceType Type, string Group, string Name);

    record InfrastructureHealthCheckResponse(InfrastructureHealthCheck Check, List<string>? Errors);
}