using Datahub.Core.Model.Datahub;

namespace Datahub.Application.Services
{
    public interface IProjectResourceWhitelistService
    {
        public Task<IEnumerable<Datahub_Project_Resources_Whitelist>> GetAllProjectResourceWhitelistAsync();
        public Task<Datahub_Project_Resources_Whitelist> GetProjectResourceWhitelistByProjectAsync(int id);
        public Task UpdateProjectResourceWhitelistAsync(Datahub_Project_Resources_Whitelist projectResourceWhitelist);
        
        
    }
}