using Datahub.Core.EFCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Datahub.Core.Services.AzureCosting
{
    public class AzureCostManagementService
    {
        private const string AZURE_BASE_URL = "https://management.azure.com/";
        private DatahubProjectDBContext _dbContext;

        public AzureCostManagementService(DatahubProjectDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<CostManagementRow>> GetCurrentMonthlyCostAsync(string subscriptionId, string token)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

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
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException($"Error accessing billing API: {response.StatusCode.ToString()}");
            var content = await response.Content.ReadAsStringAsync();
            var rows = JsonNode.Parse(content)?["properties"]?["rows"] as JsonArray;
            return rows?.Select(r => new CostManagementRow(r as JsonArray)).Where(d => d.TagName is not null);
        }

        public async Task UpdateProjectMonthlyCostToDateAsync(List<CostManagementRow> currentMonthlyCosts)
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
}
