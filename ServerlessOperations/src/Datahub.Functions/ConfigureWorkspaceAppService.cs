using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Messaging.ServiceBus;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Functions.Extensions;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Clients;
using Datahub.Shared.Configuration;
using Datahub.Shared.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("Datahub.Functions.UnitTests")]

namespace Datahub.Functions
{
    public class ConfigureWorkspaceAppService(
        ILogger<ConfigureWorkspaceAppService> logger,
        AzureConfig config,
        IDbContextFactory<DatahubProjectDBContext> dbContext)
    {
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContext = dbContext;

        [Function(nameof(ConfigureWorkspaceAppService))]
        public async Task Run(
            [ServiceBusTrigger(QueueConstants.WorkspaceAppServiceConfigurationQueueName,
                Connection = "DatahubServiceBus:ConnectionString")]
            ServiceBusReceivedMessage message)
        {
            logger.LogInformation($"C# Queue trigger function processed: {message.Body}");

            var appServiceConfigurationMessage = await  message.DeserializeAndUnwrapMessageAsync<WorkspaceAppServiceConfigurationMessage>();

            var projectAcronym = appServiceConfigurationMessage.ProjectAcronym;
            var configuration = appServiceConfigurationMessage.Configuration;

            await ConfigureAppService(projectAcronym, configuration);
        }

        private async Task ConfigureAppService(string projectAcronym, AppServiceConfiguration configuration)
        {
            var pipelineId = await GetPipelineIdByName(config.AzureDevOpsConfiguration.AppServiceConfigPipeline);
            await PostPipelineRun(pipelineId, configuration, projectAcronym);
        }

        private async Task<HttpClient> ConfigureHttpClient()
        {
            var adoProvider = new AzureDevOpsClient(config.AzureDevOpsConfiguration);
            return await adoProvider.PipelineClientAsync();
        }

        // private async Task<AppServiceConfiguration> GetAppServiceConfiguration(string projectAcronym)
        // {
        //     using var ctx = await _dbContext.CreateDbContextAsync();
        //     var project = ctx.Projects.AsNoTracking().Include(p => p.Resources)
        //         .FirstOrDefault(p => p.Project_Acronym_CD == projectAcronym);
        //
        //     if (project is null)
        //     {
        //         _logger.LogError("Project with acronym {ProjectAcronymValue} not found, could not configure web app",
        //             projectAcronym);
        //         throw new Exception($"Project with acronym {projectAcronym} not found, could not configure web app");
        //     }
        //
        //     var appServiceConfiguration = TerraformVariableExtraction.ExtractAppServiceConfiguration(project);
        //
        //     if (appServiceConfiguration is null)
        //     {
        //         _logger.LogError(
        //             "App service configuration not found for project acronym {ProjectAcronymValue}, could not configure web app",
        //             projectAcronym);
        //         throw new Exception(
        //             $"App service configuration not found for project acronym {projectAcronym}, could not configure web app");
        //     }
        //
        //     return appServiceConfiguration;
        // }

        internal async Task<int> GetPipelineIdByName(string pipelineName)
        {
            var httpClient = await ConfigureHttpClient();
            var url = config.AzureDevOpsConfiguration.ListPipelineUrlTemplate
                .Replace("{organization}", config.AzureDevOpsConfiguration.OrganizationName)
                .Replace("{project}", config.AzureDevOpsConfiguration.ProjectName);

            HttpResponseMessage response;
            try
            {
                response = await httpClient.GetAsync(url);
            }
            catch (HttpRequestException e)
            {
                logger.LogError("Invalid Pipeline URL");
                throw new ArgumentException($"Invalid Pipeline URL");
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Request to list pipelines failed");
                throw new ArgumentException($"Request to list pipelines failed");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var pipelines = JsonSerializer.Deserialize<PipelinesResponse>(responseContent)!.value;
            var pipeline = pipelines.FirstOrDefault(p => p.name == pipelineName);

            if (pipeline is null)
            {
                logger.LogError("Pipeline {PipelineName} not found", pipelineName);
                throw new Exception($"Pipeline {pipelineName} not found");
            }

            return pipeline.id;
        }

        internal JsonObject GetPipelineBody(AppServiceConfiguration appServiceConfiguration)
        {
            var gitUrl = appServiceConfiguration.GitRepo;
            if (appServiceConfiguration.IsGitRepoPrivate &&
                !string.IsNullOrWhiteSpace(appServiceConfiguration.GitToken))
            {
                gitUrl = gitUrl.Replace("https://", $"https://{appServiceConfiguration.GitToken}@");
            }

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
                    ["gitUrl"] = gitUrl,
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
            var pipelineUrl = config.AzureDevOpsConfiguration.PostPipelineRunUrlTemplate
                .Replace("{organization}", config.AzureDevOpsConfiguration.OrganizationName)
                .Replace("{project}", config.AzureDevOpsConfiguration.ProjectName)
                .Replace("{pipelineId}", pipelineId.ToString());

            logger.LogInformation(
                "Sending configuration request to pipeline url {PipelineUrl} with body {RequestBody}", pipelineUrl,
                body.ToString());

            var response =
                await httpClient.PostAsync(pipelineUrl, new StringContent(json, Encoding.UTF8, "application/json"));

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError(
                    "Error configuring web app for project acronym {ProjectAcronymValue}, could not configure web app",
                    projectAcronym);
                throw new Exception(
                    $"Error configuring web app for project acronym {projectAcronym}, could not configure web app");
            }

            logger.LogInformation("Web app configured for project acronym {ProjectAcronymValue}",
                projectAcronym);

            return response;
        }

        private record PipelinesResponse(int count, Pipeline[] value);

        private record Pipeline(string name, string url, int id);
    }
}