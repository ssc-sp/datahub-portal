using Datahub.Core.Model.Datahub;
using Datahub.Core.Services;
using Datahub.Core.Services.Metadata;
using Datahub.Core.Services.Notification;
using Datahub.Metadata.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.PowerBI.Services;

public class PowerBiDataService : IPowerBiDataService
{
    private readonly IDbContextFactory<DatahubProjectDBContext> _contextFactory;
    private readonly ILogger<PowerBiDataService> _logger;
    private readonly IMiscStorageService _miscStorageService;
    private readonly IDatahubAuditingService _auditingService;
    private readonly ISystemNotificationService _notificationService;
    private readonly IMetadataBrokerService _metadataService;

    private static readonly string GLOBAL_POWERBI_ADMIN_LIST_KEY = "GlobalPowerBiAdmins";

    private const string POWERBI_PUBLISHED_INTERNAL_LINK_PREFIX = "/internal-published-report";

    public PowerBiDataService(
        IDbContextFactory<DatahubProjectDBContext> contextFactory,
        ILogger<PowerBiDataService> logger,
        IMiscStorageService miscStorageService,
        IDatahubAuditingService auditingService,
        ISystemNotificationService notificationService,
        IMetadataBrokerService metadataService)
    {
        _contextFactory = contextFactory;
        _logger = logger;
        _miscStorageService = miscStorageService;
        _auditingService = auditingService;
        _notificationService = notificationService;
        _metadataService = metadataService;
    }

    public async Task<IList<PowerBiWorkspace>> GetAllWorkspaces()
    {
        using var ctx = await _contextFactory.CreateDbContextAsync();
        var results = await ctx.PowerBiWorkspaces.Include(w => w.Project).ToListAsync();
        return results;
    }

    public async Task<bool> AddOrUpdateCataloguedWorkspaces(IEnumerable<PowerBiWorkspaceDefinition> workspaceDefinitions)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync();

