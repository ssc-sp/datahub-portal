using System.Runtime.CompilerServices;
using Datahub.Core.Model.Context;

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
        /// <param name="ctx">The project db context to use, to avoid having to create a context every time</param>
        /// <returns>True if it is needed, false otherwise</returns>
        public bool CheckUpdateNeeded(string workspaceAcronym, DatahubProjectDBContext ctx);

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

    
}