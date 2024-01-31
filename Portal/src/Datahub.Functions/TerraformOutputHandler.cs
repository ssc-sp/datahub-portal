using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Transactions;
using Datahub.Application.Services;
using Datahub.Application.Services.Security;
using Datahub.Core.Enums;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Core.Services.Projects;
using Datahub.Core.Utils;
using Datahub.Infrastructure.Services;
using Datahub.ProjectTools.Services;
using Datahub.Shared;
using Datahub.Shared.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Datahub.Functions;

public class TerraformOutputHandler
{
    private readonly DatahubProjectDBContext _projectDbContext;
    private readonly ILogger _logger;
    private readonly QueuePongService _pongService;
    private readonly IResourceMessagingService _resourceMessagingService;
    private readonly AzureConfig _config;
    private readonly IKeyVaultService _keyVaultService;
    private const string TerraformOutputHandlerName = "terraform-output-handler";

    public TerraformOutputHandler(ILoggerFactory loggerFactory, DatahubProjectDBContext projectDbContext,
        QueuePongService pongService, IResourceMessagingService resourceMessagingService, AzureConfig config,
        IKeyVaultService keyVaultService)
    {
        _projectDbContext = projectDbContext;
        _logger = loggerFactory.CreateLogger("TerraformOutputHandler");
        _pongService = pongService;
        _resourceMessagingService = resourceMessagingService;
        _config = config;
        _keyVaultService = keyVaultService;
    }

    [Function("TerraformOutputHandler")]
    public async Task RunAsync(
        [QueueTrigger("terraform-output", Connection = "DatahubStorageConnectionString")]
        string myQueueItem,
        FunctionContext context)
    {
        _logger.LogInformation($"C# Queue trigger function started");

        // test for ping
        if (await _pongService.Pong(myQueueItem))
            return;

        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var output =
            JsonSerializer.Deserialize<Dictionary<string, TerraformOutputVariable>>(myQueueItem, deserializeOptions);

        _logger.LogInformation("C# Queue trigger function processing: {OutputCount} items", output?.Count);

        if (output is null)
        {
            _logger.LogInformation("Output is null. C# Queue trigger function processed and finishing");
            return;
        }

        try
        {
            await ProcessTerraformOutputVariables(output);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error processing output variable {OutputVariable}", output);
            throw;
        }

        await ProcessPostTerraformTriggers(output);


        _logger.LogInformation("C# Queue trigger function finished");
    }

    private async Task ProcessPostTerraformTriggers(IReadOnlyDictionary<string, TerraformOutputVariable> output)
    {
        _logger.LogInformation("Terraform processing complete, triggering post terraform triggers");

        // check if there's a workspace version variable
        if (!output.ContainsKey(TerraformVariables.OutputWorkspaceVersion) ||
            string.IsNullOrWhiteSpace(output[TerraformVariables.OutputWorkspaceVersion].Value))
        {
            _logger.LogInformation("Project version is null or empty, skipping post terraform triggers");
            return;
        }

        var projectVersionString = output[TerraformVariables.OutputWorkspaceVersion].Value;

        // exclude the first character, which is a v
        var projectVersion = new Version(projectVersionString[1..]);

        // double check it's above version 2.13.0
        if (projectVersion < new Version(2, 13, 0))
        {
            _logger.LogInformation("Project version is below 2.13.0, skipping post terraform triggers");
            return;
        }

        if (!output.ContainsKey(TerraformVariables.OutputAzureDatabricksWorkspaceUrl)
            || string.IsNullOrWhiteSpace(output[TerraformVariables.OutputAzureDatabricksWorkspaceUrl].Value))
        {
            _logger.LogInformation("Azure Databricks workspace url is null or empty, skipping post terraform triggers");
            return;
        }

        // handle external user permissions
        var projectAcronym = output[TerraformVariables.OutputProjectAcronym];
        var project = await _projectDbContext.Projects
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym.Value);

        if (project is null)
        {
            _logger.LogError("Project not found for acronym {ProjectId}", projectAcronym.Value);
            throw new Exception($"Project not found for acronym {projectAcronym.Value}");
        }

