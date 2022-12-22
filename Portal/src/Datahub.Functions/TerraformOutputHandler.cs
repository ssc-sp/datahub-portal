using System.Text.Json;
using System.Text.Json.Nodes;
using System.Transactions;
using Datahub.Core.Enums;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services.Projects;
using Datahub.ProjectTools.Services;
using DefaultNamespace;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions;

public class TerraformOutputHandler
{
    private readonly DatahubProjectDBContext _projectDbContext;
    private readonly ILogger _logger;

    private const string AzureStorageBlobTemplateName = "azure-storage-blob";
    
    private const string OutputProjectAcronym = "project_cd";
    private const string OutputNewProjectTemplate = "new_project_template";
    
    private const string OutputAzureStorageAccountName = "azure_storage_account_name";
    private const string OutputAzureStorageContainerName = "azure_storage_container_name";
    private const string OutputAzureStorageBlobStatus = "azure_storage_blob_status";
    
    private const string AzureStorageType = "blob";

    public TerraformOutputHandler(ILoggerFactory loggerFactory, DatahubProjectDBContext projectDbContext)
    {
        _projectDbContext = projectDbContext;
        _logger = loggerFactory.CreateLogger("TerraformOutputHandler");
    }

    [Function("TerraformOutputHandler")]
    public async Task RunAsync(
        [QueueTrigger("terraform-output", Connection = "TerraformOutputConnectionString")]
        string myQueueItem,
        FunctionContext context)
    {
        _logger.LogInformation($"C# Queue trigger function started");

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
            transactionScope.Complete();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error processing output variables");
            throw;
        }
    }

    private async Task ProcessAzureStorageBlob(IReadOnlyDictionary<string, TerraformOutputVariable> outputVariables)
    {
        var projectAcronym = outputVariables[OutputProjectAcronym];
        var terraformServiceType = RequestManagementService.GetTerraformServiceType(AzureStorageBlobTemplateName);

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

        var storageBlobStatus = GetStatusMapping(outputVariables[OutputAzureStorageBlobStatus].Value);
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
            throw new Exception($"Project resource not found for project acronym {projectAcronym.Value} and service type {terraformServiceType}");
        }
        if (!projectResource.TimeCreated.HasValue)
        {
            var accountName = outputVariables[OutputAzureStorageAccountName];
            var containerName = outputVariables[OutputAzureStorageContainerName];
            var jsonContent = new JsonObject
            {
                ["storage_account"] = accountName.Value,
                ["container"] = containerName.Value,
                ["storage_type"] = AzureStorageType,
            };

            var inputJsonContent = new JsonObject
            {
                ["storage_type"] = AzureStorageType
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
        var projectAcronym = outputVariables[OutputProjectAcronym];
        var project = await _projectDbContext.Projects
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym.Value);

        if (project == null)
        {
            _logger.LogError("Project not found for acronym {ProjectId}", projectAcronym.Value);
            throw new Exception($"Project not found for acronym {projectAcronym.Value}");
        }

        var outputPhase = GetStatusMapping(outputVariables[OutputNewProjectTemplate].Value);
        if (project.Project_Phase != outputPhase)
        {
            project.Project_Phase = outputPhase;
            await _projectDbContext.SaveChangesAsync();
        }
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