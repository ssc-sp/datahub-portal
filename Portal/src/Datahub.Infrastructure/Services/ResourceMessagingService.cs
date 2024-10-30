using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Core.Model.Context;
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
    public async Task SendToTerraformQueue(WorkspaceDefinition workspaceDefinition)
    {
        await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.ResourceRunRequestQueueName, workspaceDefinition);
    }

    public async Task SendToUserQueue(WorkspaceDefinition workspaceDefinition)
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
            .Select(r => r.ToTerraformTemplate())
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