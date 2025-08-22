using Datahub.Core.Model.Datahub;
using Datahub.Shared.Entities;

namespace Datahub.Application.Services;


public interface IWorkspaceVersionService
{
    public Task<string> GetLatestVersionAsync();
    public Task<List<VersionTag>> GetAllVersionsAsync();
    public Task<bool> AddNewVersion(VersionTag versionTag);
    public Task<bool> UpdateVersionTag(VersionTag versionTag);
    public Task<bool> DeleteVersion(VersionTag versionTag);
    public Task<bool> SetResourcesToCreateRequested(int projectId);
    public Task<bool> SetWorkspaceToUpdateRequested(int projectId);
}