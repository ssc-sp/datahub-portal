using System.Runtime.CompilerServices;
using System.Text.Json;
using Azure;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.CostManagement;
using Azure.ResourceManager.CostManagement.Models;
using Datahub.Application.Services.Budget;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("Datahub.Infrastructure.UnitTests")]

namespace Datahub.Infrastructure.Services.Cost
{
    public class WorkspaceCostManagementService : IWorkspaceCostManagementService
    {
        private ArmClient _armClient;
        private readonly ILogger<WorkspaceCostManagementService> _logger;
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;

        public WorkspaceCostManagementService(ArmClient armClient,
            ILogger<WorkspaceCostManagementService> logger, IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _armClient = armClient;
        }

        /// <summary>
        /// Updates the Project_Costs and Project_Credits for the given workspace acronym.
        /// </summary>
        /// <param name="subCosts">The costs at the subscription level</param>
        /// <param name="workspaceAcronym">The workspace acronym</param>
        /// <returns>(bool, decimal), a tuple representing whether a rollover is needed according to this update and the amount of costs captured in the last fiscal year</returns>
        public async Task<(bool, decimal)> UpdateWorkspaceCostAsync(List<DailyServiceCost> subCosts,
            string workspaceAcronym)
        {
            using var ctx = await _dbContextFactory.CreateDbContextAsync();
            var project = await ctx.Projects.FirstOrDefaultAsync(p => p.Project_Acronym_CD == workspaceAcronym);
            if (project is null)
            {
                _logger.LogError($"Could not find project with acronym {workspaceAcronym}");
                return (false, 0);
            }

            var projectCredits = await ctx.Project_Credits.FirstOrDefaultAsync(c => c.ProjectId == project.Project_ID);
            if (projectCredits is null)
            {
                projectCredits = new Project_Credits()
                {
                    ProjectId = project.Project_ID
                };
                ctx.Project_Credits.Add(projectCredits);
            }

            var workspaceCosts = await GetWorkspaceCosts(subCosts, workspaceAcronym);
            var workspaceCurrentFYCosts = FilterCurrentFiscalYear(workspaceCosts);
            var workspaceLastFYCosts = FilterLastFiscalYear(workspaceCosts);
            var workspaceLastFYTotal = workspaceLastFYCosts.Sum(c => c.Amount);

            var yesterday = DateTime.UtcNow.Date.AddDays(-1);
            var yesterdayCosts = FilterDateRange(workspaceCurrentFYCosts, yesterday);

            projectCredits.Current = (double)workspaceCurrentFYCosts.Sum(c => c.Amount);
            projectCredits.YesterdayCredits = (double)yesterdayCosts.Sum(c => c.Amount);
            projectCredits.CurrentPerDay = JsonSerializer.Serialize(GroupByDate(workspaceCurrentFYCosts));
            projectCredits.CurrentPerService = JsonSerializer.Serialize(GroupBySource(workspaceCurrentFYCosts));
            projectCredits.YesterdayPerService = JsonSerializer.Serialize(GroupBySource(yesterdayCosts));
            var beforeUpdate = projectCredits.LastUpdate ?? DateTime.UtcNow;
            projectCredits.LastUpdate = DateTime.UtcNow;

            ctx.Project_Credits.Update(projectCredits);

            var projectCost = await ctx.Project_Costs.FirstAsync(c => c.Date == yesterday);
            if (projectCost is null)
            {
                var yesterdayByService = GroupBySource(yesterdayCosts);
                yesterdayByService.ForEach(c =>
                {
                    var cost = new Datahub_Project_Costs()
                    {
                        CadCost = (double)c.Amount,
                        CloudProvider = "azure",
                        Date = c.Date,
                        Project_ID = project.Project_ID,
                        ServiceName = c.Source
                    };
                    ctx.Project_Costs.Add(cost);
                });
            }

            await ctx.SaveChangesAsync();

            return (RolloverNeeded(beforeUpdate), workspaceLastFYTotal);
        }

