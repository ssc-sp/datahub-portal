using System.Text.Json;
using System.Text.Json.Nodes;
using System.Transactions;
using Datahub.Core.Enums;
using Datahub.Core.Model.Datahub;
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

    public TerraformOutputHandler(ILoggerFactory loggerFactory, DatahubProjectDBContext projectDbContext, QueuePongService pongService)
    {
        _projectDbContext = projectDbContext;
        _logger = loggerFactory.CreateLogger("TerraformOutputHandler");
        _pongService = pongService;
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

        _logger.LogInformation("C# Queue trigger function finished");
    }

    private async Task ProcessTerraformOutputVariables(
        IReadOnlyDictionary<string, TerraformOutputVariable> outputVariables)
    {
        try
        {
            using var transactionScope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
            await ProcessProjectStatus(outputVariables);
            await ProcessAzureStorageBlob(outputVariables);
            await ProcessAzureDatabricks(outputVariables);
            transactionScope.Complete();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error processing output variables");
            throw;
        }
    }

    private async Task ProcessAzureDatabricks(IReadOnlyDictionary<string,TerraformOutputVariable> outputVariables)
    {
        var projectAcronym = outputVariables[TerraformVariables.OutputProjectAcronym];
        var terraformServiceType = RequestManagementService.GetTerraformServiceType(TerraformTemplate.AzureDatabricks);

        var projectRequest = _projectDbContext.Project_Requests
            .Include(x => x.Project)
            .Where(x => x.Project.Project_Acronym_CD == projectAcronym.Value)
            .Where(x => !x.Is_Completed.HasValue)
            .FirstOrDefault(x => x.ServiceType == terraformServiceType);

        if (projectRequest is null)
        {
            _logger.LogInformation("Project request not found for project acronym {ProjectAcronymValue} and service type {TerraformServiceType}", projectAcronym.Value, terraformServiceType);
            return;
        }
        
        var databricksStatus = GetStatusMapping(outputVariables[TerraformVariables.OutputAzureDatabricksStatus].Value);
        if (databricksStatus == TerraformOutputStatus.Completed)
        {
            projectRequest.Is_Completed = DateTime.Now;
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
            _logger.LogInformation("Project resource already exists for project {ProjectAcronym} and service type {ServiceType}", projectAcronym.Value, terraformServiceType);
        }

        await _projectDbContext.SaveChangesAsync();
    }

    private async Task ProcessAzureStorageBlob(IReadOnlyDictionary<string, TerraformOutputVariable> outputVariables)
    {
        var projectAcronym = outputVariables[TerraformVariables.OutputProjectAcronym];
        var terraformServiceType = RequestManagementService.GetTerraformServiceType(TerraformTemplate.AzureStorageBlob);

        var projectRequest = _projectDbContext.Project_Requests
            .Include(x => x.Project)
            .Where(x => x.Project.Project_Acronym_CD == projectAcronym.Value)
            .Where(x => !x.Is_Completed.HasValue)
            .FirstOrDefault(x => x.ServiceType == terraformServiceType);

        if (projectRequest is null)
        {
            _logger.LogInformation("Project request not found for project acronym {ProjectAcronymValue} and service type {TerraformServiceType}", projectAcronym.Value, terraformServiceType);
            return;
        }

        var storageBlobStatus = GetStatusMapping(outputVariables[TerraformVariables.OutputAzureStorageBlobStatus].Value);
        if (storageBlobStatus == TerraformOutputStatus.Completed)
        {
            projectRequest.Is_Completed = DateTime.Now;
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
            _logger.LogInformation("Project resource already exists for project {ProjectAcronym} and service type {ServiceType}", projectAcronym.Value, terraformServiceType);
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
        
        var workspaceVersion = outputVariables[TerraformVariables.OutputWorkspaceVersion].Value;
        if (project.Version != workspaceVersion)
        {
            project.Version = workspaceVersion;
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