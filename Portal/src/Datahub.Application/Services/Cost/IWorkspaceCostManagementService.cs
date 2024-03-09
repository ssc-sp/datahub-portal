namespace Datahub.Application.Services.Budget
{
    public interface IWorkspaceCostManagementService
    {
        public Task<List<DailyServiceCost>> GetCostByPeriodAsync(string workspaceAcronym, DateTime date);

        public Task<List<DailyServiceCost>> GetCostByPeriodAsync(string workspaceAcronym, DateTime startDate,
            DateTime endDate);

        public Task<List<DailyServiceCost>> GetAllCostsAsync(string workspaceAcronym);

        public Task<List<DailyServiceCost>> GroupBySource(List<DailyServiceCost> costs);
        public Task<List<DailyServiceCost>> GroupByDate(List<DailyServiceCost> costs);
    }

    public struct DailyServiceCost
    {
        public decimal Amount { get; set; }
        public string Source { get; set; }
        public DateTime Date { get; set; }
    }

    public struct FiscalYear
    {
        public FiscalYear(int startYear, int endYear)
        {
            StartDate = new DateTime(startYear, 3, 1);
            EndDate = new DateTime(endYear, 2, 31);
        }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}