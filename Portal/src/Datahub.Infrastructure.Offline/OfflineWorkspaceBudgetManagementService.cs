using Datahub.Application.Services.Budget;

namespace Datahub.Infrastructure.Offline
{
    public class OfflineWorkspaceBudgetManagementService : IWorkspaceBudgetManagementService
    {
        public Task CreateArmClient(string TenantId, string InfraClientId, string InfraClientSecret)
        {
            throw new NotImplementedException();
        }

        public Task<decimal> GetBudgetAmountAsync(string budgetId)
        {
            throw new NotImplementedException();
        }

        public Task SetBudgetAmountAsync(string budgetId, decimal amount)
        {
            throw new NotImplementedException();
        }

        public Task<decimal> GetBudgetSpentAsync(string budgetId)
        {
            throw new NotImplementedException();
        }
    }
}