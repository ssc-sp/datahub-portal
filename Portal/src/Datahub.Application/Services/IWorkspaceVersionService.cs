using Datahub.Core.Model.Datahub;
using Datahub.Shared.Entities;

namespace Datahub.Application.Services;

public class OutdatedWorkspaceInfo
{
    public string WorkspaceName { get; set; }
    public string WorkspaceAcronym { get; set; }
    public string CurrentVersion { get; set; }
}

public class WorkspaceVersionStatistics
{
    public int TotalWorkspaces { get; set; }
    public int LatestVersionWorkspaces { get; set; }
    public int OutdatedWorkspaces { get; set; }
    public decimal PercentageOnLatest { get; set; }
    public int WorkspacesWithUpdateRequests { get; set; }
    public string LatestVersion { get; set; }
}

public interface IWorkspaceVersionService
{
    public Task<string> GetLatestVersionAsync();
    public Task<List<VersionTag>> GetAllVersionsAsync();
    public Task<VersionTag?> GetVersionByIdAsync(int versionTagId);
    public Task<bool> AddNewVersion(VersionTag versionTag);
    public Task<bool> UpdateVersionTag(VersionTag versionTag);
    public Task<bool> DeleteVersion(VersionTag versionTag);
    public Task<bool> SetResourcesToCreateRequested(int projectId);
    public Task<bool> SetWorkspaceToUpdateRequested(int projectId);
    public Task<List<OutdatedWorkspaceInfo>> GetWorkspacesNotOnLatestVersionAsync();
    public Task<WorkspaceVersionStatistics> GetWorkspaceVersionStatisticsAsync();
}