using Datahub.Core.Model.Datahub;
using Datahub.Metadata.Model;

namespace Datahub.PowerBI.Services.Offline;

public class OfflinePowerBiDataService : IPowerBiDataService
{
    public Task<bool> AddOrUpdateCataloguedDatasets(IEnumerable<PowerBiDataSetDefinition> datasetDefinitions)
    {
        throw new NotImplementedException();
    }

    public Task<bool> AddOrUpdateCataloguedReports(IEnumerable<PowerBiReportDefinition> reportDefinitions)
    {
        throw new NotImplementedException();
    }

    public Task<bool> AddOrUpdateCataloguedWorkspaces(IEnumerable<PowerBiWorkspaceDefinition> workspaceDefinitions)
    {
        throw new NotImplementedException();
    }

    public Task<bool> BulkAddOrUpdatePowerBiItems(IEnumerable<PowerBiWorkspaceDefinition> workspaceDefinitions, IEnumerable<PowerBiDataSetDefinition> datasetDefinitions, IEnumerable<PowerBiReportDefinition> reportDefinitions)
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

    public Task<IList<PowerBiDataSet>> GetAllDatasets()
    {
        return Task.FromResult<IList<PowerBiDataSet>>(new List<PowerBiDataSet>());
    }

    public Task<IList<PowerBiReport>> GetAllReports()
    {
        return Task.FromResult<IList<PowerBiReport>>(new List<PowerBiReport>());
    }

    public Task<IList<PowerBiWorkspace>> GetAllWorkspaces()
    {
        return Task.FromResult<IList<PowerBiWorkspace>>(new List<PowerBiWorkspace>());
    }

    public Task<PowerBiDataSet> GetDatasetById(Guid id)
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

    public Task<PowerBiReport> GetReportById(Guid id, bool includeWorkspace = false)
    {
        throw new NotImplementedException();
    }

    public Task<List<PowerBiReport>> GetReportsForProject(string projectCode, bool includeSandbox = false)
    {
        return Task.FromResult(new List<PowerBiReport>());
    }

    public Task<List<PowerBiReport>> GetReportsForProjectWithExternalReportInfo(string projectCode, bool includeSandbox = false)
    {
        return Task.FromResult(new List<PowerBiReport>());
    }

    public Task<List<PowerBiReport>> GetReportsForUser(string userId)
    {
        return Task.FromResult(new List<PowerBiReport>());
    }

    public Task<List<ExternalPowerBiReport>> GetRequestedExternalReports()
    {
        throw new NotImplementedException();
    }

    public Task<PowerBiWorkspace> GetWorkspaceById(Guid id, bool includeChildren = false)
    {
        throw new NotImplementedException();
    }

    public Task<List<PowerBiReport>> GetWorkspaceReports(Guid id)
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