
namespace Datahub.Application.Services.Budget
{
    public interface IWorkspaceBudgetManagementService
    {
        /// <summary>
        /// Gets the budget amount for the given workspace acronym.
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym</param>
        /// <returns>The total budget amount</returns>
        public Task<decimal> GetWorkspaceBudgetAmountAsync(string workspaceAcronym);

        /// <summary>
        /// Sets the budget amount for the given workspace acronym.
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym</param>
        /// <param name="amount">The total budget amount to set</param>
        /// <param name="rollover">If the operation is part of a rollover, will update records</param>
        /// <param name="budgetId">Optional budget ID to provide. If not provided, will interpolate budget Id for workspace</param>
        /// <returns></returns>
        public Task SetWorkspaceBudgetAmountAsync(string workspaceAcronym, decimal amount, bool rollover = false, string? budgetId = null);

        /// <summary>
        /// Get the amount of budget spent for the given workspace acronym.
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym</param>
        /// <param name="budgetId">Optional budget id to use. If not provided, will interpolate.</param>
        /// <returns>The amount of budget spent</returns>
        public Task<decimal> GetWorkspaceBudgetSpentAsync(string workspaceAcronym, string? budgetId = null);

        /// <summary>
        /// Updates the amount of budget spent saved in the Project_Credits table for a given workspace acronym.
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym</param>
        /// <param name="budgetId">Optional budget id to use. If not provided, will interpolate.</param>
        /// <returns>A tuple of whether or not the budget spent has decreased (indicating a budget reset), and the budget spent before the reset</returns>
        public Task<decimal> UpdateWorkspaceBudgetSpentAsync(string workspaceAcronym, string? budgetId = null);
    }
}