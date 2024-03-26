using System.Globalization;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Azure;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.CostManagement;
using Azure.ResourceManager.CostManagement.Models;
using Datahub.Application.Services.Budget;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Infrastructure.Services.Azure;
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

        private const string COST_COLUMN = "Cost";
        private const string SERVICE_NAME_COLUMN = "ServiceName";
        private const string USAGE_DATE_COLUMN = "UsageDate";
        private const string RESOURCE_GROUP_NAME_COLUMN = "ResourceGroupName";

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

            // Get the costs for the workspace both from the query and the database
            // The costs from the query are the costs from the last several days
            // The costs from the database are all costs existing for a workspace
            var queryWorkspaceCosts = await FilterWorkspaceCosts(subCosts, workspaceAcronym);

            // We get the dates from the query and exclude today
            var dates = queryWorkspaceCosts.Select(c => c.Date).Distinct().ToList();
            dates = dates.Where(d => d >= DateTime.UtcNow.Date.AddDays(-7) && !d.Equals(DateTime.UtcNow.Date)).ToList();

            // For each of these dates (at most 7 days), we verify that the database contains the costs for that day
            // to ensure resilience against query failures
            // We add these costs to the database if they do not exist
            dates.ForEach(async date =>
            {
                var projectCost = await ctx.Project_Costs.FirstAsync(c => c.Date == date);
                var thatDateQueryCosts = FilterDateRange(queryWorkspaceCosts, date);
                
                if (projectCost is null)
                {
                    var thatDateByService = GroupBySource(thatDateQueryCosts);
                    thatDateByService.ForEach(c =>
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
            });

            // At this point, the costs in the database (Project_Costs) include all times excluding today.
            // To get the totals for the current fiscal year (Project_Credits), we just take the costs in the database in the current fiscal year
            // and we add the costs from today from the query
            var queryWorkspaceTodayCosts = FilterDateRange(queryWorkspaceCosts, DateTime.UtcNow.Date);
            var dbWorkspaceCosts = await GetWorkspaceCosts(workspaceAcronym);
            var dbWorkspaceCurrentFYCosts = FilterCurrentFiscalYear(dbWorkspaceCosts);
            var workspaceCurrentFYCosts = dbWorkspaceCurrentFYCosts.Concat(queryWorkspaceTodayCosts).ToList();
            var workspaceYesterdayCosts = FilterDateRange(dbWorkspaceCosts, DateTime.UtcNow.Date.AddDays(-1));
            var workspaceLastFYCosts = FilterLastFiscalYear(dbWorkspaceCosts);

            // Create the Project_Credits entry if it does not exist (in the case of a new workspace)
            var projectCredits = await ctx.Project_Credits.FirstOrDefaultAsync(c => c.ProjectId == project.Project_ID);
            if (projectCredits is null)
            {
                projectCredits = new Project_Credits()
                {
                    ProjectId = project.Project_ID
                };
                ctx.Project_Credits.Add(projectCredits);
            }

            // Get the last update time to check if a rollover is needed and the total costs incurred in the last fiscal year
            var beforeUpdate = projectCredits.LastUpdate ?? DateTime.UtcNow;
            var workspaceLastFYTotal = workspaceLastFYCosts.Sum(c => c.Amount);
            
            // Update the Project_Credits entry
            projectCredits.Current = (double)workspaceCurrentFYCosts.Sum(c => c.Amount);
            projectCredits.YesterdayCredits = (double)workspaceYesterdayCosts.Sum(c => c.Amount);
            projectCredits.CurrentPerDay = JsonSerializer.Serialize(GroupByDate(workspaceCurrentFYCosts));
            projectCredits.CurrentPerService = JsonSerializer.Serialize(GroupBySource(workspaceCurrentFYCosts));
            projectCredits.YesterdayPerService = JsonSerializer.Serialize(GroupBySource(workspaceYesterdayCosts));
            projectCredits.LastUpdate = DateTime.UtcNow;
            ctx.Project_Credits.Update(projectCredits);
            
            await ctx.SaveChangesAsync();

            // Return whether a rollover is needed and the total costs incurred in the last fiscal year (the last full FY)
            return (RolloverNeeded(beforeUpdate), workspaceLastFYTotal);
        }

        /// <summary>
        /// Queries the costs for the given subscription id within the given date range. The date range cannot be more than a year.
        /// </summary>
        /// <param name="subId">Subscription id, i.e. "/subscription/<...>"</param>
        /// <param name="startDate">The start date of the query</param>
        /// <param name="endDate">The end date of the query</param>
        /// <returns>A List containing all daily service costs. A daily service cost is a cost caused by one service during one day.</returns>
        public async Task<List<DailyServiceCost>?> QuerySubscriptionCosts(DateTime startDate,
            DateTime endDate)
        {
            if (startDate > endDate)
            {
                _logger.LogError("Start date is after end date");
                throw new Exception("Start date is after end date");
            }

            if (endDate-startDate > TimeSpan.FromDays(365))
            {
                _logger.LogError("Querying more than a year of data is not allowed.");
                throw new Exception("Querying more than a year of data is not allowed.");
            }
            
            var queryResult = await QueryScopeCosts(_armClient.GetDefaultSubscription().Id, startDate, endDate);
            return queryResult;
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
        /// Filters the costs for the given workspace acronym from the given list of subscription level costs
        /// </summary>
        /// <param name="subCosts">Costs at the subscription level</param>
        /// <param name="workspaceAcronym">Workspace acronym</param>
        /// <returns>List of daily service costs for the workspace. A daily service cost is a cost caused by one service during one day.</returns>
        public async Task<List<DailyServiceCost>> FilterWorkspaceCosts(List<DailyServiceCost> subCosts,
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
        /// Gets the costs for the given workspace acronym from the Project_Costs table
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym</param>
        /// <returns>A list of DailyServiceCosts</returns>
        /// <exception cref="Exception">Exceptions are thrown whenever the project is not found or it has no costs</exception>
        internal async Task<List<DailyServiceCost>> GetWorkspaceCosts(string workspaceAcronym)
        {
            using var ctx = await _dbContextFactory.CreateDbContextAsync();
            var project = await ctx.Projects.FirstOrDefaultAsync(p => p.Project_Acronym_CD == workspaceAcronym);
            if (project is null)
            {
                _logger.LogError($"Could not find project with acronym {workspaceAcronym}");
                throw new Exception($"Could not find project with acronym {workspaceAcronym}");
            }

            var projectCosts = ctx.Project_Costs.Where(c => c.Project_ID == project.Project_ID).ToList();
            if (projectCosts.Count == 0)
            {
                _logger.LogError($"Could not find costs for project with acronym {workspaceAcronym}");
                throw new Exception($"Could not find costs for project with acronym {workspaceAcronym}");
            }

            var workspaceCosts = ParseProjectCosts(projectCosts);
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
        internal async Task<List<DailyServiceCost>?> QueryScopeCosts(string scopeId, DateTime startDate,
            DateTime endDate)
        {
            using var ctx = await _dbContextFactory.CreateDbContextAsync();
            var scope = new ResourceIdentifier(scopeId);
            var dataset = new QueryDataset();
            var queryTimePeriod = new QueryTimePeriod(startDate, endDate);
            var filter1 = new QueryFilter();
            var allAcronyms = new[]
                { "DIE1", "DIE2" }; //ctx.Projects.AsNoTracking().Select(p => p.Project_Acronym_CD.ToUpper()).ToList();

            filter1.Tags = new QueryComparisonExpression("project_cd", QueryOperatorType.In, allAcronyms);
            dataset.Filter = filter1;

            dataset.Granularity = GranularityType.Daily;

            dataset.Grouping.Add(new QueryGrouping(QueryColumnType.Dimension, "ServiceName"));
            dataset.Grouping.Add(new QueryGrouping(QueryColumnType.Dimension, "ResourceGroupName"));

            dataset.Aggregation.Add("Cost", new QueryAggregation("Cost", FunctionType.Sum));

            var query = new QueryDefinition(ExportType.ActualCost, TimeframeType.Custom, dataset);
            query.TimePeriod = queryTimePeriod;

            Response<QueryResult> response = null;
            QueryResult result = null;
            var queryResults = new List<QueryResult> { };

            try
            {
                response = await _armClient.UsageQueryAsync(scope, query);

                if (!response.HasValue)
                {
                    _logger.LogError($"Could not get cost data for scope {scopeId}");
                    throw new Exception($"Could not get cost data for scope {scopeId}");
                }

                result = response!.Value;
                queryResults.Add(result);

                // Support for pagination
                while (!string.IsNullOrWhiteSpace(result.NextLink))
                {
                    var maxDate = GetLastDate(result);

                    // Make the new time period of the query include the last date of the previous query to ensure
                    // that we get all the data and that that date was not cut in half. Duplicates will be handled later
                    // in the parsing.
                    queryTimePeriod = new QueryTimePeriod(maxDate, endDate);
                    query.TimePeriod = queryTimePeriod;

                    response = await _armClient.UsageQueryAsync(scope, query);

                    if (!response.HasValue)
                    {
                        _logger.LogError($"Could not get cost data for scope {scopeId}");
                        throw new Exception($"Could not get cost data for scope {scopeId}");
                    }

                    result = response!.Value;
                    queryResults.Add(result);
                }
            }
            catch (RequestFailedException e)
            {
                _logger.LogError(e, $"Could not get cost data for scope {scopeId}");
                return null;
            }

            return ParseQueryResult(queryResults);
        }

        /// <summary>
        /// Gets the resource group names for the given workspace acronym
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym</param>
        /// <returns>List of the resource group names</returns>
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

        /// <summary>
        /// Gets the current fiscal year
        /// </summary>
        /// <returns>Tuple of (startDate, endDate) representing the current fiscal year</returns>
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

        /// <summary>
        /// Verifies if the last update was outside of the current fiscal year.
        /// </summary>
        /// <param name="lastUpdate">The datetime of the last update</param>
        /// <returns>true if the last update is outside the current fiscal year, false otherwise</returns>
        internal bool RolloverNeeded(DateTime lastUpdate)
        {
            var (currFiscalYearStart, _) = GetCurrentFiscalYear();
            return lastUpdate < currFiscalYearStart;
        }

        /// <summary>
        /// Gets the databricks resource group name from the json content of the blob storage resource
        /// </summary>
        /// <param name="jsonContent">Project_Resource.JsonContent of the blob storage resource of a workspace</param>
        /// <returns>The databricks managed resource group name</returns>
        internal string ParseDbkResourceGroup(string jsonContent)
        {
            var content = JsonSerializer.Deserialize<RgNameObject>(jsonContent);
            var dbk = content.resource_group_name.Split("_").Select((s, idx) => idx == 1 ? "dbk" : s);
            var dbkRg = string.Join("-", dbk);
            return dbkRg;
        }

        /// <summary>
        /// Gets the resource group name from the json content of the blob storage resource
        /// </summary>
        /// <param name="jsonContent">Project_Resource.JsonContent of the blob storage resource of a workspace</param>
        /// <returns>The resource group name of the workspace</returns>
        internal string ParseResourceGroup(string jsonContent)
        {
            var content = JsonSerializer.Deserialize<RgNameObject>(jsonContent);
            return content?.resource_group_name;
        }

        /// <summary>
        /// Parse a list of QueryResults into a list of DailyServiceCosts
        /// </summary>
        /// <param name="queryResults">List of the query results obtained from a usage query using the ARM SDK</param>
        /// <returns>A List of DailyServiceCosts</returns>
        internal List<DailyServiceCost> ParseQueryResult(List<QueryResult> queryResults)
        {
            var lstDailyCosts = new HashSet<DailyServiceCost>();

            queryResults.ForEach(queryResult =>
            {
                var cols = queryResult.Columns.ToList().Select(c => c.Name).ToList();
                CultureInfo provider = CultureInfo.InvariantCulture;

                queryResult.Rows.ToList().ForEach(r =>
                {
                    lstDailyCosts.Add(new DailyServiceCost()
                    {
                        Amount = decimal.Parse(r[cols.FindIndex(c => c == COST_COLUMN)].ToString()),
                        Source = r[cols.FindIndex(c => c == SERVICE_NAME_COLUMN)].ToString(),
                        Date = DateTime.ParseExact(r[cols.FindIndex(c => c == USAGE_DATE_COLUMN)].ToString(),
                            "yyyyMMdd",
                            provider),
                        ResourceGroupName = r[cols.FindIndex(c => c == RESOURCE_GROUP_NAME_COLUMN)].ToString()
                    });
                });
            });
            return lstDailyCosts.ToList();
        }

        /// <summary>
        /// Parse a list of Project_Costs into a list of DailyServiceCosts
        /// </summary>
        /// <param name="costs">A list of Project_Costs</param>
        /// <returns>A list of DailyServiceCosts</returns>
        internal List<DailyServiceCost> ParseProjectCosts(List<Datahub_Project_Costs> costs)
        {
            var dscs = new List<DailyServiceCost>();
            costs.ForEach(
                c =>
                {
                    var dsc = new DailyServiceCost()
                    {
                        Amount = (decimal)c.CadCost,
                        Source = c.ServiceName,
                        Date = c.Date,
                        ResourceGroupName = ""
                    };
                    dscs.Add(dsc);
                });
            return dscs;
        }

        /// <summary>
        /// Gets the most recent date from a QueryResult.
        /// </summary>
        /// <param name="queryResult">A query result from a usage query</param>
        /// <returns>The datetime of the most recent date present in the query result</returns>
        internal DateTime GetLastDate(QueryResult queryResult)
        {
            var cols = queryResult.Columns.ToList().Select(c => c.Name).ToList();
            CultureInfo provider = CultureInfo.InvariantCulture;
            var max = queryResult.Rows.MaxBy(r => DateTime.ParseExact(
                r[cols.FindIndex(c => c == USAGE_DATE_COLUMN)].ToString(),
                "yyyyMMdd",
                provider));
            var maxDate = DateTime.ParseExact(max[cols.FindIndex(c => c == USAGE_DATE_COLUMN)].ToString(), "yyyyMMdd",
                provider);
            return maxDate;
        }

        internal record RgNameObject(string resource_group_name);
    }
}