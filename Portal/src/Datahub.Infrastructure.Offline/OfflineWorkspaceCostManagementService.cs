using Datahub.Application.Services.Cost;
using Datahub.Core.Model.Context;

namespace Datahub.Infrastructure.Offline
{
    public class OfflineWorkspaceCostManagementService : IWorkspaceCostManagementService
    {
        public Task<(bool, decimal)> UpdateWorkspaceCostsAsync(string workspaceAcronym, List<DailyServiceCost> azureCosts)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RefreshWorkspaceCostsAsync(string workspaceAcronym)
        {
            throw new NotImplementedException();
        }

        public Task<bool> VerifyAndRefreshWorkspaceCostsAsync(string workspaceAcronym, List<DailyServiceCost> azureTotals, bool executeRefresh = true)
        {
            throw new NotImplementedException();
        }

        public bool CheckUpdateNeeded(string workspaceAcronym, DatahubProjectDBContext ctx)
        {
            throw new NotImplementedException();
        }

        public Task<List<DailyServiceCost>> QuerySubscriptionCostsAsync(string subscriptionId, DateTime startDate, DateTime endDate,
            QueryGranularity granularity, List<string>? rgNames = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<DailyServiceCost>> QueryScopeCostsAsync(string scopeId, DateTime startDate, DateTime endDate, QueryGranularity granularity,
            List<string>? rgNames = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<DailyServiceCost>> QueryWorkspaceCostsAsync(string workspaceAcronym, DateTime startDate, DateTime endDate,
            QueryGranularity granularity)
        {
            throw new NotImplementedException();
        }
    }
}