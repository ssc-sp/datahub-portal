using Datahub.Core.Model.Datahub;
using Datahub.Shared.Entities;

namespace Datahub.Application.Services;

public interface IWorkspaceVersionService
{
    public Task<string> GetLatestVersion();
    public Task<List<VersionTag>> GetAllVersionsAsync();
    public Task<bool> AddNewVersion(VersionTag versionTag);
    public Task<bool> UpdateVersionTag(VersionTag versionTag);
    public Task<bool> DeleteVersion(VersionTag versionTag);
    public Task<bool> IsGreenLightChange(string versionTag);


}