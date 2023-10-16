using System.Net;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Health;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

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
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
    {
        var defaultComponent = InfrastructureHealthResourceType.AzureSqlDatabase;

        switch (defaultComponent)
        {
            case InfrastructureHealthResourceType.AzureSqlDatabase:
                return await CheckAzureSqlDatabase(req);
            case InfrastructureHealthResourceType.AzureStorageAccount:
                break;
            case InfrastructureHealthResourceType.AzureKeyVault:
                break;
            case InfrastructureHealthResourceType.AzureDatabricks:
                break;
            case InfrastructureHealthResourceType.AzureStorageQueue:
                break;
            case InfrastructureHealthResourceType.AzureWebApp:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // return bad request if no component is found
        var response = req.CreateResponse(HttpStatusCode.BadRequest);
        await response.WriteStringAsync($"{nameof(InfrastructureHealthResourceType)} not found: {defaultComponent.ToString()}");
        return response;
    }

    private async Task<HttpResponseData> CheckAzureSqlDatabase(HttpRequestData req)
    {
        HttpResponseData response;
        bool connectable = _projectDbContext.Database.CanConnect();
        if (!connectable)
        {
            response = req.CreateResponse(HttpStatusCode.InternalServerError);
            response.WriteString("Cannot connect to database.");
        }

        var test = _projectDbContext.Projects.First();
        if (test == null)
        {
            response = req.CreateResponse(HttpStatusCode.InternalServerError);
            response.WriteString("Cannot retrieve from the database.");
        }
        else
        {
            response = req.CreateResponse(HttpStatusCode.OK);
            response.WriteString("Successfully connected and checked database.");
        }

        return response;
    }
}