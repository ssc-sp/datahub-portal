namespace Datahub.Application.Services.Reports
{
    public interface IWorkspaceReportsManagementService
    {
        public Task<bool> CreateReportAsync(string workspaceAcronym, WorkspaceReport report);
        public Task<List<WorkspaceReport>> GetReportsAsync(string workspaceAcronym);
    }

    public record WorkspaceReport(
        DateTime GenerationDate,
        DateTime UpdateDate,
        string GeneratedBy,
        (DateTime, DateTime) CoverageDates,
        WorkspaceReportType ReportType,
        WorkspaceReportStatus ReportStatus,
        string ReportName,
        string ReportUrl);

    public enum WorkspaceReportType
    {
        Cost,
        Storage,
        CostAndStorage
    }

    public enum WorkspaceReportStatus
    {
        Requested,
        Generating,
        Generated,
        Failed
    }
}