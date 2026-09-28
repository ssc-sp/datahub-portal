using System.Text.Json.Nodes;
using System.Transactions;
using Azure.Messaging.ServiceBus;
using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Shared;
using Datahub.Shared.Configuration;
using Datahub.Shared.Entities;
using Datahub.Shared.Entities.WorkspaceToolConfiguration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions;

public class TerraformOutputHandler(
    ILoggerFactory loggerFactory,
    DatahubProjectDBContext projectDbContext,
    IQueuePongService pongService,
    IResourceMessagingService resourceMessagingService,
    AzureConfig config)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger("TerraformOutputHandler");
    private const string TerraformOutputHandlerName = "terraform-output-handler";

    [Function("TerraformOutputHandler")]
    public async Task RunAsync(
        [ServiceBusTrigger(QueueConstants.TerraformOutputHandlerQueueName,
            Connection = "DatahubServiceBus:ConnectionString")]
        ServiceBusReceivedMessage message)
    {
        _logger.LogInformation($"C# Queue trigger function started");

        // Log ServiceBusReceivedMessage details
        _logger.LogInformation("ServiceBus message received - MessageId: {MessageId}, CorrelationId: {CorrelationId}, Subject: {Subject}, ContentType: {ContentType}", 
            message.MessageId, message.CorrelationId, message.Subject, message.ContentType);
        
        _logger.LogInformation("ServiceBus message body: {MessageBody}", message.Body.ToString());
                
        // test for ping
        // if (await pongService.Pong(message.Body.ToString()))
        // return;

        // Try to deserialize with wrapper first, then without
        var output = await TryDeserializeMessage(message);

        _logger.LogInformation("C# Queue trigger function processing: {OutputCount} items", output?.Count);

        if (output is null)
        {
            _logger.LogInformation("Output is null. C# Queue trigger function processed and finishing");
            return;
        }

        try
        {
            if (output.ContainsKey(TerraformVariables.PipelineRunId))
            {
                await ProcessTerraformInputVariables(output);
            }
            else
            { 
                await ProcessTerraformOutputVariables(output);            
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error processing output variable {OutputVariable}", output);
            throw;
        }

        await ProcessPostTerraformTriggers(output);

        _logger.LogInformation("C# Queue trigger function finished");
    }

    private async Task<Dictionary<string, TerraformOutputVariable>?> TryDeserializeMessage(ServiceBusReceivedMessage message)
    {
        try
        {
            var messageBody = message.Body.ToString();
            var deserializeOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            // Check if the message has the wrapper
            var messageDoc = System.Text.Json.JsonDocument.Parse(messageBody);
            var hasMessageWrapper = messageDoc.RootElement.TryGetProperty("message", out _);

            string messageToDeserialize;
            if (hasMessageWrapper)
            {
                // Already has wrapper, use as-is
                messageToDeserialize = messageBody;
            }
            else
            {
                // Add the wrapper
                messageToDeserialize = $"{{\"message\":{messageBody}}}";
                _logger.LogInformation("Message did not have wrapper, added wrapper before deserialization");
            }

            // Parse and deserialize
            var messageEnvelope = System.Text.Json.JsonDocument.Parse(messageToDeserialize);
            messageEnvelope.RootElement.TryGetProperty("message", out var messageContent);
            var content = messageContent.GetRawText();
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogError("Message content is empty");
                return null;
            }
            var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, TerraformOutputVariable>>(content, deserializeOptions);
            if (dict?.Count == 0)
                return null;
            return dict;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize message body");
            return null;
        }
    }

    internal async Task ProcessTerraformInputVariables(Dictionary<string, TerraformOutputVariable> output)
    {
        if (!TryGetRequiredValue(output, TerraformVariables.ProjectAcronym, out var projectAcronym) ||
            !TryGetRequiredValue(output, TerraformVariables.PipelineRunId, out var pipelineRunIdValue))
        {
            _logger.LogWarning("Terraform input message is missing required project or pipeline keys; skipping processing");
            return;
        }

        var pipelineId = int.Parse(pipelineRunIdValue);
        var projectAcronymValue = projectAcronym;

        var projects = await projectDbContext.Projects            
            .ToListAsync();
        var resourcesall = await projectDbContext.Project_Resources2
            .ToListAsync();

        var resources = (await projectDbContext.Projects
            .Where(p => p.Project_Acronym_CD == projectAcronymValue)
            .ToListAsync())
            .SelectMany(p => p.Resources.Where(r => r.PipelineId == null && TerraformStatus.RequestedOrInProcessOf(r.Status)));

        if (resources is null || !resources.Any())
        {
            _logger.LogError("No current resources found for {projectAcronym} that need to be created or deleted", projectAcronymValue);
            throw new Exception($"Project not found for acronym {projectAcronymValue}");
        }

        // Get unique RequestedAt dates and find the earliest one
        var uniqueRequestedAtDates = resources
            .Select(r => r.RequestedAt)
            .Distinct()
            .ToList();

        if (uniqueRequestedAtDates.Any())
        {
            var earliestRequestedAt = uniqueRequestedAtDates.Min();
            _logger.LogInformation("Earliest RequestedAt date found: {EarliestRequestedAt}", earliestRequestedAt);

            // Update resources with the earliest RequestedAt date
            var resourcesToUpdate = resources
                .Where(r => r.RequestedAt == earliestRequestedAt)
                .ToList();

            foreach (var resource in resourcesToUpdate)
            {
                resource.PipelineId = pipelineId;
            }

            _logger.LogInformation("Updated {ResourceCount} resources with PipelineId {PipelineId}", 
                resourcesToUpdate.Count, pipelineId);
            
            await projectDbContext.SaveChangesAsync();
        }
        else
        {
            _logger.LogInformation("No RequestedAt dates found in the resources");
        }

        _logger.LogInformation("Retrieved {ResourceCount} project resources for project {ProjectAcronym}", 
            resources.Count(), projectAcronymValue);
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

        if (!TryGetRequiredValue(output, TerraformVariables.OutputWorkspaceVersion, out var workspaceVersionValue) ||
            !TryGetRequiredValue(output, TerraformVariables.OutputProjectAcronym, out var projectAcronymValue))
        {
            _logger.LogWarning("Terraform output is missing project or version values; skipping post-terraform triggers");
            return;
        }

        var projectVersionString = workspaceVersionValue;

        // exclude the first character, which is a v
        var projectVersion = new Version(projectVersionString[1..]);

        // double check it's above version 2.13.0
        if (projectVersion < new Version(2, 13, 0))
        {
            _logger.LogInformation("Project version is below 2.13.0, skipping post terraform triggers");
            return;
        }

        // handle external user permissions
        var projectAcronym = output[TerraformVariables.OutputProjectAcronym];
        var project = await projectDbContext.Projects
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronymValue);

        if (project is null)
        {
            _logger.LogError("Project not found for acronym {ProjectId}", projectAcronymValue);
            throw new Exception($"Project not found for acronym {projectAcronymValue}");
        }

        _logger.LogInformation("Processing user updates to external permissions for project {ProjectAcronym}",
            projectAcronymValue);
        var workspaceDefinition =
            await resourceMessagingService.CreateWorkspaceDefinition(project.Project_Acronym_CD,
                TerraformOutputHandlerName);
        await resourceMessagingService.QueueRBACSync(workspaceDefinition);
        _logger.LogInformation(
            "Processing complete for user updates to external permissions for project {ProjectAcronym}",
            projectAcronymValue);
    }

    private static SemaphoreSlim semaphore = new(1, 1);

    private async Task ProcessTerraformOutputVariables(
        IReadOnlyDictionary<string, TerraformOutputVariable> outputVariables)
    {
        try
        {
            if (semaphore.CurrentCount == 0)
            {
                _logger.LogInformation("Semaphore is locked, waiting for release");
            }

            await semaphore.WaitAsync();

            if (!TryGetRequiredValue(outputVariables, TerraformVariables.OutputProjectAcronym, out _))
            {
                _logger.LogWarning("Terraform output is missing required project_cd; skipping processing");
                return;
            }

            using var transactionScope =
                new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
            await ProcessWorkspaceStatus(outputVariables);
            await ProcessAzureStorageBlob(outputVariables);
            await ProcessAzureDatabricks(outputVariables);
            await ProcessAzureWebApp(outputVariables);
            await ProcessAzurePostgres(outputVariables);
            await UpdateProjectVariables(outputVariables);
            transactionScope.Complete();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error processing output variables");
            throw;
        }
        finally
        {
            semaphore.Release();
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

        var projectResource = await GetProjectResource(outputVariables,
            TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureAppService));

        if (azureAppServiceStatus == TerraformStatus.Completed)
        {
            var appServiceId = outputVariables[TerraformVariables.OutputAzureAppServiceId];
            var appServiceHostName = outputVariables[TerraformVariables.OutputAzureAppServiceHostName];
            var appServiceRg = outputVariables[TerraformVariables.OutputAzureResourceGroupName];

            var jsonContent = new JsonObject
            {
                ["app_service_id"] = appServiceId.Value,
                ["app_service_hostname"] = appServiceHostName.Value,
                ["app_service_rg"] = appServiceRg.Value
            };

            projectResource.CreatedAt ??= DateTime.UtcNow;
            projectResource.JsonContent = jsonContent.ToString();

            projectResource.Project.WebApp_URL = appServiceHostName.Value;
            projectResource.Project.WebAppEnabled = true;
        }

        projectResource.Status = azureAppServiceStatus;
        projectResource.UpdatedAt = DateTime.UtcNow;

        await projectDbContext.SaveChangesAsync();
    }

    internal async Task ProcessAzureDatabricks(IReadOnlyDictionary<string, TerraformOutputVariable> outputVariables)
    {
        if (!outputVariables.ContainsKey(TerraformVariables.OutputAzureDatabricksStatus))
        {
            _logger.LogInformation("Azure Databricks status not found in output variables");
            return;
        }

        var databricksStatus = GetStatusMapping(outputVariables[TerraformVariables.OutputAzureDatabricksStatus].Value);


        var projectResource = await GetProjectResource(outputVariables,
            TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureDatabricks));

        if (databricksStatus == TerraformStatus.Completed)
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

            projectResource.CreatedAt ??= DateTime.UtcNow;
            projectResource.JsonContent = jsonContent.ToString();
        }

        projectResource.Status = databricksStatus;
        projectResource.UpdatedAt = DateTime.UtcNow;

        await projectDbContext.SaveChangesAsync();
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

        var projectResource = await GetProjectResource(outputVariables,
            TerraformTemplate.GetTerraformServiceType(TerraformTemplate.NewProjectTemplate));

        if (storageBlobStatus == TerraformStatus.Completed)
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

            projectResource.CreatedAt ??= DateTime.UtcNow;
            projectResource.JsonContent = jsonContent.ToString();
            projectResource.InputJsonContent = inputJsonContent.ToString();
        }

        projectResource.Status = storageBlobStatus;
        projectResource.UpdatedAt = DateTime.UtcNow;

        await projectDbContext.SaveChangesAsync();
    }

    internal async Task ProcessAzurePostgres(IReadOnlyDictionary<string, TerraformOutputVariable> outputVariables)
    {
        if (!outputVariables.ContainsKey(TerraformVariables.OutputAzurePostgresStatus))
        {
            _logger.LogInformation("Azure Postgres status not found in output variables");
            return;
        }

        var postgresStatus = GetStatusMapping(outputVariables[TerraformVariables.OutputAzurePostgresStatus].Value);


        var projectResource = await GetProjectResource(outputVariables,
            TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzurePostgres));

        if (postgresStatus == TerraformStatus.Completed)
        {
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
                [PostgresConfiguration.PGSQL_JSON_SERVER_NAME] = postgresServerName.Value
            };

            projectResource.CreatedAt ??= DateTime.UtcNow;
            projectResource.JsonContent = jsonContent.ToString();
        }

        projectResource.Status = postgresStatus;
        projectResource.UpdatedAt = DateTime.UtcNow;

        await projectDbContext.SaveChangesAsync();
    }

    private static string GetStatusMapping(string value)
    {
        return value switch
        {
            "completed" => TerraformStatus.Completed,
            "in_progress" => TerraformStatus.InProgress,
            "deleted" => TerraformStatus.Deleted,
            "failed" => TerraformStatus.Failed,
            _ => TerraformStatus.Missing
        };
    }

    private static bool TryGetRequiredValue(IReadOnlyDictionary<string, TerraformOutputVariable> outputVariables, string key, out string value)
    {
        value = string.Empty;

        if (!outputVariables.TryGetValue(key, out var terraformOutputVariable) ||
            terraformOutputVariable is null ||
            string.IsNullOrWhiteSpace(terraformOutputVariable.Value))
        {
            return false;
        }

        value = terraformOutputVariable.Value;
        return true;
    }

    internal async Task ProcessWorkspaceStatus(IReadOnlyDictionary<string, TerraformOutputVariable> outputVariables)
    {
        if (!TryGetRequiredValue(outputVariables, TerraformVariables.OutputProjectAcronym, out var projectAcronymValue))
        {
            _logger.LogWarning("Terraform output is missing project_cd; skipping workspace status processing");
            return;
        }

        var project = await projectDbContext.Projects
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronymValue);

        if (project == null)
        {
            _logger.LogError("Project not found for acronym {ProjectId}", projectAcronymValue);
            throw new Exception($"Project not found for acronym {projectAcronymValue}");
        }

        if (!TryGetRequiredValue(outputVariables, TerraformVariables.OutputNewProjectTemplate, out var resourceGroupStatusValue))
        {
            _logger.LogWarning("Terraform output is missing {OutputKey}; skipping workspace status processing",
                TerraformVariables.OutputNewProjectTemplate);
            return;
        }

        var resourceGroupStatus = GetStatusMapping(resourceGroupStatusValue);
        var projectResource = await GetProjectResource(outputVariables,
            TerraformTemplate.GetTerraformServiceType(TerraformTemplate.NewProjectTemplate));
        
        if (resourceGroupStatus == TerraformStatus.Completed)
        {
            if (!TryGetRequiredValue(outputVariables, TerraformVariables.OutputAzureResourceGroupName, out var resourceGroupName))
            {
                _logger.LogWarning("Terraform output is missing {OutputKey}; skipping resource group update",
                    TerraformVariables.OutputAzureResourceGroupName);
                return;
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

            var jsonContent = new JsonObject
            {
                ["resource_group_name"] = resourceGroupName
            };

            project.IsVersionUpdateRequested = false;
            projectResource.JsonContent = jsonContent.ToString();
            projectResource.CreatedAt ??= DateTime.UtcNow;
        }

        if (resourceGroupStatus == TerraformStatus.Deleted)
        {           
            project.Deleted_DT = DateTime.UtcNow;            
        }

        projectResource.Status = resourceGroupStatus;
        projectResource.UpdatedAt = DateTime.UtcNow;

        await projectDbContext.SaveChangesAsync();
    }

    internal async Task UpdateProjectVariables(IReadOnlyDictionary<string, TerraformOutputVariable> outputVariables)
    {
        if (!TryGetRequiredValue(outputVariables, TerraformVariables.OutputProjectAcronym, out var projectAcronymValue))
        {
            _logger.LogWarning("Terraform output is missing project_cd; skipping project variable update");
            return;
        }

        var project = await projectDbContext.Projects
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronymValue);

        if (project == null)
        {
            _logger.LogError("Project not found for acronym {ProjectId}", projectAcronymValue);
            throw new Exception($"Project not found for acronym {projectAcronymValue}");
        }

        project.IsVersionUpdateRequested = false;
        await projectDbContext.SaveChangesAsync();

    }

    private async Task<Project_Resources2> GetProjectResource(
        IReadOnlyDictionary<string, TerraformOutputVariable> outputVariables, string terraformServiceType)
    {
        if (!TryGetRequiredValue(outputVariables, TerraformVariables.OutputProjectAcronym, out var projectAcronymValue))
        {
            _logger.LogWarning("Terraform output is missing project_cd; unable to resolve project resource");
            throw new InvalidOperationException("Terraform output is missing project_cd");
        }

        var project = await projectDbContext.Projects
            .Include(p => p.Resources)
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronymValue);

        if (project is null)
        {
            _logger.LogError("Project not found for acronym {ProjectId}", projectAcronymValue);
            throw new Exception($"Project not found for acronym {projectAcronymValue}");
        }

        var projectResource = project.Resources
            .FirstOrDefault(x => x.ResourceType == terraformServiceType);

        if (projectResource is null)
        {
            _logger.LogError(
                "Project resource not found for project acronym {ProjectAcronymValue} and service type {TerraformServiceType}",
                projectAcronymValue, terraformServiceType);
            throw new Exception(
                $"Project resource not found for project acronym {projectAcronymValue} and service type {terraformServiceType}");
        }

        return projectResource;
    }
}