        _logger.LogInformation("Processing user updates to external permissions for project {ProjectAcronym}",
            projectAcronym.Value);
        var workspaceDefinition =
            await _resourceMessagingService.GetWorkspaceDefinition(project.Project_Acronym_CD,
                TerraformOutputHandlerName);
        await _resourceMessagingService.SendToUserQueue(workspaceDefinition,
            _config.StorageQueueConnection, _config.UserRunRequestQueueName);
        _logger.LogInformation(
            "Processing complete for user updates to external permissions for project {ProjectAcronym}",
            projectAcronym.Value);
    }

    private async Task ProcessTerraformOutputVariables(
        IReadOnlyDictionary<string, TerraformOutputVariable> outputVariables)
    {
        try
        {
            using var transactionScope =
                new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
            await ProcessWorkspaceStatus(outputVariables);
            await ProcessAzureStorageBlob(outputVariables);
            await ProcessAzureDatabricks(outputVariables);
            await ProcessAzureWebApp(outputVariables);
            await ProcessAzurePostgres(outputVariables);
            transactionScope.Complete();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error processing output variables");
            throw;
        }
    }

    internal async Task ProcessAzureWebApp(IReadOnlyDictionary<string, TerraformOutputVariable> outputVariables)
    {
        if (!outputVariables.ContainsKey(TerraformVariables.OutputAzureAppServiceStatus))
        {
            _logger.LogInformation("Azure App Service status not found in output variables");
            return;
        }

        var azureAppServiceStatus =
            GetStatusMapping(outputVariables[TerraformVariables.OutputAzureAppServiceStatus].Value);
        if (!azureAppServiceStatus.Equals(TerraformOutputStatus.Completed, StringComparison.InvariantCultureIgnoreCase))
        {
            _logger.LogError("Azure App Service status is not completed. Status: {Status}",
                azureAppServiceStatus);
            return;
        }

        var projectResource = await GetProjectResource(outputVariables,
            TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureAppService));

        var appServiceId = outputVariables[TerraformVariables.OutputAzureAppServiceId];
        var appServiceHostName = outputVariables[TerraformVariables.OutputAzureAppServiceHostName];

        JsonObject jsonContent = JsonObject.Parse(projectResource.JsonContent).AsObject();
        jsonContent.Add("app_service_id", appServiceId.Value);
        jsonContent.Add("app_service_hostname", appServiceHostName.Value);

        projectResource.CreatedAt = DateTime.UtcNow;
        projectResource.JsonContent = jsonContent.ToString();

        projectResource.Project.WebApp_URL = appServiceHostName.Value;
        projectResource.Project.WebAppEnabled = true;

        await _projectDbContext.SaveChangesAsync();

        await ConfigureAppService(outputVariables);
    }

    internal async Task ProcessAzureDatabricks(IReadOnlyDictionary<string, TerraformOutputVariable> outputVariables)
    {
        if (!outputVariables.ContainsKey(TerraformVariables.OutputAzureDatabricksStatus))
        {
            _logger.LogInformation("Azure Databricks status not found in output variables");
            return;
        }

        var databricksStatus = GetStatusMapping(outputVariables[TerraformVariables.OutputAzureDatabricksStatus].Value);
        if (!databricksStatus.Equals(TerraformOutputStatus.Completed, StringComparison.InvariantCultureIgnoreCase))
        {
            _logger.LogError("Azure Databricks status is not completed. Status: {Status}", databricksStatus);
            return;
        }

        var projectResource = await GetProjectResource(outputVariables,
            TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureDatabricks));

        var workspaceId = outputVariables[TerraformVariables.OutputAzureDatabricksWorkspaceId];
        var workspaceUrl = outputVariables[TerraformVariables.OutputAzureDatabricksWorkspaceUrl];
        var workspaceName = outputVariables[TerraformVariables.OutputAzureDatabricksWorkspaceName];

        var jsonContent = new JsonObject
        {
            ["workspace_id"] = workspaceId.Value,
            ["workspace_url"] = workspaceUrl.Value,
            ["workspace_name"] = workspaceName.Value
        };

        projectResource.CreatedAt = DateTime.UtcNow;
        projectResource.JsonContent = jsonContent.ToString();

        await _projectDbContext.SaveChangesAsync();
    }


    internal async Task ProcessAzureStorageBlob(IReadOnlyDictionary<string, TerraformOutputVariable> outputVariables)
    {
        if (!outputVariables.ContainsKey(TerraformVariables.OutputAzureStorageBlobStatus))
        {
            _logger.LogInformation("Azure storage blob status not found in output variables");
            return;
        }

        var storageBlobStatus =
            GetStatusMapping(outputVariables[TerraformVariables.OutputAzureStorageBlobStatus].Value);
        if (!storageBlobStatus.Equals(TerraformOutputStatus.Completed, StringComparison.InvariantCultureIgnoreCase))
        {
            _logger.LogError("Azure storage blob status is not completed. Status: {Status}", storageBlobStatus);
            return;
        }

        var projectResource = await GetProjectResource(outputVariables,
            TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureStorageBlob));

        var accountName = outputVariables[TerraformVariables.OutputAzureStorageAccountName];
        var containerName = outputVariables[TerraformVariables.OutputAzureStorageContainerName];
        var resourceGroupName = outputVariables[TerraformVariables.OutputAzureResourceGroupName];
        var jsonContent = new JsonObject
        {
            ["storage_account"] = accountName.Value,
            ["container"] = containerName.Value,
            ["storage_type"] = TerraformVariables.AzureStorageType,
            ["resource_group_name"] = resourceGroupName.Value
        };

        var inputJsonContent = new JsonObject
        {
            ["storage_type"] = TerraformVariables.AzureStorageType
        };

        projectResource.CreatedAt = DateTime.UtcNow;
        projectResource.JsonContent = jsonContent.ToString();
        projectResource.InputJsonContent = inputJsonContent.ToString();

        await _projectDbContext.SaveChangesAsync();
    }

    internal async Task ProcessAzurePostgres(IReadOnlyDictionary<string, TerraformOutputVariable> outputVariables)
    {
        if (!outputVariables.ContainsKey(TerraformVariables.OutputAzurePostgresStatus))
        {
            _logger.LogInformation("Azure Postgres status not found in output variables");
            return;
        }

        var postgresStatus = GetStatusMapping(outputVariables[TerraformVariables.OutputAzurePostgresStatus].Value);
        if (!postgresStatus.Equals(TerraformOutputStatus.Completed, StringComparison.InvariantCultureIgnoreCase))
        {
            _logger.LogError("Azure Postgres status is not completed. Status: {Status}", postgresStatus);
            return;
        }

        var projectResource = await GetProjectResource(outputVariables,
            TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzurePostgres));

        var postgresId = outputVariables[TerraformVariables.OutputAzurePostgresId];
        var postgresDns = outputVariables[TerraformVariables.OutputAzurePostgresDns];
        var postgresDbName = outputVariables[TerraformVariables.OutputAzurePostgresDatabaseName];
        var postgresSecretNameAdmin = outputVariables[TerraformVariables.OutputAzurePostgresSecretNameAdmin];
        var postgresSecretNamePassword = outputVariables[TerraformVariables.OutputAzurePostgresSecretNamePassword];
        var postgresServerName = outputVariables[TerraformVariables.OutputAzurePostgresServerName];

        var jsonContent = new JsonObject
        {
            ["postgres_id"] = postgresId.Value,
            ["postgres_dns"] = postgresDns.Value,
            ["postgres_db_name"] = postgresDbName.Value,
            ["postgres_secret_name_admin"] = postgresSecretNameAdmin.Value,
            ["postgres_secret_name_password"] = postgresSecretNamePassword.Value,
            ["postgres_server_name"] = postgresServerName.Value
        };

        projectResource.CreatedAt = DateTime.UtcNow;
        projectResource.JsonContent = jsonContent.ToString();

        await _projectDbContext.SaveChangesAsync();
    }

    internal async Task ProcessWorkspaceStatus(IReadOnlyDictionary<string, TerraformOutputVariable> outputVariables)
    {
        var projectAcronym = outputVariables[TerraformVariables.OutputProjectAcronym];
        var project = await _projectDbContext.Projects
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym.Value);

        if (project == null)
        {
            _logger.LogError("Project not found for acronym {ProjectId}", projectAcronym.Value);
            throw new Exception($"Project not found for acronym {projectAcronym.Value}");
        }

        var outputPhase = GetStatusMapping(outputVariables[TerraformVariables.OutputNewProjectTemplate].Value);
        if (project.Project_Phase != outputPhase)
        {
            project.Project_Phase = outputPhase;
        }

        // check if there's a workspace version variable
        if (outputVariables.ContainsKey(TerraformVariables.OutputWorkspaceVersion))
        {
            var workspaceVersion = outputVariables[TerraformVariables.OutputWorkspaceVersion].Value;
            if (project.Version != workspaceVersion)
            {
                project.Version = workspaceVersion;
            }
        }
        else
        {
            _logger.LogInformation("Workspace version not found in output variables");
        }

        var projectResource = await GetProjectResource(outputVariables,
            TerraformTemplate.GetTerraformServiceType(TerraformTemplate.NewProjectTemplate));

        projectResource.CreatedAt = DateTime.UtcNow;

        await _projectDbContext.SaveChangesAsync();
    }

    private static string GetStatusMapping(string value)
    {
        return value switch
        {
            "completed" => TerraformOutputStatus.Completed,
            "in_progress" => TerraformOutputStatus.InProgress,
            "pending_approval" => TerraformOutputStatus.PendingApproval,
            "failed" => TerraformOutputStatus.Failed,
            _ => TerraformOutputStatus.Missing
        };
    }

    private async Task<Project_Resources2> GetProjectResource(
        IReadOnlyDictionary<string, TerraformOutputVariable> outputVariables, string terraformServiceType)
    {
        var projectAcronym = outputVariables[TerraformVariables.OutputProjectAcronym];

        var project = await _projectDbContext.Projects
            .Include(p => p.Resources)
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym.Value);

        if (project is null)
        {
            _logger.LogError("Project not found for acronym {ProjectId}", projectAcronym.Value);
            throw new Exception($"Project not found for acronym {projectAcronym.Value}");
        }

        var projectResource = project.Resources
            .FirstOrDefault(x => x.ResourceType == terraformServiceType);

        if (projectResource is null)
        {
            _logger.LogError(
                "Project resource not found for project acronym {ProjectAcronymValue} and service type {TerraformServiceType}",
                projectAcronym.Value, terraformServiceType);
            throw new Exception(
                $"Project resource not found for project acronym {projectAcronym.Value} and service type {terraformServiceType}");
        }

        return projectResource;
    }

    private async Task ConfigureAppService(IReadOnlyDictionary<string, TerraformOutputVariable> outputVariables)
    {
        _logger.LogInformation("Configuring web app for project acronym {ProjectAcronymValue}",
            outputVariables[TerraformVariables.OutputProjectAcronym].Value);
        
        var projectAcronym = outputVariables[TerraformVariables.OutputProjectAcronym];
        var project = await _projectDbContext.Projects
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym.Value);

        if (project is null)
        {
            _logger.LogError("Project not found for acronym {ProjectId}, could not configure web app",
                projectAcronym.Value);
            throw new Exception($"Project not found for acronym {projectAcronym.Value}, could not configure web app");
        }

        var appServiceConfiguration = TerraformVariableExtraction.ExtractAppServiceConfiguration(project);

        if (appServiceConfiguration is null)
        {
            _logger.LogError(
                "App service configuration not found for project acronym {ProjectAcronymValue}, could not configure web app",
                projectAcronym.Value);
            throw new Exception(
                $"App service configuration not found for project acronym {projectAcronym.Value}, could not configure web app");
        }

        var credentials = await _keyVaultService.GetSecret(_config.AdoConfig.PatSecretName);
        var pipelineUrl = _config.AdoConfig.PipelineURL.Replace("{organization}", _config.AdoConfig.OrgName)
            .Replace("{project}", _config.AdoConfig.ProjectName)
            .Replace("{pipelineId}", _config.AdoConfig.AppServiceConfigPipelineId);

        var rgName = outputVariables[TerraformVariables.OutputAzureResourceGroupName].Value;

        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($":{credentials}")));

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
        
        var json = JsonConvert.SerializeObject(body);
        
        _logger.LogInformation("Sending configuration request to pipeline url {PipelineUrl} with body {RequestBody}", pipelineUrl, body.ToString());
        var response = await httpClient.PostAsync(pipelineUrl, new StringContent(json, Encoding.UTF8, "application/json"));
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Error configuring web app for project acronym {ProjectAcronymValue}, could not configure web app",
                projectAcronym.Value);
            throw new Exception(
                $"Error configuring web app for project acronym {projectAcronym.Value}, could not configure web app");
        }
        
        _logger.LogInformation("Web app configured for project acronym {ProjectAcronymValue}", projectAcronym.Value);
    }
}