        /// <summary>
        /// Queries the costs for the given subscription id within the given date range.
        /// </summary>
        /// <param name="subId">Subscription id, i.e. "/subscription/<...>"</param>
        /// <param name="startDate">The start date of the query</param>
        /// <param name="endDate">The end date of the query</param>
        /// <returns>A List containing all daily service costs. A daily service cost is a cost caused by one service during one day.</returns>
        public async Task<List<DailyServiceCost>?> QuerySubscriptionCosts(string subId, DateTime startDate,
            DateTime endDate)
        {
            var queryResult = await QueryScopeCosts(subId, startDate, endDate);
            return (queryResult is null) ? null : await ParseQueryResult(queryResult);
        }

        /// <summary>
        /// Groups the costs given by source. By executing this, you lose date information
        /// </summary>
        /// <param name="costs">The costs to group</param>
        /// <returns>The grouped costs</returns>
        public List<DailyServiceCost> GroupBySource(List<DailyServiceCost> costs)
        {
            return costs.GroupBy(c => c.Source).Select(g => new DailyServiceCost()
            {
                Amount = g.Sum(c => c.Amount),
                Source = g.Key,
                Date = g.Min(c => c.Date)
            }).ToList();
        }

        /// <summary>
        /// Groups the costs given by date. By executing this, you lose source information
        /// </summary>
        /// <param name="costs">The costs to group</param>
        /// <returns>The grouped costs</returns>
        public List<DailyServiceCost> GroupByDate(List<DailyServiceCost> costs)
        {
            return costs.GroupBy(c => c.Date).Select(g => new DailyServiceCost()
            {
                Amount = g.Sum(c => c.Amount),
                Source = "Day",
                Date = g.Key
            }).ToList();
        }

        /// <summary>
        /// Filters the given costs to be only within the current fiscal year
        /// </summary>
        /// <param name="costs">The costs to filter</param>
        /// <returns>The filtered costs, which are all in the current fiscal year</returns>
        public List<DailyServiceCost> FilterCurrentFiscalYear(List<DailyServiceCost> costs)
        {
            var (startDate, endDate) = GetCurrentFiscalYear();

            return FilterDateRange(costs, startDate, endDate);
        }

        /// <summary>
        /// Filters the given costs to be only within the last fiscal year
        /// </summary>
        /// <param name="costs">The costs to filter</param>
        /// <returns>The filtered costs, which are all in the last fiscal year</returns>
        public List<DailyServiceCost> FilterLastFiscalYear(List<DailyServiceCost> costs)
        {
            var (startDate, endDate) = GetCurrentFiscalYear();

            return FilterDateRange(costs, startDate.AddYears(-1), endDate.AddYears(-1));
        }

        /// <summary>
        /// Filters the given costs to be only within a date range
        /// </summary>
        /// <param name="costs">The costs to filter</param>
        /// <param name="startDate">The start of the date range</param>
        /// <param name="endDate">The end of the date range</param>
        /// <returns>The filtered costs, which should be between the dates provided, inclusively</returns>
        public List<DailyServiceCost> FilterDateRange(List<DailyServiceCost> costs, DateTime startDate,
            DateTime endDate)
        {
            return costs.Where(c => (c.Date >= startDate && c.Date <= endDate)).ToList();
        }

        /// <summary>
        /// Filters the given costs to be only from a given date
        /// </summary>
        /// <param name="costs">The costs to filter</param>
        /// <param name="date">The date of interest</param>
        /// <returns>The filtered costs, which should be only from the given date</returns>
        public List<DailyServiceCost> FilterDateRange(List<DailyServiceCost> costs, DateTime date)
        {
            return costs.Where(c => c.Date == date).ToList();
        }

        /// <summary>
        /// Gets the costs for the given workspace acronym from the given list of subscription level costs
        /// </summary>
        /// <param name="subCosts">Costs at the subscription level</param>
        /// <param name="workspaceAcronym">Workspace acronym</param>
        /// <returns>List of daily service costs for the workspace. A daily service cost is a cost caused by one service during one day.</returns>
        public async Task<List<DailyServiceCost>> GetWorkspaceCosts(List<DailyServiceCost> subCosts,
            string workspaceAcronym)
        {
            var rgNames = await GetResourceGroupNames(workspaceAcronym);
            var workspaceCosts = new List<DailyServiceCost>();

            subCosts.ForEach(c =>
            {
                if (rgNames.Contains(c.ResourceGroupName))
                {
                    workspaceCosts.Add(c);
                }
            });

            workspaceCosts.OrderBy(c => c.Date);

            return workspaceCosts;
        }

