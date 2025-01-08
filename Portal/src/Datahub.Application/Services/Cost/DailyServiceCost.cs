namespace Datahub.Application.Services.Cost
{
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