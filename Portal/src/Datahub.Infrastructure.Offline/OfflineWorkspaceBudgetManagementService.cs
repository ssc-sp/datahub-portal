using Datahub.Application.Services.Budget;

namespace Datahub.Infrastructure.Offline
{
    public class OfflineWorkspaceBudgetManagementService : IWorkspaceBudgetManagementService
    {
        public Task<decimal> GetWorkspaceBudgetAmountAsync(string workspaceAcronym)
        {
            throw new NotImplementedException();
        }

        public Task SetWorkspaceBudgetAmountAsync(string workspaceAcronym, decimal amount, bool rollover = false)
        {
            throw new NotImplementedException();
        }

        public Task<decimal> GetWorkspaceBudgetSpentAsync(string workspaceAcronym, string? budgetId = null)
        {
            throw new NotImplementedException();
        }

        public Task<decimal> UpdateWorkspaceBudgetSpentAsync(string workspaceAcronym, string? budgetId = null)
        {
            throw new NotImplementedException();
        }
    }
}