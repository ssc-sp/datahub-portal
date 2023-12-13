using System.Text.Json;
using System.Text.Json.Nodes;
using System.Transactions;
using Datahub.Application.Services;
using Datahub.Core.Enums;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services.Projects;
using Datahub.Infrastructure.Services;
using Datahub.ProjectTools.Services;
using Datahub.Shared;
using Datahub.Shared.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions;

public class TerraformOutputHandler
{
    private readonly DatahubProjectDBContext _projectDbContext;
    private readonly ILogger _logger;
    private readonly QueuePongService _pongService;
    private readonly IResourceMessagingService _resourceMessagingService;
    private readonly AzureConfig _config;
    private const string TerraformOutputHandlerName = "terraform-output-handler";

    public TerraformOutputHandler(ILoggerFactory loggerFactory, DatahubProjectDBContext projectDbContext,
        QueuePongService pongService, IResourceMessagingService resourceMessagingService, AzureConfig config)
    {
        _projectDbContext = projectDbContext;
        _logger = loggerFactory.CreateLogger("TerraformOutputHandler");
        _pongService = pongService;
        _resourceMessagingService = resourceMessagingService;
        _config = config;
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
        
        if(!output.ContainsKey(TerraformVariables.OutputAzureDatabricksWorkspaceUrl) 
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
            await ProcessProjectStatus(outputVariables);
            await ProcessAzureStorageBlob(outputVariables);
            await ProcessAzureDatabricks(outputVariables);
            await ProcessAzureWebApp(outputVariables);
            transactionScope.Complete();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error processing output variables");
            throw;
        }
    }

    private async Task ProcessAzureWebApp(IReadOnlyDictionary<string,TerraformOutputVariable> outputVariables)
    {
        var projectAcronym = outputVariables[TerraformVariables.OutputProjectAcronym];
        var terraformServiceType = RequestManagementService.GetTerraformServiceType(TerraformTemplate.AzureAppService);

        var projectRequest = _projectDbContext.ProjectRequestAudits
            .Include(x => x.Project)
            .Where(x => x.Project.Project_Acronym_CD == projectAcronym.Value)
            .Where(x => !x.CompletedDateTime.HasValue)
            .FirstOrDefault(x => x.RequestType == terraformServiceType);

        if (projectRequest is null)
        {
            _logger.LogInformation(
                "Project request not found for project acronym {ProjectAcronymValue} and service type {TerraformServiceType}",
                projectAcronym.Value, terraformServiceType);
            return;
        }

        var azureAppServiceStatus = GetStatusMapping(outputVariables[TerraformVariables.OutputAzureAppServiceStatus].Value);
        if (azureAppServiceStatus.Equals(TerraformOutputStatus.Completed, StringComparison.InvariantCultureIgnoreCase))
        {
            projectRequest.CompletedDateTime = DateTime.Now;
        }
        else
        {
            _logger.LogInformation("Azure App Service status is not completed. Status: {Status}", azureAppServiceStatus);
        }

        var projectResource = _projectDbContext.Project_Resources2
            .Where(x => x.ProjectId == projectRequest.Project.Project_ID)
            .FirstOrDefault(x => x.ResourceType == terraformServiceType);

        if (projectResource is null)
        {
            var inputParameters = new Dictionary<string, string>();
            projectResource = RequestManagementService.CreateEmptyProjectResource(projectRequest, inputParameters);
            _projectDbContext.Project_Resources2.Add(projectResource);
        }

        if (!projectResource.TimeCreated.HasValue)
        {
            var appServiceId = outputVariables[TerraformVariables.OutputAzureAppServiceId];
            var appServiceHostName = outputVariables[TerraformVariables.OutputAzureAppServiceHostName];

            var jsonContent = new JsonObject
            {
                ["app_service_id"] = appServiceId.Value,
                ["app_service_hostname"] = appServiceHostName.Value,
            };

            var inputJsonContent = new JsonObject();

            projectResource.TimeCreated = DateTime.Now;
            projectResource.JsonContent = jsonContent.ToString();
            projectResource.InputJsonContent = inputJsonContent.ToString();
            
            var project = await _projectDbContext.Projects
                .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym.Value);

            if (project != null)
            {
                project.WebApp_URL = appServiceHostName.Value;
                project.WebAppEnabled = true;
            }
        }
        else
        {
            _logger.LogInformation(
                "Project resource already exists for project {ProjectAcronym} and service type {ServiceType}",
                projectAcronym.Value, terraformServiceType);
        }

