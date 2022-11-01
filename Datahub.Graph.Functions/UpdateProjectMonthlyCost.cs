using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Datahub.Core.EFCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Datahub.Core.Services.AzureCosting;

namespace Datahub.Graph.Functions;

public class UpdateProjectMonthlyCost
{
    private readonly DatahubProjectDBContext _dbContext;
    private const string TENANT_ID = "TENANT_ID";
    private const string CLIENT_ID = "FUNC_SP_CLIENT_ID";
    private const string CLIENT_SECRET = "FUNC_SP_CLIENT_SECRET";
    private const string SUBSCRIPTION_ID = "SUBSCRIPTION_ID";
    public UpdateProjectMonthlyCost(DatahubProjectDBContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    // This Azure Function will be triggered everyday at 2:00 AM UTC and will capture the current cost for the month using the Azure forecast API
    // The function will then store the data in a Azure SQL database
    [FunctionName("UpdateProjectMonthlyCostScheduled")]
    public async Task RunScheduledUpdate([TimerTrigger("0 0 2 * * *")]TimerInfo myTimer, ILogger log)
    {
        await Run(log);
    }
    // Same as above but can be triggered manually
    [FunctionName("UpdateProjectMonthlyCostHttp")]
    public async Task<IActionResult> RunHttpUpdate([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
        HttpRequest req, ILogger log)
    {
        try
        {

            var returnRecord = await RunAndReturnUpdatedProjects(log);
            return new OkObjectResult(returnRecord);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            log.Log(LogLevel.Error, "Error updating project monthly cost");
            return new BadRequestObjectResult(e.Message);
        }
    }

    private async Task Run(ILogger log)
    {
        await RunAndReturnUpdatedProjects(log);

    }

    private async Task<CostReturnRecord> RunAndReturnUpdatedProjects(ILogger log)
    {
        //Get the required environment variables
        var subscriptionId = Environment.GetEnvironmentVariable(SUBSCRIPTION_ID);
        var tenantId = Environment.GetEnvironmentVariable(TENANT_ID);
        var clientId = Environment.GetEnvironmentVariable(CLIENT_ID);
        var clientSecret = Environment.GetEnvironmentVariable(CLIENT_SECRET);

        var service = new AzureCostManagementService(_dbContext, (ILogger<AzureCostManagementService>) log);
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
