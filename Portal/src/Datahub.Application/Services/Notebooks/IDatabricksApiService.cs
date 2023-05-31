using Datahub.Core.Data.Databricks;

namespace Datahub.Application.Services.Notebooks;

public interface IDatabricksApiService
{
        public Task<List<RepositoryInfo>> ListWorkspaceRepositories(string projectAcronym);
        public Task<bool> SetWorkspaceRepositoryVisibility(string projectAcronym, string repositoryUrl, bool isPublic);
        
}