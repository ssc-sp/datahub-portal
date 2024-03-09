using System.Text.Json;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.CostManagement;
using Azure.ResourceManager.CostManagement.Models;
using Datahub.Application.Services.Azure;
using Datahub.Application.Services.Budget;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Utils;
using Datahub.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services.Cost
{
    public class WorkspaceCostManagementService : IWorkspaceCostManagementService
    {
        private ArmClient _armClient;
        private readonly ILogger<WorkspaceCostManagementService> _logger;
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;

        public WorkspaceCostManagementService(IAzureResourceManagerClientProvider armClientProvider,
            ILogger<WorkspaceCostManagementService> logger, IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _armClient = armClientProvider.GetClient();
        }

        public async Task<List<DailyServiceCost>> GetCostByPeriodAsync(string workspaceAcronym, DateTime date)
        {
            var costs = await QueryWorkspaceCosts(workspaceAcronym, date.Add(TimeSpan.FromDays(-1)),
                date.Add(TimeSpan.FromDays(1)));
            var filteredCosts = costs.Where(c => c.Date == date).ToList();
            return filteredCosts;
        }

        public async Task<List<DailyServiceCost>> GetCostByPeriodAsync(string workspaceAcronym, DateTime startDate,
            DateTime endDate)
        {
            return await QueryWorkspaceCosts(workspaceAcronym, startDate, endDate);
        }

        public async Task<List<DailyServiceCost>> GetAllCostsAsync(string workspaceAcronym)
        {
            return await QueryWorkspaceCosts(workspaceAcronym, DateTime.MinValue, DateTime.Today);
        }

        public Task<List<DailyServiceCost>> GroupBySource(List<DailyServiceCost> costs)
        {
            var newCosts = new List<DailyServiceCost>();
            costs.ForEach(c => newCosts.Add(
                new DailyServiceCost()
                {
                    Amount = c.Amount,
                    Source = c.Source,
                    Date = c.Date;
                }));
        }

        public Task<List<DailyServiceCost>> GroupByDate(List<DailyServiceCost> costs)
        {
            throw new NotImplementedException();
        }

        private async Task<List<DailyServiceCost>> QueryWorkspaceCosts(string workspaceAcronym, DateTime startDate,
            DateTime endDate)
        {
            var rgNames = await GetResourceGroupNames(workspaceAcronym);
            rgNames.ForEach(async rg =>
            {
                var queryResult = await QueryResourceGroupCosts(rg, startDate, endDate);
                var dailyCosts = await ParseQueryResult(queryResult);
            });
        }

        private async Task<List<DailyServiceCost>> ParseQueryResult(QueryResult queryResult)
        {
            var costs = new List<DailyServiceCost>();
            var lstDailyCosts = new List<DailyServiceCost>();
            var cols = queryResult.Columns.ToList().Select(c => c.Name).ToList();
            queryResult.Rows.ToList().ForEach(r =>
            {
                lstDailyCosts.Add(new DailyServiceCost()
                {
                    Amount = r[cols.FindIndex(c => c == "Cost")].ToString();
                });
            });
            return costs;
        }

        private async Task<QueryResult> QueryResourceGroupCosts(string rgName, DateTime startDate, DateTime endDate)
        {
            var scope = new ResourceIdentifier(rgName);
            var dataset = new QueryDataset();
            var queryTimePeriod = new QueryTimePeriod(startDate, endDate);
            var query = new QueryDefinition(ExportType.ActualCost, TimeframeType.Custom, dataset);

            dataset.Granularity = GranularityType.Daily;
            query.TimePeriod = queryTimePeriod;

            var response = await _armClient.UsageQueryAsync(scope, query);

            if (!response.HasValue)
            {
                _logger.LogError($"Could not get cost data for resource group {rgName}");
                throw new Exception($"Could not get cost data for resource group {rgName}");
            }

            return response.Value;
        }

        private async Task<List<string>> GetResourceGroupNames(string workspaceAcronym)
        {
            using var ctx = await _dbContextFactory.CreateDbContextAsync();

            var blobStorageType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureStorageBlob);
            var dbkType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureDatabricks);

            var resources = ctx.Project_Resources2.Include(r => r.Project)
                .Where(r => r.Project.Project_Acronym_CD == workspaceAcronym);

            var rgNames = new List<string>();
            var dbkIds = new HashSet<int>();

            await resources.ForEachAsync(r =>
            {
                if (r.ResourceType == dbkType)
                {
                    dbkIds.Add(r.ProjectId);
                }
            });

            await resources.ForEachAsync(r =>
            {
                if (r.ResourceType == blobStorageType)
                {
                    rgNames.Add(ParseResourceGroup(r.JsonContent));
                    if (dbkIds.Contains(r.ProjectId))
                    {
                        rgNames.Add(ParseDbkResourceGroup(r.JsonContent));
                    }
                }
            });

            return rgNames;
        }

        private string ParseDbkResourceGroup(string jsonContent)
        {
            var content = JsonSerializer.Deserialize<RgNameObject>(jsonContent);
            var dbk = content.resource_group_name.Split("_").Select((s, idx) => idx == 1 ? "dbk" : s);
            var dbkRg = string.Join("-", dbk);
            return dbkRg;
        }

        private string ParseResourceGroup(string jsonContent)
        {
            var content = JsonSerializer.Deserialize<RgNameObject>(jsonContent);
            return content?.resource_group_name;
        }


        private record RgNameObject(string resource_group_name);
    }
}