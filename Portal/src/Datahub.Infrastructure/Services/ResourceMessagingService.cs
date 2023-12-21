using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Core.Data.ResourceProvisioner;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Utils;
using Datahub.ProjectTools.Services;
using Datahub.Shared.Entities;
using Datahub.Shared.Exceptions;
using Foundatio.Queues;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Infrastructure.Services;

public class ResourceMessagingService : IResourceMessagingService
{
    private readonly DatahubPortalConfiguration _datahubPortalConfiguration;
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;

    public ResourceMessagingService(DatahubPortalConfiguration datahubPortalConfiguration,
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
    {
        _datahubPortalConfiguration = datahubPortalConfiguration;
        _dbContextFactory = dbContextFactory;
    }
    public async Task SendToTerraformQueue(WorkspaceDefinition project)
    {
        using IQueue<WorkspaceDefinition> queue = new AzureStorageQueue<WorkspaceDefinition>(new AzureStorageQueueOptions<WorkspaceDefinition>()
        {
            ConnectionString = _datahubPortalConfiguration.DatahubStorageQueue.ConnectionString,
            Name = _datahubPortalConfiguration.DatahubStorageQueue.QueueNames.ResourceRunRequest,
        });
        await queue.EnqueueAsync(project);
    }

    public async Task SendToTerraformDeleteQueue(WorkspaceDefinition project, int projectId)
    {
        using IQueue<WorkspaceDefinition> queue = new AzureStorageQueue<WorkspaceDefinition>(new AzureStorageQueueOptions<WorkspaceDefinition>()
        {
            ConnectionString = _datahubPortalConfiguration.DatahubStorageQueue.ConnectionString,
            Name = _datahubPortalConfiguration.DatahubStorageQueue.QueueNames.DeleteRunRequest,
        });
        
        await queue.EnqueueAsync(project);
        
        // Update Deleted_DT on the project
        using var ctx = await _dbContextFactory.CreateDbContextAsync();
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
        using IQueue<WorkspaceDefinition> queue = new AzureStorageQueue<WorkspaceDefinition>(
            new AzureStorageQueueOptions<WorkspaceDefinition>()
            {
                ConnectionString = connectionString ?? _datahubPortalConfiguration.DatahubStorageQueue.ConnectionString,
                Name = queueName ?? _datahubPortalConfiguration.DatahubStorageQueue.QueueNames.UserRunRequest,
            });
        await queue.EnqueueAsync(workspaceDefinition);
    }

    public async Task<WorkspaceDefinition> GetWorkspaceDefinition(string projectAcronym, string? requestingUserEmail = "system-generated")
    {
        await using var ctx = await _dbContextFactory.CreateDbContextAsync();
        var project = await ctx.Projects
            .AsNoTracking()
            .Include(p => p.Users)
            .ThenInclude(u => u.PortalUser)
            .Include(p => p.Resources)
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
                DatabricksHostUrl = TerraformVariableExtraction.ExtractDatabricksUrl(project)
            },
            RequestingUserEmail = requestingUserEmail,
        };
    }
}