        return await DoUpdateWorkspaces(ctx, workspaceDefinitions);
    }

    private async Task<bool> DoUpdateWorkspaces(DatahubProjectDBContext ctx, IEnumerable<PowerBiWorkspaceDefinition> workspaceDefinitions)
    {
        foreach (var def in workspaceDefinitions)
        {
            var workspace = await ctx.PowerBiWorkspaces.FirstOrDefaultAsync(w => w.WorkspaceID == def.WorkspaceId);
            if (workspace == null)
            {
                _logger.LogDebug("Creating workspace record for {} ({})", def.WorkspaceName, def.WorkspaceId);

                workspace = new PowerBiWorkspace()
                {
                    WorkspaceID = def.WorkspaceId,
                    WorkspaceName = def.WorkspaceName,
                    SandboxFlag = def.SandboxFlag,
                    ProjectId = def.ProjectId
                };

                ctx.PowerBiWorkspaces.Add(workspace);
            }
            else
            {
                _logger.LogDebug("Updating workspace record for {} ({})", def.WorkspaceName, def.WorkspaceId);

                workspace.WorkspaceName = def.WorkspaceName;
                workspace.SandboxFlag = def.SandboxFlag;
                workspace.ProjectId = def.ProjectId;
            }
        }

        try
        {
            await ctx.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding/updating PowerBI workspaces");
            return false;
        }
    }

    public async Task<IList<PowerBiDataSet>> GetAllDatasets()
    {
        using var ctx = await _contextFactory.CreateDbContextAsync();
        var results = await ctx.PowerBiDataSets.ToListAsync();
        return results;
    }

    public async Task<bool> AddOrUpdateCataloguedDatasets(IEnumerable<PowerBiDataSetDefinition> datasetDefinitions)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync();

        return await DoUpdateDatasets(ctx, datasetDefinitions);
    }

    private async Task<bool> DoUpdateDatasets(DatahubProjectDBContext ctx, IEnumerable<PowerBiDataSetDefinition> datasetDefinitions)
    {
        foreach (var def in datasetDefinitions)
        {
            var dataset = await ctx.PowerBiDataSets.FirstOrDefaultAsync(d => d.DataSetID == def.DataSetId);
            if (dataset == null)
            {
                _logger.LogDebug("Creating dataset record for {} ({}) in workspace {}", def.DataSetName, def.DataSetId, def.WorkspaceId);

                dataset = new()
                {
                    DataSetID = def.DataSetId,
                    DataSetName = def.DataSetName,
                    WorkspaceId = def.WorkspaceId
                };

                ctx.PowerBiDataSets.Add(dataset);
            }
            else
            {
                _logger.LogDebug("Updating dataset {} ({})", def.DataSetName, def.DataSetId);

                dataset.DataSetID = def.DataSetId;
                dataset.DataSetName = def.DataSetName;
                dataset.WorkspaceId = def.WorkspaceId;
            }
        }

        try
        {
            await ctx.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding/updating PowerBI datasets");
            return false;
        }
    }

    public async Task<IList<PowerBiReport>> GetAllReports()
    {
        using var ctx = await _contextFactory.CreateDbContextAsync();
        var reports = await ctx.PowerBiReports.ToListAsync();
        return reports;
    }

    public async Task<bool> AddOrUpdateCataloguedReports(IEnumerable<PowerBiReportDefinition> reportDefinitions)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync();

        return await DoUpdateReports(ctx, reportDefinitions);
    }

    private async Task<bool> DoUpdateReports(DatahubProjectDBContext ctx, IEnumerable<PowerBiReportDefinition> reportDefinitions)
    {
        foreach (var def in reportDefinitions)
        {
            var report = await ctx.PowerBiReports.FirstOrDefaultAsync(d => d.ReportID == def.ReportId);
            if (report == null)
            {
                _logger.LogDebug("Creating report record for {} ({}) in workspace {}", def.ReportName, def.ReportId, def.WorkspaceId);

                report = new()
                {
                    ReportID = def.ReportId,
                    ReportName = def.ReportName,
                    WorkspaceId = def.WorkspaceId
                };

                ctx.PowerBiReports.Add(report);
            }
            else
            {
                _logger.LogDebug("Updating report {} ({})", def.ReportName, def.ReportId);

                report.ReportID = def.ReportId;
                report.ReportName = def.ReportName;
                report.WorkspaceId = def.WorkspaceId;
            }
        }

        try
        {
            await ctx.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding/updating PowerBI reports");
            return false;
        }
    }

    public async Task<List<PowerBiReport>> GetReportsForProject(string projectCode, bool includeSandbox = false)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync();

        var results = await ctx.PowerBiReports
            .Include(r => r.Workspace)
            .ThenInclude(w => w.Project)
            .Where(r => r.Workspace.Project != null
                        && r.Workspace.Project.ProjectAcronymCD.ToLower() == projectCode.ToLower()
                        && (includeSandbox || !r.Workspace.SandboxFlag))
            .ToListAsync();
        return results;
    }

    public async Task<List<PowerBiReport>> GetReportsForUser(string userId)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync();

        var results = await ctx.PowerBiReports
            .Include(r => r.Workspace)
            .ThenInclude(w => w.Project)
            .ThenInclude(p => p.Users)
            .Where(r => r.Workspace.Project != null && r.Workspace.Project.Users.Any(u => u.UserID == userId))
            .Distinct()
            .ToListAsync();

        return results;
    }

    public async Task<bool> BulkAddOrUpdatePowerBiItems(IEnumerable<PowerBiWorkspaceDefinition> workspaceDefinitions,
        IEnumerable<PowerBiDataSetDefinition> datasetDefinitions,
        IEnumerable<PowerBiReportDefinition> reportDefinitions)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync();
        var transaction = await ctx.Database.BeginTransactionAsync();

        var success = true;

        success &= await DoUpdateWorkspaces(ctx, workspaceDefinitions);
        success &= await DoUpdateDatasets(ctx, datasetDefinitions);
        success &= await DoUpdateReports(ctx, reportDefinitions);

        if (success)
        {
            await transaction.CommitAsync();
        }
        else
        {
            await transaction.RollbackAsync();
        }

        return success;
    }

    public async Task<PowerBiWorkspace> GetWorkspaceById(Guid id, bool includeChildren = false)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync();

        var query = ctx.PowerBiWorkspaces.Where(w => w.WorkspaceID == id);
        if (includeChildren)
        {
            query = query.Include(w => w.Reports).Include(w => w.Datasets);
        }

        var result = await query.FirstOrDefaultAsync();
        return result;
    }

    public async Task<List<PowerBiReport>> GetWorkspaceReports(Guid id)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync();

        var workspace = await ctx.PowerBiWorkspaces.Where(w => w.WorkspaceID == id).Include(w => w.Reports).FirstOrDefaultAsync();

        return workspace is not null ? new(workspace.Reports) : new List<PowerBiReport>();
    }

    public async Task<PowerBiDataSet> GetDatasetById(Guid id)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync();
        var result = await ctx.PowerBiDataSets.FirstOrDefaultAsync(d => d.DataSetID == id);
        return result;
    }

    public async Task<PowerBiReport> GetReportById(Guid id, bool includeWorkspace = false)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync();

        var query = ctx.PowerBiReports.Where(r => r.ReportID == id);

        if (includeWorkspace)
        {
            query = query.Include(r => r.Workspace)
                .ThenInclude(w => w.Project);
        }

        var result = await query.FirstOrDefaultAsync();
        return result;
    }

    public async Task<List<string>> GetGlobalPowerBiAdmins()
    {
        var result = await _miscStorageService.GetObject<List<string>>(GLOBAL_POWERBI_ADMIN_LIST_KEY);
        return result ?? new List<string>();
    }

    public async Task SetGlobalPowerBiAdmins(IEnumerable<string> adminEmails)
    {
        var adminList = adminEmails.ToList();
        await _miscStorageService.SaveObject(adminList, GLOBAL_POWERBI_ADMIN_LIST_KEY);
    }

    public async Task<List<PowerBiReport>> GetReportsForProjectWithExternalReportInfo(string projectCode, bool includeSandbox = false)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync();

        var results = await ctx.PowerBiReports
            .Include(r => r.Workspace)
            .ThenInclude(w => w.Project)
            .Where(r => r.Workspace.Project != null
                        && r.Workspace.Project.ProjectAcronymCD.ToLower() == projectCode.ToLower()
                        && (includeSandbox || !r.Workspace.SandboxFlag))
            .ToListAsync();

        var externalReportIds = await ctx.ExternalPowerBiReports.Where(r => r.EndDate >= DateTime.Now).Select(r => r.ReportID).ToListAsync();

        foreach (var report in results)
        {
            if (externalReportIds.Contains(report.ReportID))
                report.IsExternalReportActive = true;
        }

        return results;
    }

    public async Task CreateExternalPowerBiReportRequest(string userId, Guid reportId)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync();

        var request = new ExternalPowerBiReport
        {
            RequestingUser = userId,
            ReportID = reportId
        };

        ctx.ExternalPowerBiReports.Add(request);

        var result = await ctx.TrackSaveChangesAsync(_auditingService);

    }

    public async Task<bool> RevokePowerBiReportRequest(Guid reportId)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync();

        var existing = await ctx.ExternalPowerBiReports.FirstOrDefaultAsync(t => t.ReportID == reportId);
        var found = false;
        if (existing != null)
        {
            ctx.Remove(existing);
            found = true;
        }

        var result = await ctx.TrackSaveChangesAsync(_auditingService);
        return found;

    }


    public async Task<ExternalPowerBiReport> GetExternalReportRecord(Guid reportId)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync();

        return ctx.ExternalPowerBiReports.FirstOrDefault(r => r.ReportID == reportId);

    }

    public async Task<List<ExternalPowerBiReport>> GetRequestedExternalReports()
    {
        using var ctx = await _contextFactory.CreateDbContextAsync();

        return ctx.ExternalPowerBiReports.Where(r => !r.IsCreated).ToList();
    }

    public async Task UpdateExternalPowerBiRecord(ExternalPowerBiReport report)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync();
        var rep = ctx.ExternalPowerBiReports.Where(r => report.ExternalPowerBiReportID == r.ExternalPowerBiReportID).FirstOrDefault();
        if (rep != null)
        {
            rep.Url = report.Url;
            rep.Token = report.Token;
            rep.EndDate = report.EndDate;
            rep.IsCreated = report.IsCreated;
            rep.ValidationSalt = report.ValidationSalt;
            rep.ValidationCode = report.ValidationCode;
            await ctx.TrackSaveChangesAsync(_auditingService);
        }
    }

    public async Task<bool> DeleteWorkspace(Guid id)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync();
        using var tran = await ctx.Database.BeginTransactionAsync();

        var workspace = await ctx.PowerBiWorkspaces
            .Include(w => w.Reports)
            .Include(w => w.Datasets)
            .FirstOrDefaultAsync(w => w.WorkspaceID == id);

        var success = true;

        try
        {
            if (workspace != null)
            {
                ctx.PowerBiReports.RemoveRange(workspace.Reports);
                ctx.PowerBiDataSets.RemoveRange(workspace.Datasets);
                ctx.PowerBiWorkspaces.Remove(workspace);

                await ctx.TrackSaveChangesAsync(_auditingService);
                await tran.CommitAsync();
            }
        }
        catch (Exception e)
        {
            success = false;
            _logger.LogError(e, $"Error deleting Power BI workspace {id} and/or its children");
            await tran.RollbackAsync();
        }

        return await Task.FromResult(success);
    }

    public async Task<bool> DeleteDataset(Guid id)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync();
        var dataset = await ctx.PowerBiDataSets.FirstOrDefaultAsync(d => d.DataSetID == id);
        var success = true;
        if (dataset != null)
        {
            try
            {
                ctx.PowerBiDataSets.Remove(dataset);
                await ctx.TrackSaveChangesAsync(_auditingService);
            }
            catch (Exception e)
            {
                success = false;
                _logger.LogError(e, $"Error deleting Power BI dataset {id}");
            }
        }

        return await Task.FromResult(success);
    }

    public async Task<bool> DeleteReport(Guid id, Guid? datasetId)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync();
        using var tran = await ctx.Database.BeginTransactionAsync();
        var success = true;

        var report = await ctx.PowerBiReports.FirstOrDefaultAsync(r => r.ReportID == id);
        var dataset = datasetId.HasValue ?
            await ctx.PowerBiDataSets.FirstOrDefaultAsync(d => d.DataSetID == datasetId.Value) :
            await Task.FromResult(default(PowerBiDataSet));

        try
        {
            if (report != null)
            {
                ctx.PowerBiReports.Remove(report);
            }

            if (dataset != null)
            {
                ctx.PowerBiDataSets.Remove(dataset);
            }

            await ctx.TrackSaveChangesAsync(_auditingService);
            await tran.CommitAsync();
        }
        catch (Exception e)
        {
            success = false;
            _logger.LogError(e, $"Error deleting Power BI report {id} and/or dataset {datasetId}");
            await tran.RollbackAsync();
        }

        return await Task.FromResult(success);
    }

    private async Task<IEnumerable<string>> GetProjectAdmins(int projectId)
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();
        var projectUsers = await ctx.ProjectUsers
            .Where(u => u.Project.ProjectID == projectId && u.Role.IsAtLeastAdmin)
            .ToListAsync();

        return projectUsers.Select(u => u.PortalUser.GraphGuid);
    }

    public async Task NotifyOfMissingReport(Guid reportId, string userEmail)
    {
        var report = await GetReportById(reportId, true);
        var powerBiAdmins = await GetGlobalPowerBiAdmins();

        if (report.Workspace.ProjectId.HasValue)
        {
            var projectAdmins = await GetProjectAdmins(report.Workspace.ProjectId.Value);
            // notification service will remove duplicates when sending, so we don't need to worry about it here
            powerBiAdmins.AddRange(projectAdmins);
        }

        var localizationPrefix = "POWER_BI_REPORT";
        var textKey = $"{localizationPrefix}.NotFoundReportNotificationText";
        var linkKey = $"{localizationPrefix}.NotFoundReportNotificationLink";

        var projectAcronym = report.Workspace.Project?.ProjectAcronymCD;

        var actionLink = string.IsNullOrEmpty(projectAcronym) ? $"/admin/powerbi/report/{reportId}" : $"/admin/powerbi/{projectAcronym}/report/{reportId}";

        if (report.Workspace.Project == null)
        {
            await _notificationService.CreateSystemNotificationsWithLink(powerBiAdmins, actionLink, linkKey, textKey, report.ReportName, report.Workspace.WorkspaceName, userEmail);
        }
        else
        {
            var projectName = new BilingualStringArgument(report.Workspace.Project.Project_Name, report.Workspace.Project.ProjectNameFr);
            await _notificationService.CreateSystemNotificationsWithLink(powerBiAdmins, actionLink, linkKey, textKey, report.ReportName, projectName, userEmail);
        }
    }

    public async Task UpdateReportCatalogStatus(Guid reportId, bool inCatalog)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync();
        var report = await ctx.PowerBiReports.FirstOrDefaultAsync(r => r.ReportID == reportId);
        if (report is not null)
        {
            report.InCatalog = inCatalog;
            await ctx.TrackSaveChangesAsync(_auditingService);
        }
    }

    public static string GeneratePublishedInternalReportLinkStatic(string reportId, CatalogObjectLanguage language = CatalogObjectLanguage.Bilingual)
    {
        if (language == CatalogObjectLanguage.Bilingual)
        {
            return $"{POWERBI_PUBLISHED_INTERNAL_LINK_PREFIX}/{reportId}";
        }
        else
        {
            var languageSuffix = (language == CatalogObjectLanguage.French) ? "fr" : "en";
            return $"{POWERBI_PUBLISHED_INTERNAL_LINK_PREFIX}/{reportId}/{languageSuffix}";
        }
    }

    // in the future, this may require reading config values or something else that needs a properly setup service
    // for now, the static method is ok
    public string GeneratePublishedInternalReportLink(string reportId, CatalogObjectLanguage language = CatalogObjectLanguage.Bilingual)
        => GeneratePublishedInternalReportLinkStatic(reportId, language);

    public async Task<IEnumerable<CatalogLanguageLink>> GeneratePublishedInternalReportLinksFromCatalogAsync(string reportId)
    {
        var catalogItem = await _metadataService.GetCatalogObjectByObjectId(reportId);
        if (catalogItem != null)
        {
            if (catalogItem.GroupId.HasValue)
            {
                var groupItems = await _metadataService.GetCatalogGroup(catalogItem.GroupId.Value);
                return groupItems.Select(i => new CatalogLanguageLink(i.Language, GeneratePublishedInternalReportLink(i.Metadata.ObjectId, i.Language)));
            }
            else if (catalogItem.Language == CatalogObjectLanguage.Bilingual)
            {
                var bothLangs = new List<CatalogObjectLanguage>() { CatalogObjectLanguage.English, CatalogObjectLanguage.French };
                return bothLangs.Select(l => new CatalogLanguageLink(l, GeneratePublishedInternalReportLink(reportId, l)));
            }
            else
            {
                return new List<CatalogLanguageLink>() { new(catalogItem.Language, GeneratePublishedInternalReportLink(reportId, catalogItem.Language)) };
            }
        }
        else
        {
            return Enumerable.Empty<CatalogLanguageLink>();
        }
    }
}