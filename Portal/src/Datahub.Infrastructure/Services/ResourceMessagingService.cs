using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Core.Utils;
using Datahub.Infrastructure.Extensions;
using Datahub.Shared;
using Datahub.Shared.Configuration;
using Datahub.Shared.Entities;
using Datahub.Shared.Exceptions;
using Datahub.Core.Extensions;
using Foundatio.Queues;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Infrastructure.Services;

public class ResourceMessagingService(
    IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
    ISendEndpointProvider sendEndpointProvider,
    IWorkspaceVersionService workspaceVersionService)
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

    public async Task<WorkspaceDefinition> GetWorkspaceDefinition(string projectAcronym, string requestingUserEmail = "system-generated", string? cbrId = null)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var project = await ctx.Projects
            .AsNoTracking()
            .Include(p => p.UserRoles)
            .ThenInclude(u => u.PortalUser)
            .ThenInclude(u => u.EntraUser)
            .Include(p => p.Resources)
            .Include(p => p.DatahubAzureSubscription)
            .Include(p => p.ParentGCHostingBudget)
            .AsSingleQuery()
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym);

        if (project is null)
        {
            throw new ProjectNotFoundException($"Project {projectAcronym} not found.");
        }
        
        var entraUsers = project.UserRoles
            .Where(u => u.PortalUser != null && u.PortalUser.EntraUser != null)
            .Select(u => new TerraformUser
            {
                ObjectId = u.PortalUser!.EntraUser!.GraphGuid, 
                Email = u.PortalUser!.Email ?? throw new InvalidOperationException($"User {u.PortalUser!.EntraUser!.GraphGuid} email is missing"), 
                Role = RequestManagementService.GetTerraformUserRole(u)
            })
            .ToList();

        var workspace = project.ToResourceWorkspace(entraUsers);
        var templates = project.Resources
            .Where(r => r.ResourceType != TerraformTemplate.VariableUpdate && r.Status != TerraformStatus.Deleted)
            .Select(r => r.ToTerraformTemplate())
            .ToList();



        workspace.Version = workspace.Version == "latest" ? await workspaceVersionService.GetLatestVersionAsync() : workspace.Version;

        return new WorkspaceDefinition
        {
            Workspace = workspace,
            Templates = templates,
            AppData = new WorkspaceAppData
            {
                DatabricksHostUrl = TerraformVariableExtraction.ExtractDatabricksUrl(project, null),
                AppServiceConfiguration = TerraformVariableExtraction.ExtractAppServiceConfiguration(project),
                PostgresConfiguration = TerraformVariableExtraction.ExtractPostgresConfiguration(project),
                DatabricksConfiguration = TerraformVariableExtraction.ExtractDatabricksConfiguration(project)
            },
            RequestingUserEmail = requestingUserEmail,
            ResourceGroupName = project.GetResourceGroupName(),
            CBRID = project.ParentGCHostingBudget?.CBRID ?? string.Empty
        };
    }

    
}