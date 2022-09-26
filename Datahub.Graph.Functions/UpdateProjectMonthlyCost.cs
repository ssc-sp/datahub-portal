using Datahub.Graph.Functions.CostManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Datahub.Core.EFCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Datahub.Graph.Functions;

public class UpdateProjectMonthlyCost
{
    private readonly DatahubProjectDBContext _dbContext;
    private const string AZURE_BASE_URL = "https://management.azure.com/";
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
    // This is temporarily triggered via a HTTP request to allow for testing. Will be replace with:
    //[TimerTrigger("0 0 2 * * *")]TimerInfo myTimer  
    [FunctionName("UpdateProjectMonthlyCost")]
    public async Task Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
        HttpRequest req, ILogger log)
    {
        //Get the required environment variables
        var subscriptionId = Environment.GetEnvironmentVariable(SUBSCRIPTION_ID);
        var tenantId = Environment.GetEnvironmentVariable(TENANT_ID);
        var clientId = Environment.GetEnvironmentVariable(CLIENT_ID);
        var clientSecret = Environment.GetEnvironmentVariable(CLIENT_SECRET);

        //Acquire the access token
        var token = await GetAccessTokenAsync(tenantId, clientId, clientSecret);
        
        //Get current month cost for each resource
        var currentMonthlyCosts = await GetCurrentMonthlyCostAsync(subscriptionId, token);
        
        //group monthly cost by project and update the database
        await UpdateProjectMonthlyCostToDateAsync(currentMonthlyCosts.ToList());

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

    private async Task<IEnumerable<CostManagementRow>> GetCurrentMonthlyCostAsync(string subscriptionId, string token)
    {
        var body = new CostManagementRequestBody();
        var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        body.timePeriod = new TimePeriod()
        {
            from = firstDayOfMonth,
            to = firstDayOfMonth.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59)
        };
        var serializedBody = JsonSerializer.Serialize(body);
        var client = new HttpClient();
        client.BaseAddress = new Uri(AZURE_BASE_URL);
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
        //Add the request body to the request
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"subscriptions/{subscriptionId}/providers/Microsoft.CostManagement/query?api-version=2021-10-01");
        request.Content = new StringContent(serializedBody, System.Text.Encoding.UTF8, "application/json");
        var response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var rows = JsonNode.Parse(content)?["properties"]?["rows"] as JsonArray;
        return rows?.Select(r => new CostManagementRow(r as JsonArray)).Where(d => d.TagName is not null);
    }
    
    private async Task UpdateProjectMonthlyCostToDateAsync(List<CostManagementRow> currentMonthlyCosts)
    {
        var projectsMonthlyCost = currentMonthlyCosts.GroupBy(c => c.TagName)
            .Select(g => new Project_Current_Monthly_Cost()
            {
                ProjectAcronym = g.Key,
                TotalCost = g.Sum(r => r.TotalCost),
                UpdateDate = DateTime.Now,
                TotalCostUSD = g.Sum(r => r.TotalCostUSD)
            }).ToList();

        foreach (var project in projectsMonthlyCost)
        {
            var existingProject = _dbContext.Project_Current_Monthly_Costs.FirstOrDefault(p => p.ProjectAcronym == project.ProjectAcronym);
            if (existingProject is not null)
            {
                existingProject.TotalCost = project.TotalCost;
                existingProject.TotalCostUSD = project.TotalCostUSD;
                existingProject.UpdateDate = project.UpdateDate;
            }
            else
            {
                _dbContext.Project_Current_Monthly_Costs.Add(project);
            }
        }
        await _dbContext.SaveChangesAsync();
    }
}