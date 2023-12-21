using Datahub.Application.Services.Notebooks;
using Datahub.Core.Data.Databricks;
using Datahub.Core.Model.Repositories;

namespace Datahub.Infrastructure.Offline;

public class OfflineDatabricksApiService : IDatabricksApiService
{
    public Task<List<ProjectRepository>> ListDisplayedWorkspaceRepositoriesAsync(string projectAcronym)
    {
        return Task.FromResult(new List<ProjectRepository>());
    }

    public Task<List<RepositoryInfoDto>> ListWorkspaceRepositoriesAsync(string projectAcronym, string accessToken)
    {
        return Task.FromResult(new List<RepositoryInfoDto>());
    }

    public Task<bool> UpdateWorkspaceRepository(string projectAcronym, RepositoryInfoDto repositoryInfoDto)
    {
        return Task.FromResult(true);
    }
}