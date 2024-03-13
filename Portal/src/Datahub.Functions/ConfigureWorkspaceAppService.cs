using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Storage.Queues.Models;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Utils;
using Datahub.Functions.Providers;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("Datahub.Functions.UnitTests")]

namespace Datahub.Functions
{
    public class ConfigureWorkspaceAppService
    {
        private readonly ILogger<ConfigureWorkspaceAppService> _logger;
        private readonly AzureConfig _config;
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContext;

        public ConfigureWorkspaceAppService(ILogger<ConfigureWorkspaceAppService> logger,
            AzureConfig config, IDbContextFactory<DatahubProjectDBContext> dbContext)
        {
            _logger = logger;
            _config = config;
            _dbContext = dbContext;
        }

        [Function(nameof(ConfigureWorkspaceAppService))]
        public async Task Run(
            [QueueTrigger("workspace-app-service-configuration", Connection = "DatahubStorageConnectionString")]
            QueueMessage message)
        {
            _logger.LogInformation($"C# Queue trigger function processed: {message.MessageText}");

            var appServiceConfigurationMessage =
                JsonSerializer.Deserialize<WorkspaceAppServiceConfigurationMessage>(message.MessageText)!;

            var projectAcronym = appServiceConfigurationMessage.ProjectAcronym;

            await ConfigureAppService(appServiceConfigurationMessage, projectAcronym);
        }

        private async Task ConfigureAppService(WorkspaceAppServiceConfigurationMessage appServiceConfigurationMessage,
            string projectAcronym)
        {
            var pipelineId = await GetPipelineIdByName(_config.AdoConfig.AppServiceConfigPipeline);
            var appServiceConfiguration = await GetAppServiceConfiguration(projectAcronym);
            await PostPipelineRun(pipelineId, appServiceConfiguration, projectAcronym);
        }

        private async Task<HttpClient> ConfigureHttpClient()
        {
            var adoProvider = new AdoClientProvider(_config);
            return await adoProvider.GetPipelineClient();
        }

        private async Task<AppServiceConfiguration> GetAppServiceConfiguration(string projectAcronym)
        {
            using var ctx = await _dbContext.CreateDbContextAsync();
            var project = ctx.Projects.AsNoTracking().Include(p => p.Resources)
                .FirstOrDefault(p => p.Project_Acronym_CD == projectAcronym);

            if (project is null)
            {
                _logger.LogError("Project with acronym {ProjectAcronymValue} not found, could not configure web app",
                    projectAcronym);
                throw new Exception($"Project with acronym {projectAcronym} not found, could not configure web app");
            }

            var appServiceConfiguration = TerraformVariableExtraction.ExtractAppServiceConfiguration(project);

            if (appServiceConfiguration is null)
            {
                _logger.LogError(
                    "App service configuration not found for project acronym {ProjectAcronymValue}, could not configure web app",
                    projectAcronym);
                throw new Exception(
                    $"App service configuration not found for project acronym {projectAcronym}, could not configure web app");
            }

            return appServiceConfiguration;
        }

        internal async Task<int> GetPipelineIdByName(string pipelineName)
        {
            var httpClient = await ConfigureHttpClient();
            var url = _config.AdoConfig.ListPipelineUrlTemplate.Replace("{organization}", _config.AdoConfig.OrgName)
                .Replace("{project}", _config.AdoConfig.ProjectName);
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error listing pipelines");
                throw new Exception($"Error listing pipelines");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var pipelines = JsonSerializer.Deserialize<PipelinesResponse>(responseContent)!.value;
            var pipeline = pipelines.FirstOrDefault(p => p.name == pipelineName);

            if (pipeline is null)
            {
                _logger.LogError("Pipeline {PipelineName} not found", pipelineName);
                throw new ArgumentException($"Pipeline {pipelineName} not found");
            }

            return pipeline.id;
        }

        internal JsonObject GetPipelineBody(AppServiceConfiguration appServiceConfiguration)
        {
            return new JsonObject
            {
                ["resources"] = new JsonObject
                {
                    ["repositories"] = new JsonObject
                    {
                        ["self"] = new JsonObject
                        {
                            ["refName"] = "refs/heads/main"
                        }
                    }
                },
                ["templateParameters"] = new JsonObject
                {
                    ["webAppId"] = appServiceConfiguration.Id,
                    ["gitUrl"] = appServiceConfiguration.GitRepo,
                    ["composePath"] = appServiceConfiguration.ComposePath
                }
            };
        }

        internal async Task<HttpResponseMessage> PostPipelineRun(int pipelineId,
            AppServiceConfiguration appServiceConfiguration,
            string projectAcronym)
        {
            var httpClient = await ConfigureHttpClient();
            var body = GetPipelineBody(appServiceConfiguration);
            var json = JsonSerializer.Serialize(body); 
            var pipelineUrl = _config.AdoConfig.PostPipelineRunUrlTemplate.Replace("{organization}", _config.AdoConfig.OrgName)
                .Replace("{project}", _config.AdoConfig.ProjectName)
                .Replace("{pipelineId}", pipelineId.ToString());
            
            _logger.LogInformation(
                "Sending configuration request to pipeline url {PipelineUrl} with body {RequestBody}", pipelineUrl,
                body.ToString());

            var response =
                await httpClient.PostAsync(pipelineUrl, new StringContent(json, Encoding.UTF8, "application/json"));
            
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Error configuring web app for project acronym {ProjectAcronymValue}, could not configure web app",
                    projectAcronym);
                throw new Exception(
                    $"Error configuring web app for project acronym {projectAcronym}, could not configure web app");
            }

            _logger.LogInformation("Web app configured for project acronym {ProjectAcronymValue}",
                projectAcronym);

            return response;
        }

        private record PipelinesResponse(int count, Pipeline[] value);

        private record Pipeline(string name, string url, int id);
    }
}