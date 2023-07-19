using System.Net;
using Datahub.Infrastructure.Services.Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions;

public class UpdateStorageCapacityFunction
{
    private readonly AzureConfig _config;
    private readonly ILogger _logger;
    private readonly AzureManagementService _azureManagementService;

    public UpdateStorageCapacityFunction(ILoggerFactory loggerFactory, AzureConfig config, AzureManagementService azureManagementService)
    {
        _logger = loggerFactory.CreateLogger<UpdateStorageCapacityFunction>();
        _config = config;
        _azureManagementService = azureManagementService;
    }

    //[Function("UpdateStorageCapacityFunction")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req, CancellationToken cancellationToken)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var session = await _azureManagementService.GetSession(cancellationToken);
        var capacity = await session.GetTotalAverageStorageCapacity("fsdh_proj_die1_dev_rg", "fsdh-dbk-die1-dev-rg");

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        response.WriteString($"Capacity: {capacity}");

        return response;
    }
}
