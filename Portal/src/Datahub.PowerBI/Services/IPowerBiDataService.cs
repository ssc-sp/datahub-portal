using Datahub.Core.Model.Datahub;
using Datahub.Metadata.Model;

namespace Datahub.PowerBI.Services;

public interface IPowerBiDataService
{
    public Task<IList<PowerBiWorkspace>> GetAllWorkspaces();
    public Task<bool> AddOrUpdateCataloguedWorkspaces(IEnumerable<PowerBiWorkspaceDefinition> workspaceDefinitions);
    public Task<bool> AddOrUpdateCataloguedWorkspace(PowerBiWorkspaceDefinition def) => AddOrUpdateCataloguedWorkspaces(new List<PowerBiWorkspaceDefinition> { def });
    public Task<PowerBiWorkspace> GetWorkspaceById(Guid id, bool includeChildren = false);
    public Task<List<PowerBiReport>> GetWorkspaceReports(Guid id);

    public Task<bool> DeleteWorkspace(Guid id);
    public Task<IList<PowerBiDataSet>> GetAllDatasets();
    public Task<bool> AddOrUpdateCataloguedDatasets(IEnumerable<PowerBiDataSetDefinition> datasetDefinitions);
    public Task<PowerBiDataSet> GetDatasetById(Guid id);
    public Task<bool> DeleteDataset(Guid id);
    public Task<IList<PowerBiReport>> GetAllReports();
    public Task<bool> AddOrUpdateCataloguedReports(IEnumerable<PowerBiReportDefinition> reportDefinitions);
    public Task<PowerBiReport> GetReportById(Guid id, bool includeWorkspace = false);
    public Task<bool> DeleteReport(Guid id, Guid? datasetId);
    public Task<bool> BulkAddOrUpdatePowerBiItems(IEnumerable<PowerBiWorkspaceDefinition> workspaceDefinitions, IEnumerable<PowerBiDataSetDefinition> datasetDefinitions, IEnumerable<PowerBiReportDefinition> reportDefinitions);

    public Task<List<PowerBiReport>> GetReportsForProject(string projectCode, bool includeSandbox = false);
    public Task<List<PowerBiReport>> GetReportsForUser(string userId);
    public Task<List<string>> GetGlobalPowerBiAdmins();
    public Task SetGlobalPowerBiAdmins(IEnumerable<string> adminEmails);
    public Task<List<PowerBiReport>> GetReportsForProjectWithExternalReportInfo(string projectCode, bool includeSandbox = false);

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