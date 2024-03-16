using Datahub.Application.Services.Budget;

namespace Datahub.Infrastructure.Offline
{
    public class OfflineWorkspaceCostManagementService : IWorkspaceCostManagementService
    {
        public Task<(bool, decimal)> UpdateWorkspaceCostAsync(List<DailyServiceCost> subCosts, string workspaceAcronym)
        {
            throw new NotImplementedException();
        }

        public Task<List<DailyServiceCost>?> QuerySubscriptionCosts(string subId, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public Task<List<DailyServiceCost>> GetWorkspaceCosts(List<DailyServiceCost> subCosts, string workspaceAcronym)
        {
            throw new NotImplementedException();
        }

        public List<DailyServiceCost> GroupBySource(List<DailyServiceCost> costs)
        {
            throw new NotImplementedException();
        }

        public List<DailyServiceCost> GroupByDate(List<DailyServiceCost> costs)
        {
            throw new NotImplementedException();
        }

        public List<DailyServiceCost> FilterCurrentFiscalYear(List<DailyServiceCost> costs)
        {
            throw new NotImplementedException();
        }

        public List<DailyServiceCost> FilterLastFiscalYear(List<DailyServiceCost> costs)
        {
            throw new NotImplementedException();
        }

        public List<DailyServiceCost> FilterDateRange(List<DailyServiceCost> costs, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public List<DailyServiceCost> FilterDateRange(List<DailyServiceCost> costs, DateTime date)
        {
            throw new NotImplementedException();
        }
    }
}