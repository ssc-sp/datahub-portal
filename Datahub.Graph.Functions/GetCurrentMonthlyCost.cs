using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Datahub.Graph.Functions;

public static class GetCurrentMonthlyCost
{
    // This Azure Function will be triggered everyday at 2:00 AM UTC and will capture the current cost for the month using the Azure forecast API
    // The function will then store the data in a Azure SQL database
    //[TimerTrigger("0 0 2 * * *")]TimerInfo myTimer  
    [FunctionName("GetCurrentMonthlyCost")]
    public static async Task Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
    HttpRequest req, ILogger log)
    {
        //Get the required environment variables
        // Get Subscription ID from Environment Variable
        var subscriptionId = Environment.GetEnvironmentVariable("SUBSCRIPTION_ID");
        //TEMPORARILY Get Token from Environment Variable
        var token = Environment.GetEnvironmentVariable("TOKEN");
        var url = "https://management.azure.com/";
        
        //construct the request body
        var body = new CostManagementRequestBody();
        var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        body.timePeriod = new TimePeriod()
        {
            from = firstDayOfMonth,
            to = firstDayOfMonth.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59)
        };
        var serializedBody = JsonSerializer.Serialize(body);
        var client = new HttpClient();
        client.BaseAddress = new Uri(url);
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
        //Add the request body to the request
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"subscriptions/{subscriptionId}/providers/Microsoft.CostManagement/query?api-version=2021-10-01");
        request.Content = new StringContent(serializedBody, System.Text.Encoding.UTF8, "application/json");
        var response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var rows = JsonNode.Parse(content)?["properties"]?["rows"] as JsonArray;
        var data = rows.Select(r => new CostManagementRow(r as JsonArray)).ToList();
        var projectCosts = data.Where(d => d.TagName is not null).GroupBy(d => d.TagName).Select(g => (g.Key, g.Sum(d => d.TotalCost))).ToList();
        ;
        //connect to database using environment variable "datahub-mssql-project"
        //insert data into database




    }
}