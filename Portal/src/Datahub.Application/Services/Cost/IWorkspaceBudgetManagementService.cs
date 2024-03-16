
namespace Datahub.Application.Services.Budget
{
    public interface IWorkspaceBudgetManagementService
    {
        public Task<decimal> GetBudgetAmountAsync(string budgetId);
        public Task<decimal> GetWorkspaceBudgetAmountAsync(string workspaceAcronym);
        public Task SetBudgetAmountAsync(string budgetId, decimal amount);
        public Task SetWorkspaceBudgetAmountAsync(string workspaceAcronym, decimal amount);
        public Task<decimal> GetBudgetSpentAsync(string budgetId);
        public Task<decimal> GetWorkspaceBudgetSpentAsync(string workspaceAcronym);
        public Task<(bool, decimal)> UpdateWorkspaceBudgetSpentAsync(string workspaceAcronym);
    }
}