        await _projectDbContext.SaveChangesAsync();
    }

    private async Task ProcessAzureDatabricks(IReadOnlyDictionary<string, TerraformOutputVariable> outputVariables)
    {
        var projectAcronym = outputVariables[TerraformVariables.OutputProjectAcronym];
        var terraformServiceType = RequestManagementService.GetTerraformServiceType(TerraformTemplate.AzureDatabricks);

        var projectRequest = _projectDbContext.ProjectRequestAudits
            .Include(x => x.Project)
            .Where(x => x.Project.Project_Acronym_CD == projectAcronym.Value)
            .Where(x => !x.CompletedDateTime.HasValue)
            .FirstOrDefault(x => x.RequestType == terraformServiceType);

        if (projectRequest is null)
        {
            _logger.LogInformation(
                "Project request not found for project acronym {ProjectAcronymValue} and service type {TerraformServiceType}",
                projectAcronym.Value, terraformServiceType);
            return;
        }

        var databricksStatus = GetStatusMapping(outputVariables[TerraformVariables.OutputAzureDatabricksStatus].Value);
        if (databricksStatus.Equals(TerraformOutputStatus.Completed, StringComparison.InvariantCultureIgnoreCase))
        {
            projectRequest.CompletedDateTime = DateTime.Now;
        }
        else
        {
            _logger.LogInformation("Azure Databricks status is not completed. Status: {Status}", databricksStatus);
        }

        var projectResource = _projectDbContext.Project_Resources2
            .Where(x => x.ProjectId == projectRequest.Project.Project_ID)
            .FirstOrDefault(x => x.ResourceType == terraformServiceType);

        if (projectResource is null)
        {
            var inputParameters = new Dictionary<string, string>();
            projectResource = RequestManagementService.CreateEmptyProjectResource(projectRequest, inputParameters);
            _projectDbContext.Project_Resources2.Add(projectResource);
        }

        if (!projectResource.TimeCreated.HasValue)
        {
            var workspaceId = outputVariables[TerraformVariables.OutputAzureDatabricksWorkspaceId];
            var workspaceUrl = outputVariables[TerraformVariables.OutputAzureDatabricksWorkspaceUrl];
            var workspaceName = outputVariables[TerraformVariables.OutputAzureDatabricksWorkspaceName];

            var jsonContent = new JsonObject
            {
                ["workspace_id"] = workspaceId.Value,
                ["workspace_url"] = workspaceUrl.Value,
                ["workspace_name"] = workspaceName.Value
            };

            var inputJsonContent = new JsonObject();

            projectResource.TimeCreated = DateTime.Now;
            projectResource.JsonContent = jsonContent.ToString();
            projectResource.InputJsonContent = inputJsonContent.ToString();
        }
        else
        {
            _logger.LogInformation(
                "Project resource already exists for project {ProjectAcronym} and service type {ServiceType}",
                projectAcronym.Value, terraformServiceType);
        }

        await _projectDbContext.SaveChangesAsync();
    }

    private async Task ProcessAzureStorageBlob(IReadOnlyDictionary<string, TerraformOutputVariable> outputVariables)
    {
        var projectAcronym = outputVariables[TerraformVariables.OutputProjectAcronym];
        var terraformServiceType = RequestManagementService.GetTerraformServiceType(TerraformTemplate.AzureStorageBlob);

        var projectRequest = _projectDbContext.ProjectRequestAudits
            .Include(x => x.Project)
            .Where(x => x.Project.Project_Acronym_CD == projectAcronym.Value)
            .Where(x => !x.CompletedDateTime.HasValue)
            .FirstOrDefault(x => x.RequestType == terraformServiceType);

        if (projectRequest is null)
        {
            _logger.LogInformation(
                "Project request not found for project acronym {ProjectAcronymValue} and service type {TerraformServiceType}",
                projectAcronym.Value, terraformServiceType);
            return;
        }

        var storageBlobStatus =
            GetStatusMapping(outputVariables[TerraformVariables.OutputAzureStorageBlobStatus].Value);
        if (storageBlobStatus.Equals(TerraformOutputStatus.Completed, StringComparison.InvariantCultureIgnoreCase))
        {
            projectRequest.CompletedDateTime = DateTime.Now;
        }
        else
        {
            _logger.LogInformation("Azure storage blob status is not completed. Status: {Status}", storageBlobStatus);
        }


        var projectResource = _projectDbContext.Project_Resources2
            .Where(x => x.ProjectId == projectRequest.Project.Project_ID)
            .FirstOrDefault(x => x.ResourceType == terraformServiceType);

        if (projectResource is null)
        {
            var inputParameters = new Dictionary<string, string>();
            projectResource = RequestManagementService.CreateEmptyProjectResource(projectRequest, inputParameters);
            _projectDbContext.Project_Resources2.Add(projectResource);
        }

        if (!projectResource.TimeCreated.HasValue)
        {
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

            projectResource.TimeCreated = DateTime.Now;
            projectResource.JsonContent = jsonContent.ToString();
            projectResource.InputJsonContent = inputJsonContent.ToString();
        }
        else
        {
            _logger.LogInformation(
                "Project resource already exists for project {ProjectAcronym} and service type {ServiceType}",
                projectAcronym.Value, terraformServiceType);
        }

        await _projectDbContext.SaveChangesAsync();
    }

    private async Task ProcessProjectStatus(IReadOnlyDictionary<string, TerraformOutputVariable> outputVariables)
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
}