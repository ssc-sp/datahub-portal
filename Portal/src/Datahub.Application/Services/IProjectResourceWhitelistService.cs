using Datahub.Core.Model.Projects;

namespace Datahub.Application.Services
{
	public interface IProjectResourceWhitelistService
    {
        public Task<IEnumerable<Project_Whitelist>> GetAllProjectResourceWhitelistAsync();
        public Task<Project_Whitelist> GetWhitelistByProjectAsync(int id);
        public Task<Project_Whitelist> GetProjectResourceWhitelistByProjectAsync(int id);
        public Task UpdateProjectResourceWhitelistAsync(Project_Whitelist projectResourceWhitelist);
    }
}