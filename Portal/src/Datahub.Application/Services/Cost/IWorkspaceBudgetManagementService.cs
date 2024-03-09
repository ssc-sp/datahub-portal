
namespace Datahub.Application.Services.Budget
{
    public interface IWorkspaceBudgetManagementService
    {
        public Task<decimal> GetBudgetAmountAsync(string budgetId);
        public Task SetBudgetAmountAsync(string budgetId, decimal amount);
        public Task<decimal> GetBudgetSpentAsync(string budgetId);
    }
}