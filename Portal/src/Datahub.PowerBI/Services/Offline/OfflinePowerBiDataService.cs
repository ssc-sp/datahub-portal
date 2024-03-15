using Datahub.Core.Model.Datahub;
using Datahub.Metadata.Model;

namespace Datahub.PowerBI.Services.Offline;

public class OfflinePowerBiDataService : IPowerBiDataService
{
	public Task<bool> AddOrUpdateCataloguedDatasets(IEnumerable<PowerBi_DataSetDefinition> datasetDefinitions)
	{
		throw new NotImplementedException();
	}

	public Task<bool> AddOrUpdateCataloguedReports(IEnumerable<PowerBi_ReportDefinition> reportDefinitions)
	{
		throw new NotImplementedException();
	}

	public Task<bool> AddOrUpdateCataloguedWorkspaces(IEnumerable<PowerBi_WorkspaceDefinition> workspaceDefinitions)
	{
		throw new NotImplementedException();
	}

	public Task<bool> BulkAddOrUpdatePowerBiItems(IEnumerable<PowerBi_WorkspaceDefinition> workspaceDefinitions, IEnumerable<PowerBi_DataSetDefinition> datasetDefinitions, IEnumerable<PowerBi_ReportDefinition> reportDefinitions)
	{
		throw new NotImplementedException();
	}

	public Task CreateExternalPowerBiReportRequest(string userId, Guid reportId)
	{
		throw new NotImplementedException();
	}

	public Task<bool> DeleteDataset(Guid id)
	{
		throw new NotImplementedException();
	}

	public Task<bool> DeleteReport(Guid id, Guid? datasetId)
	{
		throw new NotImplementedException();
	}

	public Task<bool> DeleteWorkspace(Guid id)
	{
		throw new NotImplementedException();
	}

	public string GeneratePublishedInternalReportLink(string reportId, CatalogObjectLanguage language = CatalogObjectLanguage.Bilingual)
	{
		throw new NotImplementedException();
	}

	public Task<IEnumerable<CatalogLanguageLink>> GeneratePublishedInternalReportLinksFromCatalogAsync(string reportId)
	{
		throw new NotImplementedException();
	}

	public Task<IList<PowerBi_DataSet>> GetAllDatasets()
	{
		return Task.FromResult<IList<PowerBi_DataSet>>(new List<PowerBi_DataSet>());
	}

	public Task<IList<PowerBi_Report>> GetAllReports()
	{
		return Task.FromResult<IList<PowerBi_Report>>(new List<PowerBi_Report>());
	}

	public Task<IList<PowerBi_Workspace>> GetAllWorkspaces()
	{
		return Task.FromResult<IList<PowerBi_Workspace>>(new List<PowerBi_Workspace>());
	}

	public Task<PowerBi_DataSet> GetDatasetById(Guid id)
	{
		throw new NotImplementedException();
	}

	public Task<ExternalPowerBiReport> GetExternalReportRecord(Guid reportId)
	{
		throw new NotImplementedException();
	}

	public Task<List<string>> GetGlobalPowerBiAdmins()
	{
		throw new NotImplementedException();
	}

	public Task<PowerBi_Report> GetReportById(Guid id, bool includeWorkspace = false)
	{
		throw new NotImplementedException();
	}

	public Task<List<PowerBi_Report>> GetReportsForProject(string projectCode, bool includeSandbox = false)
	{
		return Task.FromResult(new List<PowerBi_Report>());
	}

	public Task<List<PowerBi_Report>> GetReportsForProjectWithExternalReportInfo(string projectCode, bool includeSandbox = false)
	{
		return Task.FromResult(new List<PowerBi_Report>());
	}

	public Task<List<PowerBi_Report>> GetReportsForUser(string userId)
	{
		return Task.FromResult(new List<PowerBi_Report>());
	}

	public Task<List<ExternalPowerBiReport>> GetRequestedExternalReports()
	{
		throw new NotImplementedException();
	}

	public Task<PowerBi_Workspace> GetWorkspaceById(Guid id, bool includeChildren = false)
	{
		throw new NotImplementedException();
	}

	public Task<List<PowerBi_Report>> GetWorkspaceReports(Guid id)
	{
		throw new NotImplementedException();
	}

	public Task NotifyOfMissingReport(Guid reportId, string userEmail)
	{
		throw new NotImplementedException();
	}

	public Task<bool> RevokePowerBiReportRequest(Guid reportId)
	{
		throw new NotImplementedException();
	}

	public Task SetGlobalPowerBiAdmins(IEnumerable<string> adminEmails)
	{
		throw new NotImplementedException();
	}

	public Task UpdateExternalPowerBiRecord(ExternalPowerBiReport report)
	{
		throw new NotImplementedException();
	}

	public Task UpdateReportCatalogStatus(Guid reportId, bool inCatalog)
	{
		throw new NotImplementedException();
	}
}