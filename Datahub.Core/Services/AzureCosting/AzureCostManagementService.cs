using Datahub.Core.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Configuration;
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
        private const string BILLING_CONFIG_SECTION = "Billing";
        private const string SUBSCRIPTION_ID_CONFIG_KEY = "AzureSubscription";
        private const string RESOURCE_GROUP_CONFIG_KEY = "ResourceGroup";

        private DatahubProjectDBContext _dbContext;
        private readonly ILogger<AzureCostManagementService> logger;
        private ITokenAcquisition _tokenAcquisition;
        private MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;



        private string _subscriptionId;
        private string _resourceGroup;

        public static readonly string[] RequiredScopes = new string[]
        {
            "https://management.azure.com/user_impersonation"
        };


        public AzureCostManagementService(
            DatahubProjectDBContext dbContext, 
            ILogger<AzureCostManagementService> logger, 
            ITokenAcquisition tokenAcquisition, 
            MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            this.logger = logger;
            _tokenAcquisition = tokenAcquisition;
            _consentHandler = consentHandler;
            _subscriptionId = configuration.GetSection(BILLING_CONFIG_SECTION).GetValue<string>(SUBSCRIPTION_ID_CONFIG_KEY);
            _resourceGroup = configuration.GetSection(BILLING_CONFIG_SECTION).GetValue<string>(RESOURCE_GROUP_CONFIG_KEY);

            if (string.IsNullOrEmpty(_resourceGroup))
            {
                _resourceGroup = null;
            }
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var token = default(string);

            try
            {
                token = await _tokenAcquisition.GetAccessTokenForUserAsync(RequiredScopes);
            }
            catch (MicrosoftIdentityWebChallengeUserException e)
            {
                // user isn't logged into Power BI -> redirect to Microsoft
                _consentHandler.HandleException(e);
            }
            catch (HttpOperationException)
            {
                // couldn't load the report - missing or unauthorized
            }
            catch (Exception)
            {
                // some other exception, crash and log like normal
                throw;
            }

            return await Task.FromResult(token);
        }

        public async Task<IEnumerable<CostManagementRow>> GetCurrentMonthlyCostAsync(string token) => await GetCurrentMonthlyCostAsync(_subscriptionId, token, _resourceGroup);

        public async Task<IEnumerable<CostManagementRow>> GetCurrentMonthlyCostAsync(string subscriptionId, string token, string resourceGroup = null)
        {
            logger.LogDebug($"Querying Azure for subscription '{subscriptionId}' - Resource Group: '{resourceGroup}'");
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }
            var resourceGroupFilter = resourceGroup is null?string.Empty:$"/resourceGroups/{resourceGroup}";

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
            ///
            var requestUrl = $"subscriptions/{subscriptionId}{resourceGroupFilter}/providers/Microsoft.CostManagement/query?api-version=2021-10-01";
            logger.LogDebug($"Request: {requestUrl}");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
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
