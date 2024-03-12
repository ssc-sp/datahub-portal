using Datahub.Core.Model.Datahub;
using Datahub.Metadata.Model;

namespace Datahub.PowerBI.Services;

public interface IPowerBiDataService
{
	public Task<IList<PowerBi_Workspace>> GetAllWorkspaces();
	public Task<bool> AddOrUpdateCataloguedWorkspaces(IEnumerable<PowerBi_WorkspaceDefinition> workspaceDefinitions);
	public Task<bool> AddOrUpdateCataloguedWorkspace(PowerBi_WorkspaceDefinition def) => AddOrUpdateCataloguedWorkspaces(new List<PowerBi_WorkspaceDefinition> { def });
	public Task<PowerBi_Workspace> GetWorkspaceById(Guid id, bool includeChildren = false);
	public Task<List<PowerBi_Report>> GetWorkspaceReports(Guid id);

	public Task<bool> DeleteWorkspace(Guid id);
	public Task<IList<PowerBi_DataSet>> GetAllDatasets();
	public Task<bool> AddOrUpdateCataloguedDatasets(IEnumerable<PowerBi_DataSetDefinition> datasetDefinitions);
	public Task<PowerBi_DataSet> GetDatasetById(Guid id);
	public Task<bool> DeleteDataset(Guid id);
	public Task<IList<PowerBi_Report>> GetAllReports();
	public Task<bool> AddOrUpdateCataloguedReports(IEnumerable<PowerBi_ReportDefinition> reportDefinitions);
	public Task<PowerBi_Report> GetReportById(Guid id, bool includeWorkspace = false);
	public Task<bool> DeleteReport(Guid id, Guid? datasetId);
	public Task<bool> BulkAddOrUpdatePowerBiItems(IEnumerable<PowerBi_WorkspaceDefinition> workspaceDefinitions, IEnumerable<PowerBi_DataSetDefinition> datasetDefinitions, IEnumerable<PowerBi_ReportDefinition> reportDefinitions);

	public Task<List<PowerBi_Report>> GetReportsForProject(string projectCode, bool includeSandbox = false);
	public Task<List<PowerBi_Report>> GetReportsForUser(string userId);
	public Task<List<string>> GetGlobalPowerBiAdmins();
	public Task SetGlobalPowerBiAdmins(IEnumerable<string> adminEmails);
	public Task<List<PowerBi_Report>> GetReportsForProjectWithExternalReportInfo(string projectCode, bool includeSandbox = false);

	public Task CreateExternalPowerBiReportRequest(string userId, Guid reportId);
	public Task<bool> RevokePowerBiReportRequest(Guid reportId);
	public Task<ExternalPowerBiReport> GetExternalReportRecord(Guid reportId);
	public Task<List<ExternalPowerBiReport>> GetRequestedExternalReports();
	public Task UpdateExternalPowerBiRecord(ExternalPowerBiReport report);

	public Task NotifyOfMissingReport(Guid reportId, string userEmail);

	public Task UpdateReportCatalogStatus(Guid reportId, bool inCatalog);

	string GeneratePublishedInternalReportLink(string reportId, CatalogObjectLanguage language = CatalogObjectLanguage.Bilingual);
	string GeneratePublishedInternalReportLink(Guid reportId, CatalogObjectLanguage language = CatalogObjectLanguage.Bilingual)
		=> GeneratePublishedInternalReportLink(reportId.ToString(), language);

	Task<IEnumerable<CatalogLanguageLink>> GeneratePublishedInternalReportLinksFromCatalogAsync(string reportId);
	Task<IEnumerable<CatalogLanguageLink>> GeneratePublishedInternalReportLinksFromCatalogAsync(Guid reportId)
		=> GeneratePublishedInternalReportLinksFromCatalogAsync(reportId.ToString());
}

public record CatalogLanguageLink(CatalogObjectLanguage Language, string Url);