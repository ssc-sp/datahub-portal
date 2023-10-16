using System.Net;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Health;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Datahub.Functions;

public class CheckInfrastructureStatus
{
    private readonly ILogger _logger;
    private readonly DatahubProjectDBContext _projectDbContext;

    public CheckInfrastructureStatus(ILoggerFactory loggerFactory, DatahubProjectDBContext projectDbContext)
    {
        _logger = loggerFactory.CreateLogger<CheckInfrastructureStatus>();
        _projectDbContext = projectDbContext;
    }

    [Function("CheckInfrastructureStatus")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")]
        HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request");

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var request = JsonSerializer.Deserialize<InfrastructureHealthCheckRequest>(requestBody);

        switch (request?.type)
        {
            case InfrastructureHealthResourceType.AzureSqlDatabase:
                return new OkObjectResult(await CheckAzureSqlDatabase(request));
            case InfrastructureHealthResourceType.AzureStorageAccount:
                return new OkObjectResult(await CheckAzureStorageAccount(request));
            case InfrastructureHealthResourceType.AzureKeyVault:
                break;
            case InfrastructureHealthResourceType.AzureDatabricks:
                break;
            case InfrastructureHealthResourceType.AzureStorageQueue:
                break;
            case InfrastructureHealthResourceType.AzureWebApp:
                break;
            default:
                return new BadRequestObjectResult("Please pass a valid request body");
        }
        return new BadRequestObjectResult("Please pass a valid request body");
    }

    private async Task<InfrastructureHealthCheckResponse> CheckAzureKeyVault(InfrastructureHealthCheckRequest data)
    {
        throw new NotImplementedException();
    }

    private async Task<InfrastructureHealthCheckResponse> CheckAzureStorageAccount(
        InfrastructureHealthCheckRequest data)
    {
        throw new NotImplementedException();
    }

    private async Task<InfrastructureHealthCheckResponse> CheckAzureSqlDatabase(
        InfrastructureHealthCheckRequest request)
    {
        var errors = new List<string>();
        var check = new InfrastructureHealthCheck()
        {
            Group = request.group,
            Name = request.group,
            ResourceType = request.type,
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

        return new InfrastructureHealthCheckResponse(request, check, errors);
    }

    record InfrastructureHealthCheckRequest(InfrastructureHealthResourceType type, string group);

    record InfrastructureHealthCheckResponse(InfrastructureHealthCheckRequest request, InfrastructureHealthCheck check,
        List<string>? errors);
}