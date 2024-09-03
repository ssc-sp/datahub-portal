using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.CostManagement;
using Azure.ResourceManager.CostManagement.Models;
using Datahub.Application.Services.Cost;
using Datahub.Application.Services.ResourceGroups;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("Datahub.Infrastructure.UnitTests")]

namespace Datahub.Infrastructure.Services.Cost
{
    public class WorkspaceCostManagementService(
        ArmClient armClient,
        ILogger<WorkspaceCostManagementService> logger,
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
        IWorkspaceResourceGroupsManagementService rgMgmtService)
        : IWorkspaceCostManagementService
    {
        private const string COST_COLUMN = "Cost";
        private const string SERVICE_NAME_COLUMN = "ServiceName";
        private const string USAGE_DATE_COLUMN = "UsageDate";
        private const string RESOURCE_GROUP_NAME_COLUMN = "ResourceGroupName";
        private const string AZURE_IDENTIFIER = "azure";
        private const decimal DIFFERENCE_THRESHOLD = (decimal)0.1;
        private const decimal REFRESH_THRESHOLD = 5;
        private static readonly TimeSpan UPDATE_THRESHOLD = TimeSpan.FromHours(2);

        #region Implementations

        /// <inheritdoc />
        public async Task<(bool, decimal)> UpdateWorkspaceCostsAsync(string workspaceAcronym,
            List<DailyServiceCost> azureCosts)
        {
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = await ctx.Projects.FirstAsync(p => p.Project_Acronym_CD == workspaceAcronym);
            var rgNames = await rgMgmtService.GetWorkspaceResourceGroupsAsync(workspaceAcronym);
            // Get the costs for the workspace both from the query and the database
            // The costs from the query are the costs from the last several days
            // The costs from the database are all costs existing for a workspace
            var azureWorkspaceCosts = azureCosts.FilterResourceGroups(rgNames);

            if (azureWorkspaceCosts.Count == 0)
            {
                logger.LogWarning(
                    "Could not find costs for project with acronym {WorkspaceAcronym}. " +
                    "This could be due to the workspace not having any associated cloud infrastructure, or that the costs " +
                    "are too small to be considered significant",
                    workspaceAcronym);
            }

            logger.LogInformation("Found {Count} costs for project with acronym {WorkspaceAcronym}",
                azureWorkspaceCosts.Count, workspaceAcronym);

            // Backfill the costs in the database for the days that are not present. This will exclude today
            logger.LogInformation("Backfilling costs for project with acronym {WorkspaceAcronym}", workspaceAcronym);
            await BackFillWorkspaceCosts(project.Project_Acronym_CD, azureWorkspaceCosts);
            logger.LogInformation("Backfilled costs for project with acronym {WorkspaceAcronym}", workspaceAcronym);

            // Update the Project_Credits entry for the workspace
            logger.LogInformation("Updating credits for project with acronym {WorkspaceAcronym}", workspaceAcronym);
            var (rolloverNeeded, workspaceLastFYTotal) =
                await UpdateWorkspaceCredits(project.Project_Acronym_CD, azureWorkspaceCosts);
            logger.LogInformation("Updated credits for project with acronym {WorkspaceAcronym}", workspaceAcronym);

            // Return whether a rollover is needed and the total costs incurred in the last fiscal year (the last full FY)
            return (rolloverNeeded, workspaceLastFYTotal);
        }

        /// <inheritdoc />
        public async Task<bool> RefreshWorkspaceCostsAsync(string workspaceAcronym)
        {
            var currentFiscalYear = CostManagementUtilities.CurrentFiscalYear;
            var costs = await QueryWorkspaceCostsAsync(workspaceAcronym, currentFiscalYear.StartDate,
                DateTime.UtcNow.Date, QueryGranularity.Daily);

            try
            {
                await UpdateWorkspaceCostsAsync(workspaceAcronym, costs);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> VerifyAndRefreshWorkspaceCostsAsync(string workspaceAcronym,
            List<DailyServiceCost> azureTotals, bool executeRefresh = true)
        {
            var workspaceRgs = await rgMgmtService.GetWorkspaceResourceGroupsAsync(workspaceAcronym);
            var workspaceCosts = azureTotals.FilterResourceGroups(workspaceRgs);
            var workspaceAzureTotal = workspaceCosts.TotalAmount();

            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var projectCredits = await ctx.Project_Credits
                .Include(c => c.Project)
                .FirstOrDefaultAsync(c => c.Project.Project_Acronym_CD == workspaceAcronym);
            if (projectCredits is null) return false;
            var workspaceDbTotal = (decimal)projectCredits.Current;
            var diff = Math.Abs(workspaceAzureTotal - workspaceDbTotal);

            if (diff > REFRESH_THRESHOLD)
            {
                logger.LogWarning("Workspace costs for {WorkspaceAcronym} do not match Azure costs (diff = ${Diff} > ${Threshold}). " +
                                  "Refreshing costs for workspace", workspaceAcronym, diff, REFRESH_THRESHOLD);
                if (executeRefresh) return await RefreshWorkspaceCostsAsync(workspaceAcronym);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public bool CheckUpdateNeeded(string workspaceAcronym)
        {
            using var ctx = dbContextFactory.CreateDbContext();
            var credits = ctx.Project_Credits
                .Include(c => c.Project)
                .FirstOrDefault(c => c.Project.Project_Acronym_CD == workspaceAcronym);
            if (credits is null) return true;
            if (credits.LastUpdate is null) return true;
            if (DateTime.UtcNow - credits.LastUpdate > UPDATE_THRESHOLD) return true;
            return false;
        }

        /// <inheritdoc />
        public async Task<List<DailyServiceCost>> QuerySubscriptionCostsAsync(string subscriptionId,
            DateTime startDate,
            DateTime endDate, QueryGranularity granularity, List<string>? rgNames = default)
        {
            ValidateDateRange(startDate, endDate);
            if (!subscriptionId.Contains("/"))
            {
                subscriptionId = $"/subscriptions/{subscriptionId}";
            }
            var queryResult = await QueryScopeCostsAsync(subscriptionId, startDate, endDate, granularity, rgNames);
            return queryResult;
        }

        /// <inheritdoc />
        public async Task<List<DailyServiceCost>> QueryScopeCostsAsync(string scopeId, DateTime startDate,
            DateTime endDate, QueryGranularity granularity, List<string>? rgNames = default)
        {
            ValidateDateRange(startDate, endDate);
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var scope = new ResourceIdentifier(scopeId);

            if (scope is null)
            {
                logger.LogError("Could not find scope with id {ScopeId}", scopeId);
                throw new Exception($"Could not find scope with id {scopeId}");
            }

            if (rgNames is null)
            {
                switch (scope.ResourceType.Type)
                {
                    case "subscriptions":
                        logger.LogInformation("Getting all subscription workspace resource groups for query filtering");
                        rgNames = await rgMgmtService.GetAllSubscriptionResourceGroupsAsync(scope.SubscriptionId!);
                        break;
                    case "resourceGroups":
                        logger.LogInformation("Using provided resource group for query filtering");
                        rgNames = new List<string> { scope.ResourceGroupName! };
                        break;
                    default:
                        logger.LogInformation("Getting all workspace resource groups for query filtering");
                        rgNames = await rgMgmtService.GetAllResourceGroupsAsync();
                        break;
                }
            }

            var queryResults = new List<QueryResult>();
            string nextLink;
            var lastDate = startDate;
            do
            {
                var query = BuildQueryDefinition(rgNames, lastDate, endDate, granularity);
                var response = await armClient.UsageQueryAsync(scope, query);

                if (!response.HasValue)
                {
                    throw new Exception($"Could not get cost data for scope {scopeId}");
                }

                var result = response.Value;
                queryResults.Add(result);
                lastDate = granularity == QueryGranularity.Daily ? GetLastDate(result) : endDate;
                nextLink = result.NextLink;
            } while (!string.IsNullOrEmpty(nextLink));

            return ParseQueryResult(queryResults);
        }

        /// <inheritdoc />
        public async Task<List<DailyServiceCost>> QueryWorkspaceCostsAsync(string workspaceAcronym, DateTime startDate,
            DateTime endDate, QueryGranularity granularity)
        {
            ValidateDateRange(startDate, endDate);
            var rgIds = await rgMgmtService.GetWorkspaceResourceGroupsIdentifiersAsync(workspaceAcronym);

            var workspaceCosts = new List<DailyServiceCost>();
            foreach (var rgId in rgIds)
            {
                var rgCosts = await QueryScopeCostsAsync(rgId.ToString(), startDate, DateTime.UtcNow,
                    granularity);

                workspaceCosts.AddRange(rgCosts);
            }

            return workspaceCosts;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Backfills the costs for a workspace in the database with the given Azure costs. This will add new costs
        /// if they do not exist, update costs if they are different enough from the Azure data, and skip if the costs
        /// are the same.
        /// </summary>
        /// <param name="projectAcronym">The project acronym to backfill for</param>
        /// <param name="azureWorkspaceCosts">The Azure workspace costs</param>
        internal async Task BackFillWorkspaceCosts(string projectAcronym, List<DailyServiceCost> azureWorkspaceCosts)
        {
            var dates = azureWorkspaceCosts.DistinctDates();
            dates = dates.Where(d => !d.Date.Equals(DateTime.UtcNow.Date))
                .ToList();
            logger.LogInformation("Days to check: {Join}", string.Join(", ", dates));

            var (entriesAdded, entriesUpdated, entriesSkipped) = (0, 0, 0);
            foreach (var date in dates)
            {
                var costsForDate = azureWorkspaceCosts.FilterDateRange(date);
                var (added, updated, skipped) = await AddOrUpdateDailyCosts(projectAcronym, costsForDate, date);
                entriesAdded += added;
                entriesUpdated += updated;
                entriesSkipped += skipped;
            }

            logger.LogInformation(
                "SUMMARY: Added {EntriesAdded}, updated {EntriesUpdated} and verified/skipped {EntriesSkipped} cost records for workspace with acronym {WorkspaceAcronym}",
                entriesAdded, entriesUpdated, entriesSkipped, projectAcronym);
            if (entriesUpdated > 0)
            {
                logger.LogWarning(
                    "Some costs were updated. This implies past records were incorrect and have been updated with new Azure data. Please verify that the costs are correct in the database for workspace with acronym {WorkspaceAcronym}",
                    projectAcronym);
            }
        }

        /// <summary>
        /// This method adds or updates the daily costs for a project in the database given a specific date.
        /// It will add a new record if the service does not exist for the given date, update the record if the cost is
        /// different enough from the Azure data, and skip if the cost is the same.
        /// </summary>
        /// <param name="projectAcronym">The project acronym of the project to work on</param>
        /// <param name="azureWorkspaceDayCosts">The Azure workspace costs for the day given</param>
        /// <param name="date">The date to add or update costs for</param>
        /// <returns>A tuple of (entries added, entries updated, entries skipped).</returns>
        internal async Task<(int, int, int)> AddOrUpdateDailyCosts(string projectAcronym,
            List<DailyServiceCost> azureWorkspaceDayCosts, DateTime date)
        {
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            logger.LogInformation("Checking for missing costs for date {Date}", date);

            var project = await ctx.Projects.AsNoTracking().FirstAsync(p => p.Project_Acronym_CD == projectAcronym);

            var databaseCosts = ctx.Project_Costs
                .Where(c => c.Project_ID == project.Project_ID && c.Date == date.Date)
                .ToList();

            var services = azureWorkspaceDayCosts.DistinctSources();
            var (entriesAdded, entriesUpdated, entriesSkipped) = (0, 0, 0);

            // For each service that we have Azure data of, we check if it is in the database and if records match. If not, we add/update.
            services.ForEach(s =>
            {
                var existing = databaseCosts.FirstOrDefault(c => c.ServiceName == s);
                if (existing is null)
                {
                    var amount = (double)azureWorkspaceDayCosts.FilterSource(s).TotalAmount();
                    // Adding
                    var cost = new Datahub_Project_Costs()
                    {
                        CadCost = amount,
                        CloudProvider = AZURE_IDENTIFIER,
                        Date = date,
                        Project_ID = project.Project_ID,
                        ServiceName = s
                    };
                    ctx.Project_Costs.Add(cost);
                    entriesAdded++;
                    logger.LogInformation(
                        "Added cost for service {ServiceName} on date {Date} for an amount of ${Amount}", s,
                        date.ToString("d"),
                        amount);
                }
                else
                {
                    // Verifying/Updating
                    var diff = Math.Abs((decimal)existing.CadCost -
                                        azureWorkspaceDayCosts.FilterSource(s).TotalAmount());
                    if (diff > DIFFERENCE_THRESHOLD)
                    {
                        logger.LogWarning(
                            "Existing cost for service {ServiceName} on date {Date} in {Acronym} was verified to be" +
                            " incorrect in database (diff = {Diff} > {Threshold}). Updating with most up to date Azure data",
                            s, date, project.Project_Acronym_CD, diff, DIFFERENCE_THRESHOLD);
                        existing.CadCost = (double)azureWorkspaceDayCosts.FilterSource(s).TotalAmount();
                        ctx.Project_Costs.Update(existing);
                        entriesUpdated++;
                        logger.LogInformation("Updated cost for service {ServiceName} on date {Date}", s, date);
                    }
                    else
                    {
                        entriesSkipped++;
                    }
                }
            });
            await ctx.SaveChangesAsync();
            return (entriesAdded, entriesUpdated, entriesSkipped);
        }

        /// <summary>
        /// Updates the Project_Credits entry for the given workspace acronym with the given Azure costs.
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym to update the credits for</param>
        /// <param name="azureWorkspaceCosts">The Azure costs to use. These costs MUST contain records for today</param>
        /// <returns>A tuple of (is a rollover needed, how much would the rollover be for if it was needed)</returns>
        internal async Task<(bool, decimal)> UpdateWorkspaceCredits(string workspaceAcronym,
            List<DailyServiceCost> azureWorkspaceCosts)
        {
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = await ctx.Projects.AsNoTracking().FirstAsync(p => p.Project_Acronym_CD == workspaceAcronym);

            // We grab certain filterings and groupings of the costs for the workspace
            var queryWorkspaceTodayCosts = azureWorkspaceCosts.FilterDateRange(DateTime.UtcNow.Date);
            var dbWorkspaceCosts = await GetWorkspaceCosts(workspaceAcronym);
            var dbWorkspaceCurrentFYCosts = dbWorkspaceCosts.FilterCurrentFiscalYear();
            var workspaceCurrentFYCosts = dbWorkspaceCurrentFYCosts.Concat(queryWorkspaceTodayCosts).ToList();
            var workspaceYesterdayCosts = dbWorkspaceCosts.FilterDateRange(DateTime.UtcNow.Date.AddDays(-1));
            var workspaceLastFYCosts = dbWorkspaceCosts.FilterLastFiscalYear();

            // Create the Project_Credits entry if it does not exist (in the case of a new workspace)
            logger.LogInformation("Updating credits for project with acronym {WorkspaceAcronym}", workspaceAcronym);
            var projectCredits = await ctx.Project_Credits.FirstOrDefaultAsync(c => c.ProjectId == project.Project_ID);
            if (projectCredits is null)
            {
                logger.LogInformation("Creating new Project_Credits entry");
                projectCredits = new Project_Credits()
                {
                    ProjectId = project.Project_ID
                };
                ctx.Project_Credits.Add(projectCredits);
                await ctx.SaveChangesAsync();
            }

            // Get the last update time to check if a rollover is needed and the total costs incurred in the last fiscal year
            var beforeUpdate = projectCredits.LastUpdate;
            var lastRollover = projectCredits.LastRollover;
            var workspaceLastFYTotal = workspaceLastFYCosts.TotalAmount();

            // Update the Project_Credits entry
            projectCredits.Current = (double)workspaceCurrentFYCosts.TotalAmount();
            projectCredits.YesterdayCredits = (double)workspaceYesterdayCosts.TotalAmount();
            projectCredits.CurrentPerDay = JsonSerializer.Serialize(workspaceCurrentFYCosts.GroupByDate());
            projectCredits.CurrentPerService = JsonSerializer.Serialize(workspaceCurrentFYCosts.GroupBySource());
            projectCredits.YesterdayPerService = JsonSerializer.Serialize(workspaceYesterdayCosts.GroupBySource());
            projectCredits.LastUpdate = DateTime.UtcNow;
            ctx.Project_Credits.Update(projectCredits);

            await ctx.SaveChangesAsync();
            return (RolloverNeeded(beforeUpdate, lastRollover), workspaceLastFYTotal);
        }

        /// <summary>
        /// Gets the costs for the given workspace acronym from the Project_Costs table
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym</param>
        /// <returns>A list of DailyServiceCosts</returns>
        /// <exception cref="Exception">Exceptions are thrown whenever the project is not found or it has no costs</exception>
        internal async Task<List<DailyServiceCost>> GetWorkspaceCosts(string workspaceAcronym)
        {
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = await ctx.Projects.FirstAsync(p => p.Project_Acronym_CD == workspaceAcronym);

            var projectCosts = ctx.Project_Costs.Where(c => c.Project_ID == project.Project_ID).ToList();

            var workspaceCosts = ParseProjectCosts(projectCosts);
            return workspaceCosts;
        }

        /// <summary>
        /// Builds the query definition for the azure cost management query. If it can determine the current environment
        /// it is in, it will filter the query to only include that environment. Otherwise it will include all environments.
        /// </summary>
        /// <param name="resourceGroups">The list of resource groups to filter the query for</param>
        /// <param name="startDate">The start date of the query</param>
        /// <param name="endDate">The end date of the query</param>
        /// <param name="granularity">The granularity of the query</param>
        /// <returns></returns>
        internal QueryDefinition BuildQueryDefinition(List<string> resourceGroups, DateTime startDate, DateTime endDate,
            QueryGranularity granularity)
        {
            switch (granularity)
            {
                case QueryGranularity.Daily:
                    return BuildGranularQueryDefinition(resourceGroups, startDate, endDate);
                case QueryGranularity.Total:
                    return BuildTotalsQueryDefinition(resourceGroups, startDate, endDate);
                default:
                    return BuildTotalsQueryDefinition(resourceGroups, startDate, endDate);
            }
        }

        /// <summary>
        /// Creates the query definition for the azure cost management query. If it can determine the current environment
        /// it is in, it will filter the query to only include that environment. Otherwise it will include all environments.
        /// </summary>
        /// <param name="resourceGroups">A list of resourceGroups to filter the query to</param>
        /// <param name="startDate">Start date of the query</param>
        /// <param name="endDate">End date of the query</param>
        /// <returns>The QueryDefinition object necessary for the usage query</returns>
        internal QueryDefinition BuildGranularQueryDefinition(List<string> resourceGroups, DateTime startDate,
            DateTime endDate)
        {
            var dataset = new QueryDataset
            {
                Filter = new QueryFilter
                {
                    Dimensions = new QueryComparisonExpression(RESOURCE_GROUP_NAME_COLUMN, QueryOperatorType.In,
                        resourceGroups)
                },
                Granularity = GranularityType.Daily,
                Grouping =
                {
                    new QueryGrouping(QueryColumnType.Dimension, SERVICE_NAME_COLUMN),
                    new QueryGrouping(QueryColumnType.Dimension, RESOURCE_GROUP_NAME_COLUMN)
                },
                Aggregation =
                {
                    { COST_COLUMN, new QueryAggregation(COST_COLUMN, FunctionType.Sum) }
                }
            };

            var query = new QueryDefinition(ExportType.ActualCost, TimeframeType.Custom, dataset)
            {
                TimePeriod = new QueryTimePeriod(startDate, endDate)
            };

            return query;
        }

        /// <summary>
        /// Creates the query definition for the azure cost management query. If it can determine the current environment
        /// it is in, it will filter the query to only include that environment. Otherwise it will include all environments.
        /// This query is for the total costs only per resource group.
        /// </summary>
        /// <param name="resourceGroups">A list of all resource groups to filter the query to</param>
        /// <param name="startDate">Start date of the query</param>
        /// <param name="endDate">End date of the query</param>
        /// <returns>The QueryDefinition object necessary for the usage query</returns>
        internal QueryDefinition BuildTotalsQueryDefinition(List<string> resourceGroups, DateTime startDate,
            DateTime endDate)
        {
            var dataset = new QueryDataset
            {
                Filter = new QueryFilter
                {
                    Dimensions = new QueryComparisonExpression(RESOURCE_GROUP_NAME_COLUMN, QueryOperatorType.In,
                        resourceGroups)
                },
                Grouping =
                {
                    new QueryGrouping(QueryColumnType.Dimension, RESOURCE_GROUP_NAME_COLUMN)
                },
                Aggregation =
                {
                    { COST_COLUMN, new QueryAggregation(COST_COLUMN, FunctionType.Sum) }
                }
            };

            var query = new QueryDefinition(ExportType.ActualCost, TimeframeType.Custom, dataset)
            {
                TimePeriod = new QueryTimePeriod(startDate, endDate)
            };

            return query;
        }

        /// <summary>
        /// Verifies if the last update and last rollover were outside of the current fiscal year.
        /// </summary>
        /// <param name="lastUpdate">The datetime of the last update</param>
        /// <param name="lastRollover">The datetime of the last rollover</param>
        /// <returns>true if the last update is outside the current fiscal year, false otherwise</returns>
        internal bool RolloverNeeded(DateTime? lastUpdate, DateTime? lastRollover)
        {
            var (currFiscalYearStart, _) = CostManagementUtilities.CurrentFiscalYear;
            return lastUpdate < currFiscalYearStart && lastRollover is null || lastRollover < currFiscalYearStart;
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
                var cols = queryResult.Columns.Select(c => c.Name).ToList();

                var costColumn = cols.FindIndex(c => c == COST_COLUMN);
                var serviceColumn = cols.FindIndex(c => c == SERVICE_NAME_COLUMN);
                var dateColumn = cols.FindIndex(c => c == USAGE_DATE_COLUMN);
                var rgColumn = cols.FindIndex(c => c == RESOURCE_GROUP_NAME_COLUMN);

                CultureInfo provider = CultureInfo.InvariantCulture;

                queryResult.Rows.ToList().ForEach(r =>
                {
                    lstDailyCosts.Add(new DailyServiceCost
                    {
                        Amount = costColumn < 0 ? 0 : decimal.Parse(r[costColumn].ToString(), CultureInfo.InvariantCulture),
                        Source = serviceColumn < 0
                            ? String.Empty
                            : r[serviceColumn].ToString().Replace("\"", ""),
                        Date = dateColumn < 0
                            ? DateTime.MinValue
                            : DateTime.ParseExact(r[dateColumn].ToString(),
                                "yyyyMMdd",
                                provider),
                        ResourceGroupName = rgColumn < 0
                            ? String.Empty
                            : r[rgColumn].ToString()
                                .Replace("\"", "")
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
            var maxDate = DateTime.ParseExact(max![cols.FindIndex(c => c == USAGE_DATE_COLUMN)].ToString(), "yyyyMMdd",
                provider);
            return maxDate;
        }

        /// <summary>
        /// Small helper to validate date ranges. Throws an exception if the start date is after the end date or if the
        /// date range is more than a year.
        /// </summary>
        /// <param name="startDate">The start date of the range</param>
        /// <param name="endDate">The end date of the range</param>
        /// <exception cref="Exception">Will throw if the start is after the end or if the range is larger than a year</exception>
        internal void ValidateDateRange(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                logger.LogError("Start date is after end date");
                throw new Exception("Start date is after end date");
            }

            if (endDate - startDate > TimeSpan.FromDays(365))
            {
                logger.LogError("Querying more than a year of data is not allowed");
                throw new Exception("Querying more than a year of data is not allowed.");
            }
        }

        #endregion
    }
}