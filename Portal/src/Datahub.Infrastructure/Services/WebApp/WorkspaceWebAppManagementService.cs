using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.AppService.Models;
using Datahub.Application.Configuration;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.WebApp;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Core.Utils;
using Datahub.Infrastructure.Extensions;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Configuration;
using Datahub.Shared.Entities;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Services.Common;

namespace Datahub.Infrastructure.Services.WebApp
{
    public class WorkspaceWebAppManagementService : IWorkspaceWebAppManagementService
    {
        private readonly ArmClient _armClient;
        private readonly DatahubPortalConfiguration _portalConfiguration;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
        private readonly IKeyVaultUserService _keyVaultUserService;

        public WorkspaceWebAppManagementService(DatahubPortalConfiguration portalConfiguration,
            IDbContextFactory<DatahubProjectDBContext> dbContextFactory, ISendEndpointProvider sendEndpointProvider,
            IKeyVaultUserService keyVaultUserService)
        {
            _portalConfiguration = portalConfiguration;
            _dbContextFactory = dbContextFactory;
            _sendEndpointProvider = sendEndpointProvider;
            _keyVaultUserService = keyVaultUserService;
            var credential = new ClientSecretCredential(_portalConfiguration.AzureAd.TenantId,
                _portalConfiguration.AzureAd.InfraClientId, _portalConfiguration.AzureAd.InfraClientSecret);
            _armClient = new ArmClient(credential);
        }

        public async Task<bool> Start(string webAppId, string workspaceAcronym)
        {
            if (!string.IsNullOrEmpty(workspaceAcronym))
            {
                await SetAppSettings(workspaceAcronym, webAppId);
            }

            return await Start(webAppId);
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

        public async Task<bool> Restart(string webAppId, string workspaceAcronym)
        {
            if (!string.IsNullOrEmpty(workspaceAcronym))
            {
                await SetAppSettings(workspaceAcronym, webAppId);
            }

            return await Restart(webAppId);
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

        public async Task FillSystemConfiguration(string workspaceAcronym, AppServiceConfiguration config)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var projectResource = await GetResource(context, workspaceAcronym);
            var json = JsonSerializer.Deserialize<Dictionary<string, string>>(projectResource.JsonContent);
            config.Id = json["app_service_id"];
            config.HostName = json["app_service_hostname"];
        }

        public async Task SetAppSettings(string workspaceAcronym, string webAppId)
        {
            var webAppResource = await GetWebAppAzureResource(webAppId);
            using var ctx = await _dbContextFactory.CreateDbContextAsync();

            var projectResource = await GetResource(ctx, workspaceAcronym);
            var envVarKeys = TerraformVariableExtraction.ExtractEnvironmentVariableKeys(projectResource);
            var appSettings = await GetAzureAppSettings(webAppId);
            var keyVaultName = _keyVaultUserService.GetVaultName(workspaceAcronym,
                _portalConfiguration.Hosting.EnvironmentName);

            foreach (var key in envVarKeys)
            {
                if (appSettings.Properties.ContainsKey(key))
                {
                    // If the key already exists in the app settings, we update it with a secret reference
                    appSettings.Properties[key] = BuildSecretReference(keyVaultName, key);
                }
                else
                {
                    // Otherwise we add a new key with a secret reference
                    var secretReference = BuildSecretReference(keyVaultName, key);
                    appSettings.Properties.Add(key, secretReference);
                }
            }

            await webAppResource.UpdateApplicationSettingsAsync(appSettings);
        }

        internal string BuildSecretReference(string keyVaultName, string envVarKey)
        {
            return $"@Microsoft.KeyVault(VaultName={keyVaultName};SecretName={ToSecretName(envVarKey)})";
        }
        
        internal string ToSecretName(string key)
        {
            return key.ToLower().Replace("_", "-");
        }

        public async Task<Dictionary<string, string>> GetAppSettings(string webAppId)
        {
            var webAppResource = await GetWebAppAzureResource(webAppId);
            var appSettings = await GetAzureAppSettings(webAppId);

            return appSettings.Properties.ToDictionary();
        }

        internal async Task<AppServiceConfigurationDictionary> GetAzureAppSettings(string webAppId)
        {
            var webAppResource = await GetWebAppAzureResource(webAppId);
            var appSettingsResponse = await webAppResource.GetApplicationSettingsAsync();
            if (!appSettingsResponse.HasValue)
            {
                throw new Exception($"App settings not found for web app {webAppId}");
            }

            var appSettings = appSettingsResponse.Value;
            if (appSettings == null)
            {
                throw new Exception($"Could not get app settings for web app {webAppId}");
            }

            return appSettings;
        }

        public async Task SaveConfiguration(string workspaceAcronym, AppServiceConfiguration config)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var projectResource = await GetResource(context, workspaceAcronym);
            var inputJson = JsonSerializer.Deserialize<Dictionary<string, string>>(projectResource.InputJsonContent);
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
            await _sendEndpointProvider.SendDatahubServiceBusMessage(
                QueueConstants.WorkspaceAppServiceConfigurationQueueName, message);
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