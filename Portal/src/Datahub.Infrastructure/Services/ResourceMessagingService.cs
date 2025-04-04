using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Utils;
using Datahub.Infrastructure.Extensions;
using Datahub.Shared;
using Datahub.Shared.Configuration;
using Datahub.Shared.Entities;
using Datahub.Shared.Exceptions;
using Foundatio.Queues;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Infrastructure.Services;

public class ResourceMessagingService(
    IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
    ISendEndpointProvider sendEndpointProvider,
    WorkspaceVersionService workspaceVersionService)
    : IResourceMessagingService
{
    public async Task SendToTerraformQueue(WorkspaceDefinition workspaceDefinition)
    {
        await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.ResourceRunRequestQueueName, workspaceDefinition);
    }

    public async Task SendToUserQueue(WorkspaceDefinition workspaceDefinition)
    {
        await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.UserRunRequestQueueName, workspaceDefinition);
    }

    public async Task<WorkspaceDefinition> GetWorkspaceDefinition(string projectAcronym, string? requestingUserEmail = "system-generated", string? cbrId = null)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var project = await ctx.Projects
            .AsNoTracking()
            .Include(p => p.Users)
            .ThenInclude(u => u.PortalUser)
            .Include(p => p.Resources)
            .Include(p => p.DatahubAzureSubscription)
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym);
        
        if(project == null)
        {
            throw new ProjectNotFoundException($"Project {projectAcronym} not found.");
        }
        // TODO: Add handling for CBRs

        var users = project.Users
            .Where(u => u.PortalUser != null)
            .Select(u => new TerraformUser
            {
                ObjectId = u.PortalUser.GraphGuid, 
                Email = u.PortalUser.Email, 
                Role = RequestManagementService.GetTerraformUserRole(u)
            })
            .ToList();

        var workspace = project.ToResourceWorkspace(users);
        var templates = project.Resources
            .Where(r => r.ResourceType != TerraformTemplate.VariableUpdate && !TerraformStatus.DeletedOrInProcessOf(r.Status))
            .Select(r => r.ToTerraformTemplate())
            .ToList();


        workspace.Version = workspace.Version == "latest" ? await workspaceVersionService.GetLatestVersion() : workspace.Version;


        return new WorkspaceDefinition
        {
            Workspace = workspace,
            Templates = templates,
            AppData = new WorkspaceAppData
            {
                DatabricksHostUrl = TerraformVariableExtraction.ExtractDatabricksUrl(project, null),
                AppServiceConfiguration = TerraformVariableExtraction.ExtractAppServiceConfiguration(project),
                PostgresConfiguration = TerraformVariableExtraction.ExtractPostgresConfiguration(project)
            },
            RequestingUserEmail = requestingUserEmail
        };
    }

    private string GetResourceNameSuffix(string templatetype, Datahub_Project project)
    {
        var resourceNumber = 0;

        //get total resources of template type
        switch (templatetype)
        {
            case TerraformTemplate.AzureAppService:
                resourceNumber = project.Resources.Count(r => r.ResourceType.Equals(TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureAppService)));
                break;
            case TerraformTemplate.AzurePostgres:
                resourceNumber = project.Resources.Count(r => r.ResourceType.Equals(TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzurePostgres)));
                break;
            default:
                throw new ArgumentException("Invalid template type", nameof(templatetype));
        }

        //get next iteration for suffix
        resourceNumber++;

        // format resourceNumber to three digits
        return resourceNumber.ToString("D3");

    }
}