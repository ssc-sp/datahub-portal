using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Queues.Models;
using Azure.Identity;
using Azure.Storage.Queues;
using Datahub.Application.Configuration;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Health;
using Datahub.Core.Services.Security;
using Datahub.Core.Utils;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Storage;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using MediatR;
using Datahub.Infrastructure.Queues.MessageHandlers;
using Lucene.Net.Search;
using Microsoft.EntityFrameworkCore.Internal;

namespace Datahub.Functions;

public class CheckInfrastructureStatus
{
    private readonly ILogger _logger;
    private readonly DatahubProjectDBContext _projectDbContext;
    private readonly AzureConfig _azureConfig;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly DatahubPortalConfiguration _portalConfiguration;
    private readonly IMediator _mediator;
    private readonly ProjectStorageConfigurationService _projectStorageConfigurationService;
    private const string workspaceKeyCheck = "project-cmk";
    private const string coreKeyCheck = "datahubportal-client-id";

    public CheckInfrastructureStatus(
        ILoggerFactory loggerFactory,
        DatahubProjectDBContext dbProjectContext,
        AzureConfig azureConfig,
        IHttpClientFactory httpClientFactory,
        DatahubPortalConfiguration portalConfiguration,
        IConfiguration configuration,
        IMediator mediator,
        ProjectStorageConfigurationService projectStorageConfigurationService)
    {
        _logger = loggerFactory.CreateLogger<CheckInfrastructureStatus>();
        _projectDbContext = dbProjectContext;
        _azureConfig = azureConfig;
        _httpClientFactory = httpClientFactory;
        _portalConfiguration = portalConfiguration;
        _configuration = configuration;
        _mediator = mediator;
        _projectStorageConfigurationService = projectStorageConfigurationService;
    }

    [Function("CheckInfrastructureScheduled")]
    public async Task RunCheckTimer([TimerTrigger("%ProjectUsageCRON%")] TimerInfo timerInfo)
    {
        // Core checks
        await RunHealthCheck(new InfrastructureHealthCheckRequest(InfrastructureHealthResourceType.AzureSqlDatabase, "core", "core"));
        await RunHealthCheck(new InfrastructureHealthCheckRequest(InfrastructureHealthResourceType.AzureKeyVault, "core", "core"));
        await RunHealthCheck(new InfrastructureHealthCheckRequest(InfrastructureHealthResourceType.AzureKeyVault, "workspaces", "workspaces"));
        await RunHealthCheck(new InfrastructureHealthCheckRequest(InfrastructureHealthResourceType.AzureFunction, "", ""));

        // Workspace checks (Storage, Databricks, eventually Web App)
        var projects = _projectDbContext.Projects.AsNoTracking().Include(p => p.Resources).ToList();
        foreach (var project in projects)
        {
            await RunHealthCheck(new InfrastructureHealthCheckRequest(InfrastructureHealthResourceType.AzureStorageAccount, project.Project_ID.ToString(), "temp"));
            await RunHealthCheck(new InfrastructureHealthCheckRequest(InfrastructureHealthResourceType.AzureDatabricks, project.Project_Acronym_CD, "temp"));
        }

        // Queues checks
        var queues = _configuration["DatahubStorageQueue:QueueNames"];
        foreach (var queue in queues)
        {
            await RunHealthCheck(new InfrastructureHealthCheckRequest(InfrastructureHealthResourceType.AzureStorageQueue, queue.ToString(), "0"));
            await RunHealthCheck(new InfrastructureHealthCheckRequest(InfrastructureHealthResourceType.AzureStorageQueue, queue.ToString(), "1"));
        }

    }

    [Function("CheckInfrastructureStatusHttp")]
    public async Task<IActionResult> RunHealthCheckHttp([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var request = System.Text.Json.JsonSerializer.Deserialize<InfrastructureHealthCheckRequest>(requestBody);
        IActionResult result = await RunHealthCheck(request);
        return result;
    }

    [Function("CheckInfrastructureStatusQueue")]
    public async Task<IActionResult> RunHealthCheckQueue([QueueTrigger("infrastructure-health-check", Connection = "DatahubStorageConnectionString")] QueueMessage queueItem)
    {
        var request = System.Text.Json.JsonSerializer.Deserialize<InfrastructureHealthCheckRequest>(queueItem.MessageText);
        return await RunHealthCheck(request);
    }

    private async Task<IActionResult> RunHealthCheck(InfrastructureHealthCheckRequest request)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request");

        InfrastructureHealthCheckResponse result = new InfrastructureHealthCheckResponse(new InfrastructureHealthCheck(), new List<string>());

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
            case InfrastructureHealthResourceType.AzureStorageQueue: // Name is the Azure Storage Queue name. Group == 1 for poison
                result = await CheckAzureStorageQueue(request);
                break;
            case InfrastructureHealthResourceType.AzureWebApp:
                break;
            case InfrastructureHealthResourceType.AzureFunction: // Name is the Azure Function location
                result = await CheckAzureFunctions(request);
                break;
            default:
                return new BadRequestObjectResult("Please pass a valid request body");
        }

