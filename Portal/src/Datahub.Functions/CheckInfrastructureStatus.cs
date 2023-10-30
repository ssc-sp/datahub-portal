using System.Net;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Health;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using Datahub.Application.Configuration;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace Datahub.Functions;

public class CheckInfrastructureStatus
{
    private readonly ILogger _logger;
    private readonly DatahubProjectDBContext _projectDbContext;
    private readonly AzureConfig _azureConfig;
    private readonly IHttpClientFactory _httpClientFactory;

    private const string workspaceKeyCheck = "project-cmk";
    private const string coreKeyCheck = "datahubportal-client-id";

    public CheckInfrastructureStatus(ILoggerFactory loggerFactory, DatahubProjectDBContext projectDbContext, AzureConfig azureConfig, IHttpClientFactory httpClientFactory)
    {
        _logger = loggerFactory.CreateLogger<CheckInfrastructureStatus>();
        _projectDbContext = projectDbContext;
        _azureConfig = azureConfig;
        _httpClientFactory = httpClientFactory;
    }

    [Function("CheckInfrastructureStatus")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")]
        HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request");

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var request = JsonSerializer.Deserialize<InfrastructureHealthCheckRequest>(requestBody);

        switch (request?.Type)
        {
            case InfrastructureHealthResourceType.AzureSqlDatabase:
                return new OkObjectResult(await CheckAzureSqlDatabase(request));
            case InfrastructureHealthResourceType.AzureStorageAccount:
                return new OkObjectResult(await CheckAzureStorageAccount(request));
            case InfrastructureHealthResourceType.AzureKeyVault:
                return new OkObjectResult(await CheckAzureKeyVault(request));
            case InfrastructureHealthResourceType.AzureDatabricks:
                break;
            case InfrastructureHealthResourceType.AzureStorageQueue:
                break;
            case InfrastructureHealthResourceType.AzureWebApp:
                break;
            case InfrastructureHealthResourceType.AzureFunction:
                return new OkObjectResult(await CheckAzureFunctions(request));
            default:
                return new BadRequestObjectResult("Please pass a valid request body");
        }
        return new BadRequestObjectResult("Please pass a valid request body");
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
        
        try {
            KeyVaultSecret secret;
            if (request.Group == "core") // Key check for core
            {
                secret = await client.GetSecretAsync(coreKeyCheck);
            }
            else // Key check for workspaces (to verify)
            {
                secret = await client.GetSecretAsync(workspaceKeyCheck);
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
            Name = request.Group,
            ResourceType = request.Type,
            Status = InfrastructureHealthStatus.Unhealthy,
            HealthCheckTimeUtc = DateTime.UtcNow
        };
        
        // check and see if the storage account exists
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

    record InfrastructureHealthCheckRequest(InfrastructureHealthResourceType Type, string Group, string Name);

    record InfrastructureHealthCheckResponse(InfrastructureHealthCheck Check, List<string>? Errors);
}