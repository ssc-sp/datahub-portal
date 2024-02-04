using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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

namespace Datahub.Functions
{
    public class ConfigureAppService
    {
        private readonly ILogger<ConfigureAppService> _logger;
        private readonly IKeyVaultService _keyVaultService;
        private readonly AzureConfig _config;
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContext;
        private readonly IResourceMessagingService _resourceMessagingService;
        private HttpClient _httpClient = new ();

        public ConfigureAppService(ILogger<ConfigureAppService> logger, IKeyVaultService keyVaultService, AzureConfig config, IResourceMessagingService resourceMessagingService, IDbContextFactory<DatahubProjectDBContext> dbContext)
        {
            _logger = logger;
            _keyVaultService = keyVaultService;
            _config = config;
            _resourceMessagingService = resourceMessagingService;
            _dbContext = dbContext;
        }

        [Function(nameof(ConfigureAppService))]
        public async Task Run([QueueTrigger("web-app-configuration", Connection = "DatahubStorageConnectionString")] QueueMessage message)
        {
            _logger.LogInformation($"C# Queue trigger function processed: {message.MessageText}");
            
            var appServiceConfigurationMessage = JsonSerializer.Deserialize<AppServiceConfigurationMessage>(message.MessageText)!;
            
            var projectAcronym = appServiceConfigurationMessage.ProjectAcronym;
            
            using var ctx = await _dbContext.CreateDbContextAsync();
            var project = ctx.Projects.AsNoTracking().FirstOrDefault(p => p.Project_Acronym_CD == projectAcronym);
            
            if (project is null)
            {
                _logger.LogError("Project with acronym {ProjectAcronymValue} not found, could not configure web app", projectAcronym);
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

            var rgName = appServiceConfiguration.AppServiceRg;

            await ConfigureHttpClient();

            var pipelineUrl = await GetPipelineUrlByName(_config.AdoConfig.AppServiceConfigPipeline);
            
            await PostPipelineRun(pipelineUrl, appServiceConfiguration, projectAcronym, rgName);
        }
        
        private async Task ConfigureHttpClient()
        {
            var credentials = await _keyVaultService.GetSecret(_config.AdoConfig.PatSecretName);
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes($":{credentials}")));
        }

        private async Task<string> GetPipelineUrlByName(string pipelineName)
        {
            var url = _config.AdoConfig.GetPipelineURL.Replace("{organization}", _config.AdoConfig.OrgName)
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
                throw new Exception($"Pipeline {pipelineName} not found");
            }

            return pipeline.Url;
        }
        
        private async Task PostPipelineRun(string pipelineUrl, AppServiceConfiguration appServiceConfiguration, string projectAcronym, string rgName)
        {
            var body = new
            {
                resources = new
                {
                    repositories = new
                    {
                        self = new
                        {
                            refName = "refs/heads/main"
                        }
                    }
                },
                templateParameters = new
                {
                    resourceGroup = rgName,
                    webAppId = appServiceConfiguration.AppServiceId,
                    gitUrl = appServiceConfiguration.AppServiceGitRepo,
                    composePath = appServiceConfiguration.AppServiceComposePath
                }
            };

            var json = JsonSerializer.Serialize(body);

            _logger.LogInformation(
                "Sending configuration request to pipeline url {PipelineUrl} with body {RequestBody}", pipelineUrl,
                body.ToString());
            
            var response =
                await _httpClient.PostAsync(pipelineUrl, new StringContent(json, Encoding.UTF8, "application/json"));

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
        }
        
        private record PipelinesResponse(int Count, Pipeline[] Value);

        private record Pipeline(string Name, string Url, int Id);

    }
}