        // If the result is unhealthy, create a bug in DevOps
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
                Description: $"[TESTING] The infrastructure health check for {request.Name} failed. Please investigate."
            );

            //await _mediator.Send(bugReport);
        }

        //await StoreResult(request, result);

        return new OkObjectResult(result);
    }

    private async Task StoreResult(InfrastructureHealthCheckRequest request, InfrastructureHealthCheckResponse result)
    {
        throw new NotImplementedException();
    }

    private async Task<InfrastructureHealthCheckResponse> CheckAzureSqlDatabase(
        InfrastructureHealthCheckRequest request)
    {
        var errors = new List<string>();
        var check = new InfrastructureHealthCheck()
        {
            Group = request.Group,
            Name = request.Group,
            ResourceType = request.Type,
            Status = InfrastructureHealthStatus.Unhealthy,
            HealthCheckTimeUtc = DateTime.UtcNow
        };

        bool connectable = await _projectDbContext.Database.CanConnectAsync();
        if (!connectable)
        {
            errors.Add("Cannot connect to the database.");
        }

        var test = _projectDbContext.Projects.First();
        if (test == null)
        {
            errors.Add("Cannot retrieve from the database.");
        }

        if (!errors.Any())
        {
            check.Status = InfrastructureHealthStatus.Healthy;
        }

        return new InfrastructureHealthCheckResponse(check, errors);
    }

    private Uri GetAzureKeyVaultUrl(InfrastructureHealthCheckRequest request)
    {
        if (request.Group != "core")
        {
            return new Uri($"https://fsdh-proj-{request.Name}-dev-kv.vault.azure.net/");
        }

        return new Uri($"https://{request.Name}.vault.azure.net/");
    }

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

        Environment.SetEnvironmentVariable("AZURE_TENANT_ID", _azureConfig.TenantId);
        Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", _azureConfig.ClientId);
        Environment.SetEnvironmentVariable("AZURE_CLIENT_SECRET", _azureConfig.ClientSecret);

        _logger.LogInformation($"URI: {GetAzureKeyVaultUrl(request)}");
        var client = new SecretClient(GetAzureKeyVaultUrl(request), new DefaultAzureCredential()); // Authenticates with Azure AD and creates a SecretClient object for the specified key vault

        try
        {
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
            catch
            {
                errors.Add("Unable to retrieve the secrets from the key vault.");
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

            if (projectStorageManager.IsNull())
            {
                errors.Add("Unable to find the data container.");
            }
        }
        catch (Exception ex)
        {
            errors.Add("Unable to retrieve project. " + ex.GetType().ToString());
        }

        if (!errors.Any())
        {
            check.Status = InfrastructureHealthStatus.Healthy;
        }

        return new InfrastructureHealthCheckResponse(check, errors);
    }

    private async Task<InfrastructureHealthCheckResponse> CheckAzureDatabricks(
               InfrastructureHealthCheckRequest request)
    {
        var errors = new List<string>();
        var check = new InfrastructureHealthCheck()
        {
            Group = request.Group,
            Name = request.Group,
            ResourceType = request.Type,
            Status = InfrastructureHealthStatus.Unhealthy,
            HealthCheckTimeUtc = DateTime.UtcNow
        };

        var project = _projectDbContext.Projects.AsNoTracking().Include(p => p.Resources).FirstOrDefault(p => p.Project_Acronym_CD == request.Name);

        if (project == null)
        {
            errors.Add("Failed to retrieve project.");
        }
        else
        {
            var databricksUrl = TerraformVariableExtraction.ExtractDatabricksUrl(project);

            if (databricksUrl == null)
            {
                errors.Add("Failed to retrieve Databricks URL.");
            }
            else
            {
                try
                {
                    using var httpClient = _httpClientFactory.CreateClient();
                    var response = await httpClient.GetAsync(databricksUrl);

                    if (!response.IsSuccessStatusCode)
                    {
                        errors.Add($"Databricks returned an unhealthy status code: {response.StatusCode}.");
                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Error while checking Databricks health: {ex.Message}");
                }
            }
        }


        if (!errors.Any())
        {
            check.Status = InfrastructureHealthStatus.Healthy;
        }

        return new InfrastructureHealthCheckResponse(check, errors);
    }

    private async Task<InfrastructureHealthCheckResponse> CheckAzureFunctions(InfrastructureHealthCheckRequest request)
    {
        string azureFunctionUrl = $"http://{request.Name}/api/FunctionsHealthCheck";
        var errors = new List<string>();
        var check = new InfrastructureHealthCheck()
        {
            Group = request.Group,
            Name = request.Group,
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

        return new InfrastructureHealthCheckResponse(check, errors);
    }

    private async Task<InfrastructureHealthCheckResponse> CheckAzureStorageQueue(InfrastructureHealthCheckRequest request)
    {
        var errors = new List<string>();
        var check = new InfrastructureHealthCheck()
        {
            Group = request.Group,
            Name = request.Group,
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

    record InfrastructureHealthCheckRequest(InfrastructureHealthResourceType Type, string Group, string Name);

    record InfrastructureHealthCheckResponse(InfrastructureHealthCheck Check, List<string>? Errors);
}