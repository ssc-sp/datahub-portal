using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Shared.Entities;

namespace Datahub.Application.Services.WebApp
{
    public interface IWorkspaceWebAppManagementService
    {
        public Task<bool> Start(string webAppId, string workspaceAcronym);
        public Task<bool> Start(string webAppId);
        public Task<bool> Stop(string webAppId);
        public Task<bool> Restart(string webAppId, string workspaceAcronym);
        public Task<bool> Restart(string webAppId);
        public Task<bool> GetState(string webAppId);
        public Task SetAppSettings(string workspaceAcronym, string webAppId);
        public Task<Dictionary<string, string>> GetAppSettings(string webAppId);
        public Task SaveConfiguration(string workspaceAcronym, AppServiceConfiguration configuration);
        public Task FillSystemConfiguration(string workspaceAcronym, AppServiceConfiguration configuration);
        public Task Configure(string workspaceAcronym, AppServiceConfiguration configuration);
        public Task<Project_Resources2> GetResource(DatahubProjectDBContext dbContext, string workspaceAcronym);
        public Task<Project_Resources2> GetResource(string workspaceAcronym);
        
    }
}