using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Datahub.SpecflowTests")]

namespace Datahub.Application.Services.Cost
{
    public interface IWorkspaceCostManagementService
    {
        #region Querying and Database Operations

        /// <summary>
        /// Updates the Project_Costs and Project_Credits for the given workspace acronym.
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym</param>
        /// <param name="azureCosts">The costs list to use to update the database. The process will filter these costs to only use relevant cost records</param>
        /// <returns>(bool, decimal), a tuple representing whether a rollover is needed according to this update and the amount of costs captured in the last fiscal year</returns>
        public Task<(bool, decimal)> UpdateWorkspaceCostsAsync(string workspaceAcronym,
            List<DailyServiceCost> azureCosts);

        /// <summary>
        /// Queries the costs for a singular workspace for the totality of the current fiscal year and updates the database.
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym</param>
        /// <returns>True if the refresh was successful, false otherwise</returns>
        public Task<bool> RefreshWorkspaceCostsAsync(string workspaceAcronym);

        /// <summary>
        /// Verifies the totals for the given workspace acronym compared to the totals given.
        /// If they are different, it will refresh the costs for the workspace.
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym to verify</param>
        /// <param name="azureTotals">The totals by resource groups, given by Azure</param>
        /// <param name="executeRefresh">Whether or not to execute the refresh if it is required</param>
        /// <returns>True if a refresh was done succesfully, false otherwise</returns>
        public Task<bool> VerifyAndRefreshWorkspaceCostsAsync(string workspaceAcronym,
            List<DailyServiceCost> azureTotals, bool executeRefresh = true);

        /// <summary>
        /// Checks if the costs for the given workspace acronym need to be updated.
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym to check for</param>
        /// <returns>True if it is needed, false otherwise</returns>
        public bool CheckUpdateNeeded(string workspaceAcronym);

        /// <summary>
        /// Queries the costs for the given subscription id within the given date range.
        /// </summary>
        /// <param name="subscriptionId">The subscription id to query costs at</param>
        /// <param name="startDate">The start date of the query</param>
        /// <param name="endDate">The end date of the query</param>
        /// <param name="granularity">The granularity of the query. Daily will do a very granular and detailed query and
        /// Total will only fetch totals per resource groups</param>
        /// <param name="rgNames">Optional list of resource group names to filter for. If not provided, will make
        /// the queries to find them</param>
        /// <returns>A List containing all daily service costs</returns>
        public Task<List<DailyServiceCost>> QuerySubscriptionCostsAsync(string subscriptionId, DateTime startDate,
            DateTime endDate, QueryGranularity granularity, List<string>? rgNames = default);

        /// <summary>
        /// Queries the given scopes for costs within the given date range. Daily granularity.
        /// </summary>
        /// <param name="scopeId">The id of the scope. e.g. /subscriptions/... </param>
        /// <param name="startDate">The start date</param>
        /// <param name="endDate">The end date</param>
        /// <param name="granularity">The granularity of the query. Daily will do a very granular and detailed query and
        /// Total will only fetch totals per resource groups</param>
        /// <param name="rgNames">Optional list of resource group names to filter for. If not provided, will make
        /// the queries to find them</param>
        /// <returns>A List containing all daily service costs or null if the query was throttled</returns>
        /// <exception cref="Exception">Throws exception if the query was incorrect or if it was throttled</exception>
        public Task<List<DailyServiceCost>> QueryScopeCostsAsync(string scopeId, DateTime startDate,
            DateTime endDate, QueryGranularity granularity, List<string>? rgNames = default);

        /// <summary>
        /// Queries the costs for the given workspace acronym within the given date range.
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym to filter for</param>
        /// <param name="startDate">The start date of the filter</param>
        /// <param name="endDate">The end date of the filter</param>
        /// <param name="granularity">The granularity of the query. Daily will do a very granular and detailed query and
        /// Total will only fetch totals per resource groups</param>
        /// <returns>A List containing all daily service costs or null if the query was throttled. A daily service cost is a cost caused by one service during one day.</returns>
        public Task<List<DailyServiceCost>> QueryWorkspaceCostsAsync(string workspaceAcronym, DateTime startDate,
            DateTime endDate, QueryGranularity granularity);

        #endregion
    }

    public enum QueryGranularity
    {
        Daily,
        Total
    }

    /// <summary>
    /// A daily service cost is a cost caused by one service during one day.
    /// </summary>
    public class DailyServiceCost
    {
        public decimal Amount { get; set; }
        public string Source { get; set; } = String.Empty;
        public string ResourceGroupName { get; set; } = String.Empty;
        public DateTime Date { get; set; }

