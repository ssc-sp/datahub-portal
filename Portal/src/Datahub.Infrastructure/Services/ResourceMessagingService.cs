using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Utils;
using Datahub.Infrastructure.Extensions;
using Datahub.Shared.Configuration;
using Datahub.Shared.Entities;
using Datahub.Shared.Exceptions;
using Foundatio.Queues;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Infrastructure.Services;

public class ResourceMessagingService(
    IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
    ISendEndpointProvider sendEndpointProvider)
    : IResourceMessagingService
{
    public async Task SendToTerraformQueue(WorkspaceDefinition project)
    {
        await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.ResourceRunRequestQueueName, project);
    }

    public async Task SendToTerraformDeleteQueue(WorkspaceDefinition project, int projectId)
    {
        await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.ResourceDeleteRequestQueueName, project);
        
        // Update Deleted_DT on the project
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var projectToDeleted = await ctx.Projects
            .FirstOrDefaultAsync(p => p.Project_ID == projectId);
        if (projectToDeleted != null)
        {
            projectToDeleted.Deleted_DT = DateTime.UtcNow;
            ctx.Projects.Update(projectToDeleted);
            await ctx.SaveChangesAsync();
        }
    }

    public async Task SendToUserQueue(WorkspaceDefinition workspaceDefinition, string? connectionString = null, string? queueName = null)
    {
        await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.UserRunRequestQueueName, workspaceDefinition);
    }

    public async Task<WorkspaceDefinition> GetWorkspaceDefinition(string projectAcronym, string? requestingUserEmail = "system-generated")
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
            .Where(r => r.ResourceType != TerraformTemplate.VariableUpdate)
            .Select(r => r.ResourceType)
            .Select(TerraformTemplate.GetTemplateByName)
            .ToList();

        return new WorkspaceDefinition
        {
            Workspace = workspace,
            Templates = templates,
            AppData = new WorkspaceAppData
            {
                DatabricksHostUrl = TerraformVariableExtraction.ExtractDatabricksUrl(project),
                AppServiceConfiguration = TerraformVariableExtraction.ExtractAppServiceConfiguration(project)
            },
            RequestingUserEmail = requestingUserEmail,
        };
    }
}