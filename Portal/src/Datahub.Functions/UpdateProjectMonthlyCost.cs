using Datahub.Core.Model.Datahub;
using Datahub.Core.Services.AzureCosting;
using Datahub.Infrastructure.Services.Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions;

public class UpdateProjectMonthlyCost
{
    private readonly DatahubProjectDBContext _dbContext;
    private readonly AzureConfig _azureConfig;
    private readonly ILogger<Core.Services.AzureCosting.AzureCostManagementService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public UpdateProjectMonthlyCost(DatahubProjectDBContext dbContext, IConfiguration configuration, ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory)
    {
        _dbContext = dbContext;
        _azureConfig = new(configuration);
        _logger = loggerFactory.CreateLogger<Core.Services.AzureCosting.AzureCostManagementService>();
        _httpClientFactory = httpClientFactory;
}
    
    // This Azure Function will be triggered everyday at 2:00 AM UTC and will capture the current cost for the month using the Azure forecast API
    // The function will then store the data in a Azure SQL database
    [Function("UpdateProjectMonthlyCostScheduled")]
    public async Task RunScheduledUpdate([TimerTrigger("0 0 2 * * *")] TimerInfo timerInfo)
    {
        await Run();
    }
    
    [Function("UpdateProjectMonthlyCostHttp")]
    public async Task<IActionResult> RunHttpUpdate([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
        HttpRequestData req)
    {
        try
        {
            var returnRecord = await RunAndReturnUpdatedProjects();
            return new OkObjectResult(returnRecord);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _logger.Log(LogLevel.Error, "Error updating project monthly cost");
            return new BadRequestObjectResult(e.Message);
        }
    }

    private async Task Run()
    {
        await RunAndReturnUpdatedProjects();
    }

    private async Task<CostReturnRecord> RunAndReturnUpdatedProjects()
    {
        var service = new Core.Services.AzureCosting.AzureCostManagementService(_dbContext, (ILogger<Core.Services.AzureCosting.AzureCostManagementService>)_logger);

        // acquire the access token
        var azureManager = new Infrastructure.Services.Azure.AzureManagementService(_azureConfig, _httpClientFactory);
        var token = await azureManager.GetAccessTokenAsync(default);

        //Get current month cost for each resource
        var currentMonthlyCosts = (await service.GetCurrentMonthlyCostAsync(_azureConfig.SubscriptionId, token)).ToList();
        
        //group monthly cost by project and update the database
        await service.UpdateProjectMonthlyCostToDateAsync(currentMonthlyCosts);
        return new CostReturnRecord
        {
            NumberOfProjectsUpdated = currentMonthlyCosts.Count
        };
    }
}

public record CostReturnRecord
{
    public int NumberOfProjectsUpdated { get; init; }
};