        public override bool Equals(object? obj)
        {
            var other = obj as DailyServiceCost;
            return Amount.Equals(other!.Amount) && Source.Equals(other.Source) && ResourceGroupName.Equals(other.ResourceGroupName) && Date.Equals(other.Date);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Amount, Source, ResourceGroupName, Date);
        }
    }

    public static class CostManagementUtilities
    {
        public static (DateTime StartDate, DateTime EndDate) CurrentFiscalYear = (
            new DateTime(DateTime.UtcNow.Month < 4 ? DateTime.UtcNow.Year - 1 : DateTime.UtcNow.Year, 4, 1),
            new DateTime(DateTime.UtcNow.Month < 4 ? DateTime.UtcNow.Year : DateTime.UtcNow.Year + 1, 3, 31));

        public static (DateTime StartDate, DateTime EndDate) LastFiscalYear = (
            new DateTime(DateTime.UtcNow.Month < 4 ? DateTime.UtcNow.Year - 2 : DateTime.UtcNow.Year - 1, 4, 1),
            new DateTime(DateTime.UtcNow.Month < 4 ? DateTime.UtcNow.Year - 1 : DateTime.UtcNow.Year, 3, 31));

        #region Groupings

        /// <summary>
        /// Groups the costs given by source. By executing this, you lose date information
        /// </summary>
        /// <param name="costs">The costs to group</param>
        /// <returns>The grouped costs</returns>
        public static List<DailyServiceCost> GroupBySource(this List<DailyServiceCost> costs) => costs
            .GroupBy(c => c.Source)
            .Select(g => new DailyServiceCost
            {
                Amount = g.Sum(c => c.Amount),
                Source = g.Key,
                ResourceGroupName = g.First().ResourceGroupName
            }).ToList();

        /// <summary>
        /// Groups the costs given by date. By executing this, you lose source information
        /// </summary>
        /// <param name="costs">The costs to group</param>
        /// <returns>The grouped costs</returns>
        public static List<DailyServiceCost> GroupByDate(this List<DailyServiceCost> costs) => costs
            .GroupBy(c => c.Date)
            .Select(g => new DailyServiceCost
            {
                Amount = g.Sum(c => c.Amount),
                Date = g.Key,
                ResourceGroupName = g.First().ResourceGroupName
            }).ToList();

        #endregion

        #region Filters

        /// <summary>
        /// Filters the costs for the given workspace acronym from the given list of costs
        /// </summary>
        /// <param name="costs">Costs to filter from</param>
        /// <param name="rgNames">Resource group names to filter with</param>
        /// <returns>List of daily service costs for the workspace</returns>
        public static List<DailyServiceCost>
            FilterResourceGroups(this List<DailyServiceCost> costs, List<string> rgNames) =>
            costs.Where(c => rgNames.Contains(c.ResourceGroupName)).ToList();

        /// <summary>
        /// Filters the costs for the given workspace acronym from the given list of costs
        /// </summary>
        /// <param name="costs"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static List<DailyServiceCost> FilterSource(this List<DailyServiceCost> costs, string source) =>
            costs.Where(c => c.Source == source).ToList();

        /// <summary>
        /// Filters the given costs to be only within a date range
        /// </summary>
        /// <param name="costs">Costs to filter from</param>
        /// <param name="startDate">The start of the date range</param>
        /// <param name="endDate">The end of the date range</param>
        /// <returns>The filtered costs, which should be between the dates provided, inclusively</returns>
        public static List<DailyServiceCost> FilterDateRange(this List<DailyServiceCost> costs, DateTime startDate,
            DateTime endDate) => costs.Where(c => c.Date >= startDate && c.Date <= endDate).ToList();

        /// <summary>
        /// Filters the given costs to be only from a given date
        /// </summary>
        /// <param name="costs">Costs to filter from</param>
        /// <param name="date">The date of interest</param>
        /// <returns>The filtered costs, which should be only from the given date</returns>
        public static List<DailyServiceCost> FilterDateRange(this List<DailyServiceCost> costs, DateTime date) =>
            costs.Where(c => c.Date == date).ToList();

        /// <summary>
        /// Filters the given costs to be only within the current fiscal year
        /// </summary>
        /// <param name="costs">Costs to filter from</param>
        /// <returns>The filtered costs, which are all in the current fiscal year</returns>
        public static List<DailyServiceCost> FilterCurrentFiscalYear(this List<DailyServiceCost> costs) =>
            FilterDateRange(costs, CurrentFiscalYear.StartDate, CurrentFiscalYear.EndDate);

        /// <summary>
        /// Filters the given costs to be only within the last fiscal year
        /// </summary>
        /// <param name="costs">Costs to filter from</param>
        /// <returns>The filtered costs, which are all in the last fiscal year</returns>
        public static List<DailyServiceCost> FilterLastFiscalYear(this List<DailyServiceCost> costs) =>
            FilterDateRange(costs, LastFiscalYear.StartDate, LastFiscalYear.EndDate);

        #endregion

        #region Utils

        public static List<DateTime> DistinctDates(this List<DailyServiceCost> costs) =>
            costs.Select(c => c.Date).Distinct().ToList();

        public static List<string> DistinctSources(this List<DailyServiceCost> costs) =>
            costs.Select(c => c.Source).Distinct().ToList();

        public static List<string> DistinctResourceGroups(this List<DailyServiceCost> costs) =>
            costs.Select(c => c.ResourceGroupName).Distinct().ToList();

        public static decimal TotalAmount(this List<DailyServiceCost> costs) => costs.Sum(c => c.Amount);

        #endregion
    }
}