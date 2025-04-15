using Datahub.Shared.Entities;

namespace Datahub.Application.Services;

public interface IWorkspaceVersionService
{
    public Task<string> GetLatestVersion();
    
}