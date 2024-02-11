using System.Runtime.CompilerServices;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Storage.Queues.Models;
using Datahub.Application.Services;
using Datahub.Application.Services.Security;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Utils;
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
        private readonly IKeyVaultService _keyVaultService;
        private readonly AzureConfig _config;
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContext;
        internal HttpClient _httpClient = new();

        public ConfigureWorkspaceAppService(ILogger<ConfigureWorkspaceAppService> logger,
            IKeyVaultService keyVaultService, AzureConfig config, IDbContextFactory<DatahubProjectDBContext> dbContext)
        {
            _logger = logger;
            _keyVaultService = keyVaultService;
            _config = config;
            _dbContext = dbContext;
        }

        [Function(nameof(ConfigureWorkspaceAppService))]
        public async Task Run(
            [QueueTrigger("web-app-configuration", Connection = "DatahubStorageConnectionString")]
            QueueMessage message)
        {
            _logger.LogInformation($"C# Queue trigger function processed: {message.MessageText}");

            var appServiceConfigurationMessage =
                JsonSerializer.Deserialize<WorkspaceAppServiceConfigurationMessage>(message.MessageText)!;

            var projectAcronym = appServiceConfigurationMessage.ProjectAcronym;

            await ConfigureHttpClient();
            await ConfigureAppService(appServiceConfigurationMessage, projectAcronym);
        }

        private async Task ConfigureAppService(WorkspaceAppServiceConfigurationMessage appServiceConfigurationMessage,
            string projectAcronym)
        {
            var pipelineUrl = await GetPipelineUrlByName(_config.AdoConfig.AppServiceConfigPipeline);
            var appServiceConfiguration = await GetAppServiceConfiguration(projectAcronym);
            await PostPipelineRun(pipelineUrl, appServiceConfiguration, projectAcronym);
        }

        private async Task ConfigureHttpClient()
        {
            var credentials = await _keyVaultService.GetSecret(_config.AdoConfig.PatSecretName);
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes($":{credentials}")));
        }

        private async Task<AppServiceConfiguration> GetAppServiceConfiguration(string projectAcronym)
        {
            using var ctx = await _dbContext.CreateDbContextAsync();
            var project = ctx.Projects.AsNoTracking().FirstOrDefault(p => p.Project_Acronym_CD == projectAcronym);

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

        internal async Task<string> GetPipelineUrlByName(string pipelineName)
        {
            var url = _config.AdoConfig.ListPipelineUrlTemplate.Replace("{organization}", _config.AdoConfig.OrgName)
                .Replace("{project}", _config.AdoConfig.ProjectName);
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error listing pipelines");
                throw new Exception($"Error listing pipelines");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var pipelines = JsonSerializer.Deserialize<PipelinesResponse>(responseContent)!.Value;
            var pipeline = pipelines.FirstOrDefault(p => p.Name == pipelineName);

            if (pipeline is null)
            {
                _logger.LogError("Pipeline {PipelineName} not found", pipelineName);
                throw new ArgumentException($"Pipeline {pipelineName} not found");
            }

            return pipeline.Url;
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
                    ["resourceGroup"] = appServiceConfiguration.ResourceGroupName,
                    ["webAppId"] = appServiceConfiguration.Id,
                    ["gitUrl"] = appServiceConfiguration.GitRepo,
                    ["composePath"] = appServiceConfiguration.ComposePath
                }
            };
        }

        internal async Task<HttpResponseMessage> PostPipelineRun(string pipelineUrl,
            AppServiceConfiguration appServiceConfiguration,
            string projectAcronym)
        {
            var body = GetPipelineBody(appServiceConfiguration);
            var json = JsonSerializer.Serialize(body);

            _logger.LogInformation(
                "Sending configuration request to pipeline url {PipelineUrl} with body {RequestBody}", pipelineUrl,
                body.ToString());

            var response =
                await _httpClient.PostAsync(pipelineUrl,
                    new StringContent(json, Encoding.UTF8, "application/json"));

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

        private record PipelinesResponse(int Count, Pipeline[] Value);

        private record Pipeline(string Name, string Url, int Id);
    }
}