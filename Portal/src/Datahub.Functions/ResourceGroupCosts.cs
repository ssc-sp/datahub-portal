using Datahub.Infrastructure.Services.Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Datahub.Functions
{
    public class ResourceGroupCosts
    {
        private readonly ILogger _logger;
        private readonly AzureManagementService _azureManagementService;

        public ResourceGroupCosts(ILoggerFactory loggerFactory, AzureManagementService azureManagementService)
        {
            _logger = loggerFactory.CreateLogger<ResourceGroupCosts>();
            _azureManagementService = azureManagementService;
        }

        //[Function("ResourceGroupCosts")]
        public async Task<HttpResponseData> RunResourceGroupCosts([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req, CancellationToken ct)
        {
            var session = await _azureManagementService.GetSession(ct);

            var cost = await session.GetResourceGroupMonthlyCost("sp-datahub-sand-rg");
            _logger.LogInformation($"Cost: {cost.Value}");

            var dailyCosts = await session.GetResourceGroupMonthlyCostPerDay("sp-datahub-sand-rg");

            cost = await session.GetResourceGroupYesterdayCost("sp-datahub-sand-rg");
            _logger.LogInformation($"Cost: {cost.Value}");

            var costs = await session.GetResourceGroupMonthlyCostByService("sp-datahub-sand-rg");
            var costReport = string.Join("\n", costs?.Select(c => $"{c.Name}\t${c.Cost}").ToList() ?? new());
            _logger.LogInformation($"Cost: {costReport}");

            costs = await session.GetResourceGroupYesterdayCostByService("sp-datahub-sand-rg");
            costReport = string.Join("\n", costs?.Select(c => $"{c.Name}\t${c.Cost}").ToList() ?? new());
            _logger.LogInformation($"Cost: {costReport}");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            return response;
        }
    }
}
