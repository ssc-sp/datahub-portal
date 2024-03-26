namespace Datahub.Application.Services.Budget
{
    public interface IWorkspaceCostManagementService
    {
        /// <summary>
        /// Updates the Project_Costs and Project_Credits for the given workspace acronym.
        /// </summary>
        /// <param name="subCosts">The costs at the subscription level</param>
        /// <param name="workspaceAcronym">The workspace acronym</param>
        /// <returns>(bool, decimal), a tuple representing whether a rollover is needed according to this update and the amount of costs captured in the last fiscal year</returns>
        public Task<(bool, decimal)> UpdateWorkspaceCostAsync(List<DailyServiceCost> subCosts, string workspaceAcronym);
        
        /// <summary>
        /// Queries the costs for the given subscription id within the given date range.
        /// </summary>
        /// <param name="subId">Subscription id, i.e. "/subscription/<...>"</param>
        /// <param name="startDate">The start date of the query</param>
        /// <param name="endDate">The end date of the query</param>
        /// <returns>A List containing all daily service costs. A daily service cost is a cost caused by one service during one day.</returns>
        public Task<List<DailyServiceCost>?> QuerySubscriptionCosts(DateTime startDate,
            DateTime endDate);
        
        /// <summary>
        /// Groups the costs given by source. By executing this, you lose date information
        /// </summary>
        /// <param name="costs">The costs to group</param>
        /// <returns>The grouped costs</returns>
        public List<DailyServiceCost> GroupBySource(List<DailyServiceCost> costs);
        
        /// <summary>
        /// Groups the costs given by date. By executing this, you lose source information
        /// </summary>
        /// <param name="costs">The costs to group</param>
        /// <returns>The grouped costs</returns>
        public List<DailyServiceCost> GroupByDate(List<DailyServiceCost> costs);
        
        /// <summary>
        /// Filters the given costs to be only within the current fiscal year
        /// </summary>
        /// <param name="costs">The costs to filter</param>
        /// <returns>The filtered costs, which are all in the current fiscal year</returns>
        public List<DailyServiceCost> FilterCurrentFiscalYear(List<DailyServiceCost> costs);
        
        /// <summary>
        /// Filters the costs for the given workspace acronym from the given list of subscription level costs
        /// </summary>
        /// <param name="subCosts">Costs at the subscription level</param>
        /// <param name="workspaceAcronym">Workspace acronym</param>
        /// <returns>List of daily service costs for the workspace. A daily service cost is a cost caused by one service during one day.</returns>
        public Task<List<DailyServiceCost>> FilterWorkspaceCosts(List<DailyServiceCost> subCosts, string workspaceAcronym);
        
        /// <summary>
        /// Filters the given costs to be only within the last fiscal year
        /// </summary>
        /// <param name="costs">The costs to filter</param>
        /// <returns>The filtered costs, which are all in the last fiscal year</returns>
        public List<DailyServiceCost> FilterLastFiscalYear(List<DailyServiceCost> costs);
        
        /// <summary>
        /// Filters the given costs to be only within a date range
        /// </summary>
        /// <param name="costs">The costs to filter</param>
        /// <param name="startDate">The start of the date range</param>
        /// <param name="endDate">The end of the date range</param>
        /// <returns>The filtered costs, which should be between the dates provided, inclusively</returns>
        public List<DailyServiceCost> FilterDateRange(List<DailyServiceCost> costs, DateTime startDate,
            DateTime endDate);

        /// <summary>
        /// Filters the given costs to be only from a given date
        /// </summary>
        /// <param name="costs">The costs to filter</param>
        /// <param name="date">The date of interest</param>
        /// <returns>The filtered costs, which should be only from the given date</returns>
        public List<DailyServiceCost> FilterDateRange(List<DailyServiceCost> costs, DateTime date);
    }

    public struct DailyServiceCost
    {
        public decimal Amount { get; set; }
        public string Source { get; set; }
        public string ResourceGroupName { get; set; }
        public DateTime Date { get; set; }

        public bool Equals(DailyServiceCost other)
        {
            return Amount == other.Amount && Source == other.Source && ResourceGroupName == other.ResourceGroupName && Date.Equals(other.Date);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Amount, Source, ResourceGroupName, Date);
        }
    }
}