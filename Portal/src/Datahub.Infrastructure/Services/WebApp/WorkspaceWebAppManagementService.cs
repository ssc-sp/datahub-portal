using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.AppService;
using Datahub.Application.Configuration;
using Datahub.Application.Services.WebApp;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Infrastructure.Services.WebApp
{
    public class WorkspaceWebAppManagementService : IWorkspaceWebAppManagementService
    {
        private readonly ArmClient _armClient;
        private readonly DatahubPortalConfiguration _portalConfiguration;
        private readonly IMediator _mediatr;
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;

        public WorkspaceWebAppManagementService(DatahubPortalConfiguration portalConfiguration,
            IDbContextFactory<DatahubProjectDBContext> dbContextFactory, IMediator mediatr)
        {
            _portalConfiguration = portalConfiguration;
            _dbContextFactory = dbContextFactory;
            _mediatr = mediatr;
            var credential = new ClientSecretCredential(_portalConfiguration.AzureAd.TenantId,
                _portalConfiguration.AzureAd.InfraClientId, _portalConfiguration.AzureAd.InfraClientSecret);
            _armClient = new ArmClient(credential);
        }

        public async Task<bool> Start(string webAppId)
        {
            var webAppResource = await GetWebAppAzureResource(webAppId);
            var response = await webAppResource.StartAsync();
            return !response.IsError;
        }

        public async Task<bool> Stop(string webAppId)
        {
            var webAppResource = await GetWebAppAzureResource(webAppId);
            var response = await webAppResource.StopAsync();
            return !response.IsError;
        }

        public async Task<bool> Restart(string webAppId)
        {
            var webAppResource = await GetWebAppAzureResource(webAppId);
            var response = await webAppResource.RestartAsync();
            return !response.IsError;
        }

        public async Task<bool> GetState(string webAppId)
        {
            var webAppResource = await GetWebAppAzureResource(webAppId);
            if (webAppResource.HasData && webAppResource.Data.State == "Running")
            {
                return true;
            }

            return false;
        }

        public async Task SaveConfiguration(string workspaceAcronym, AppServiceConfiguration config)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var projectResource = await GetResource(context, workspaceAcronym);
            var inputJson = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(projectResource.InputJsonContent);
            inputJson["app_service_framework"] = config.Framework;
            inputJson["app_service_git_repo"] = config.GitRepo;
            inputJson["app_service_git_repo_visibility"] = config.IsGitRepoPrivate.ToString();
            inputJson["app_service_git_token_secret_name"] = config.GitTokenSecretName;
            inputJson["app_service_compose_path"] = config.ComposePath;
            var jsonObject = JsonSerializer.Serialize(inputJson);
            projectResource.InputJsonContent = jsonObject;

            await context.SaveChangesAsync();
        }

        public async Task Configure(string workspaceAcronym, AppServiceConfiguration configuration)
        {
            var message = new WorkspaceAppServiceConfigurationMessage(workspaceAcronym, configuration);
            await _mediatr.Send(message);
        }

        private async Task<WebSiteResource> GetWebAppAzureResource(string webAppId)
        {
            var resourceIdentifier = new ResourceIdentifier(webAppId);
            var webAppResource = _armClient.GetWebSiteResource(resourceIdentifier);
            if (webAppResource == null)
            {
                throw new Exception($"Web app with id {webAppId} not found.");
            }

            var response = await webAppResource.GetAsync();
            if (response.Value == null)
            {
                throw new Exception($"Web app with id {webAppId} not found.");
            }
            return response.Value;
        }

        private SiteInstanceResource GetSiteInstanceResource(string webAppId)
        {
            var resourceIdentifier = new ResourceIdentifier(webAppId);
            var webAppResource = _armClient.GetWebSiteResource(resourceIdentifier);
            var siteInstanceResource = _armClient.GetSiteInstanceResource(resourceIdentifier);
            if (siteInstanceResource == null)
            {
                throw new Exception($"Web app with id {webAppId} not found.");
            }

            return siteInstanceResource;
        }

        public async Task<Project_Resources2> GetResource(DatahubProjectDBContext context, string workspaceAcronym)
        {
            var workspace = await context.Projects
                .Include(x => x.Resources)
                .FirstOrDefaultAsync(x => x.Project_Acronym_CD == workspaceAcronym);

            if (workspace == null)
                throw new Exception($"Workspace with acronym {workspaceAcronym} not found");

            var projectResource = workspace.Resources
                .FirstOrDefault(x =>
                    x.ResourceType == TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureAppService));

            if (projectResource == null)
                throw new Exception($"Azure App Service resource not found for workspace {workspaceAcronym}");

            return projectResource;
        }

        public async Task<Project_Resources2> GetResource(string workspaceAcronym)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            return await GetResource(context, workspaceAcronym);
        }
    }
}