        /// <summary>
        /// Queries the given scopes for costs within the given date range. Daily granularity.
        /// </summary>
        /// <param name="scopeId">The id of the scope. e.g. /subscription/<...> </param>
        /// <param name="startDate">The start date</param>
        /// <param name="endDate">The end date</param>
        /// <returns>Returns the result of the query or null if the query was throttled.</returns>
        /// <exception cref="Exception">Throws exception if the query was incorrect</exception>
        internal async Task<QueryResult?> QueryScopeCosts(string scopeId, DateTime startDate, DateTime endDate)
        {
            var scope = new ResourceIdentifier(scopeId);
            var dataset = new QueryDataset();
            var queryTimePeriod = new QueryTimePeriod(startDate, endDate);
            var filter1 = new QueryFilter();
            filter1.Dimensions = new QueryComparisonExpression("ResourceGroupName", , )
            dataset.Granularity = GranularityType.Daily;
            dataset.Grouping.Add(new QueryGrouping(QueryColumnType.Dimension, "ServiceName"));
            dataset.Grouping.Add(new QueryGrouping(QueryColumnType.Dimension, "ResourceGroupName"));
            dataset.Filter.And.Add(new QueryFilter("Cost", new QueryComparisonExpression, 0));
            dataset.Aggregation.Add("Cost", new QueryAggregation("Cost", FunctionType.Sum));
            var query = new QueryDefinition(ExportType.ActualCost, TimeframeType.Custom, dataset);

            query.TimePeriod = queryTimePeriod;
            Response<QueryResult> response = null;

            try
            {
                response = await _armClient.UsageQueryAsync(scope, query);
            }
            catch (RequestFailedException e)
            {
                _logger.LogError(e, $"Could not get cost data for scope {scopeId}");
                return null;
            }

            if (!response.HasValue)
            {
                _logger.LogError($"Could not get cost data for scope {scopeId}");
                throw new Exception($"Could not get cost data for scope {scopeId}");
            }

            return response.Value;
        }

        internal async Task<List<string>> GetResourceGroupNames(string workspaceAcronym)
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
                        dbkIds.Remove(r.ProjectId);
                    }
                }
            });

            return rgNames;
        }

        internal (DateTime startDate, DateTime endDate) GetCurrentFiscalYear()
        {
            var today = DateTime.UtcNow.Date;

            var startYear = today.Year;
            var endYear = today.Year + 1;

            if (today.Month < 4)
            {
                startYear = today.Year - 1;
                endYear = today.Year;
            }

            var startDate = new DateTime(startYear, 4, 1);
            var endDate = new DateTime(endYear, 3, 31);
            return (startDate, endDate);
        }

        internal bool RolloverNeeded(DateTime lastUpdate)
        {
            var (currFiscalYearStart, _) = GetCurrentFiscalYear();
            return lastUpdate < currFiscalYearStart;
        }

        internal string ParseDbkResourceGroup(string jsonContent)
        {
            var content = JsonSerializer.Deserialize<RgNameObject>(jsonContent);
            var dbk = content.resource_group_name.Split("_").Select((s, idx) => idx == 1 ? "dbk" : s);
            var dbkRg = string.Join("-", dbk);
            return dbkRg;
        }

        internal string ParseResourceGroup(string jsonContent)
        {
            var content = JsonSerializer.Deserialize<RgNameObject>(jsonContent);
            return content?.resource_group_name;
        }

        internal async Task<List<DailyServiceCost>> ParseQueryResult(QueryResult queryResult)
        {
            var lstDailyCosts = new List<DailyServiceCost>();

            var cols = queryResult.Columns.ToList().Select(c => c.Name).ToList();

            queryResult.Rows.ToList().ForEach(r =>
            {
                lstDailyCosts.Add(new DailyServiceCost()
                {
                    Amount = decimal.Parse(r[cols.FindIndex(c => c == "Cost")].ToString()),
                    Source = r[cols.FindIndex(c => c == "ServiceName")].ToString(),
                    Date = DateTime.Parse(r[cols.FindIndex(c => c == "UsageDate")].ToString()),
                    ResourceGroupName = r[cols.FindIndex(c => c == "ResourceGroupName")].ToString()
                });
            });

            return lstDailyCosts;
        }

        internal record RgNameObject(string resource_group_name);
    }
}