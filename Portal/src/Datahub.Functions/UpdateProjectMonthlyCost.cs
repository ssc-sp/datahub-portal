using System.Text.Json;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services.AzureCosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace Datahub.Functions;

public class UpdateProjectMonthlyCost
{
    private readonly DatahubProjectDBContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UpdateProjectMonthlyCost> _logger;
    private const string TENANT_ID = "TENANT_ID";
    private const string CLIENT_ID = "FUNC_SP_CLIENT_ID";
    private const string CLIENT_SECRET = "FUNC_SP_CLIENT_SECRET";
    private const string SUBSCRIPTION_ID = "SUBSCRIPTION_ID";
    public UpdateProjectMonthlyCost(DatahubProjectDBContext dbContext, IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = loggerFactory.CreateLogger<UpdateProjectMonthlyCost>();
    }
    
    // This Azure Function will be triggered everyday at 2:00 AM UTC and will capture the current cost for the month using the Azure forecast API
    // The function will then store the data in a Azure SQL database
    [Function("UpdateProjectMonthlyCostScheduled")]
    public async Task RunScheduledUpdate([TimerTrigger("0 0 2 * * *")]MyInfo myTimer)
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
        //Get the required environment variables
        var subscriptionId = _configuration[SUBSCRIPTION_ID];
        var tenantId = _configuration[TENANT_ID];
        var clientId = _configuration[CLIENT_ID];
        var clientSecret = _configuration[CLIENT_SECRET];

        var service = new AzureCostManagementService(_dbContext, (ILogger<AzureCostManagementService>) _logger);
        //Acquire the access token
        var token = await GetAccessTokenAsync(tenantId, clientId, clientSecret);
        
        //Get current month cost for each resource
        var currentMonthlyCosts = (await service.GetCurrentMonthlyCostAsync(subscriptionId, token)).ToList();
        
        //group monthly cost by project and update the database
        await service.UpdateProjectMonthlyCostToDateAsync(currentMonthlyCosts);
        return new CostReturnRecord
        {
            NumberOfProjectsUpdated = currentMonthlyCosts.Count
        };
    }
    
    //Given tenantId, clientId, and clientSecret this function will return an access token
    private async Task<string> GetAccessTokenAsync(string tenantId, string clientId, string clientSecret)
    {
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://login.microsoftonline.com/" + tenantId + "/oauth2/token");
        tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            {"grant_type", "client_credentials"},
            {"client_id", clientId},
            {"client_secret", clientSecret},
            {"resource", "https://management.azure.com/"}
        });
        var tokenResponse = await new HttpClient().SendAsync(tokenRequest);
        var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();
        var tokenResponseJson = JsonDocument.Parse(tokenResponseContent);
        return tokenResponseJson.RootElement.GetProperty("access_token").GetString();
    }

}

public record CostReturnRecord
{
    public int NumberOfProjectsUpdated { get; init; }
};

public class MyInfo
{
    public MyScheduleStatus ScheduleStatus { get; set; }

    public bool IsPastDue { get; set; }
}

public class MyScheduleStatus
{
    public DateTime Last { get; set; }

    public DateTime Next { get; set; }

    public DateTime LastUpdated { get; set; }
}
