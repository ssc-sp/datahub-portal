using Datahub.Core.Model.Projects;

namespace Datahub.Application.Services
{
    public interface IProjectResourceWhitelistService
    {
        public Task<IEnumerable<ProjectWhitelist>> GetAllProjectResourceWhitelistAsync();
        public Task<ProjectWhitelist> GetWhitelistByProjectAsync(int id);
        public Task<ProjectWhitelist> GetProjectResourceWhitelistByProjectAsync(int id);
        public Task UpdateProjectResourceWhitelistAsync(ProjectWhitelist projectResourceWhitelist);
    }
}