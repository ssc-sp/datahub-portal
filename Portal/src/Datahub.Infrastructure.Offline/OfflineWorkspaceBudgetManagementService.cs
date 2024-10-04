using Datahub.Application.Services.Cost;

namespace Datahub.Infrastructure.Offline
{
    public class OfflineWorkspaceBudgetManagementService : IWorkspaceBudgetManagementService
    {
        public Task<decimal> GetWorkspaceBudgetAmountAsync(string workspaceAcronym, List<string>? budgetIds = null)
        {
            throw new NotImplementedException();
        }

        public Task SetWorkspaceBudgetAmountAsync(string workspaceAcronym, decimal amount, bool rollover = false,
            List<string>? budgetIds = null)
        {
            throw new NotImplementedException();
        }

        public Task<decimal> GetWorkspaceBudgetSpentAsync(string workspaceAcronym, List<string>? budgetIds = null)
        {
            throw new NotImplementedException();
        }

        public Task<decimal> UpdateWorkspaceBudgetSpentAsync(string workspaceAcronym, List<string>? budgetIds = null)
        {
            throw new NotImplementedException();
        }
    }
}