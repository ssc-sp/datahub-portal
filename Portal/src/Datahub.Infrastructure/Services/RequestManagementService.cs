using Datahub.Core.Services.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Datahub.Application.Services;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Projects;
using Datahub.Shared.Entities;
using Datahub.Shared.Enums;
using Datahub.Core.Model.Context;
using Datahub.Shared;

namespace Datahub.Infrastructure.Services;

public class RequestManagementService(
    ILogger<RequestManagementService> logger,
    IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
    IDatahubAuditingService datahubAuditingService,
    IResourceMessagingService resourceMessagingService,
    IWorkspaceVersionService workspaceVersionService)
    : IRequestManagementService
{
    public async Task HandleUserUpdatesToExternalPermissions(Datahub_Project project, PortalUser currentPortalUser)
    {
        var workspaceDefinition =
            await resourceMessagingService.GetWorkspaceDefinition(project.Project_Acronym_CD, currentPortalUser.Email);
        await resourceMessagingService.SendToUserQueue(workspaceDefinition);
    }

    /// <summary>
    /// Scaffold local changes for the given project asynchronously.
    /// </summary>
    /// <param name="project">The project to scaffold for</param>
    /// <param name="requestingUser">The requesting user</param>
    /// <param name="requestedTemplate">The template to scaffold</param>
    /// <param name="ctx">The db context to use</param>
    public async Task ScaffoldLocalChanges(Datahub_Project project, PortalUser requestingUser,
        TerraformTemplate requestedTemplate,
        DatahubProjectDBContext ctx,
        DateTime requestTime)
    {
        ctx.Projects.Attach(project);

        await ctx.Entry(project)
            .Collection(p => p.Resources)
            .LoadAsync();

        var resource = project.Resources
            .FirstOrDefault(r => r.ResourceType == TerraformTemplate.GetTerraformServiceType(requestedTemplate.Name));

        if (resource is not null)
        {
            if (requestedTemplate.Status == TerraformStatus.DeleteRequested)
            {
                resource.Status = TerraformStatus.DeleteRequested;
            }
            else
            {
                resource.Status = TerraformStatus.ExistsOrInAnyProcess(resource.Status)
                    ? resource.Status
                    : requestedTemplate.Status;
            }

            ctx.Project_Resources2.Update(resource);
        }
        else
        {
            resource = new Project_Resources2
            {
                ProjectId = project.Project_ID,
                RequestedById = requestingUser.Id,
                ResourceType = TerraformTemplate.GetTerraformServiceType(requestedTemplate.Name),
                Status = requestedTemplate.Status,
                RequestedAt = requestTime
            };

            await ctx.Project_Resources2.AddAsync(resource);

            if (requestedTemplate.Name == TerraformTemplate.NewProjectTemplate)
            { 
                project.Version = await workspaceVersionService.GetLatestVersionAsync();
            }
        }
    }

    /// <summary>
    /// Processes the given request for a specific project asynchronously.
    /// </summary>
    /// <param name="project">The project for which the request is being processed.</param>
    /// <param name="requestingUser">The user making the request.</param>
    /// <param name="requestedTemplate">The template requested for the project.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ProcessRequest(Datahub_Project project, PortalUser requestingUser,
        TerraformTemplate requestedTemplate)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        await ScaffoldLocalChanges(project, requestingUser, requestedTemplate, ctx);
        await ctx.TrackSaveChangesAsync(datahubAuditingService);
    }

    /// <summary>
    /// Handles a Terraform request asynchronously.
    /// </summary>
    /// <param name="datahubProject">The Datahub project.</param>
    /// <param name="terraformTemplate">The Terraform template.</param>
    /// <param name="requestingUser">The user making the request.</param>
    /// <returns>True if the Terraform request was handled successfully; otherwise, false.</returns>
    public async Task<bool> HandleTerraformRequestServiceAsync(Datahub_Project datahubProject,
        TerraformTemplate terraformTemplate,
        PortalUser requestingUser)
    {
        try
        {
            await using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = await ctx.Projects
                .Include(p => p.Resources)
                .Include(p => p.UserRoles)
                .ThenInclude(u => u.PortalUser)
                .FirstOrDefaultAsync(p => p.Project_ID == datahubProject.Project_ID);

            if (project == null)
            {
                return false;
            }

            if (terraformTemplate.Status == TerraformStatus.DeleteRequested)
            {
                await ProcessRequest(project, requestingUser, terraformTemplate);
            }
            else
            {
                var dependencyTemplates = TerraformTemplate.GetDependenciesToCreate(terraformTemplate.Name);
                if (terraformTemplate.Name != TerraformTemplate.VariableUpdate)
                {
                    await ProcessRequest(project, requestingUser, terraformTemplate);
                    foreach (var template in dependencyTemplates)
                    {
                        await ProcessRequest(project, requestingUser, template);
                    }
                }
            
            }

            var workspaceDefinition =
                await resourceMessagingService.GetWorkspaceDefinition(project.Project_Acronym_CD,
                    requestingUser.Email);

            
            await resourceMessagingService.SendToTerraformQueue(workspaceDefinition);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating resource {@TerraformTemplate} for {DatahubProjectProjectAcronymCd}",
                terraformTemplate, datahubProject.Project_Acronym_CD);
            return false;
        }
    }



    public async Task<bool> TriggerBuildVersionUpdates(string versionTag, string email)
    {
        // Parse the version tag and extract major and minor versions  
        try
        {
            var parsedVersion = Version.Parse(versionTag.TrimStart('v'));
            var parsedMajorMinor = $"v{parsedVersion.Major}.{parsedVersion.Minor}";

            await using var db = await dbContextFactory.CreateDbContextAsync();
            var currentVersionProjects = await db.Projects
                .Where(p => p.Version.StartsWith(parsedMajorMinor))
                .ToListAsync();

            if (currentVersionProjects.Any())
            {
                foreach (var project in currentVersionProjects)
                {
                    var workspaceDefinition = await resourceMessagingService.GetWorkspaceDefinition(project.Project_Acronym_CD, email);
                    var parsedProjectVersion = workspaceDefinition.Workspace.Version.TrimStart('v');

                    if (Version.Parse(parsedProjectVersion) >= parsedVersion)
                    {
                        continue;
                    }
                    await SendVersionUpdateToQueueAsync(versionTag, workspaceDefinition);
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error triggering green light changes for version {VersionTag} requseted by {Email}", versionTag, email);
            return false;
        }
    }

    public async Task SendVersionUpdateToQueueAsync(string versionTag, WorkspaceDefinition workspaceDefinition)
    {
        workspaceDefinition.Workspace.Version = versionTag;
        workspaceDefinition.UpdateWorkspaceVersion = true;
        await resourceMessagingService.SendToTerraformQueue(workspaceDefinition);
    }                    
    public static Role GetTerraformUserRole(UserRoleLinks projectUser)
    {
        return projectUser.RoleId switch
        {
            (int)Project_Role.RoleNames.Removed => Role.Removed,
            (int)Project_Role.RoleNames.WorkspaceLead => Role.Owner,
            (int)Project_Role.RoleNames.Admin => Role.Admin,
            (int)Project_Role.RoleNames.Collaborator => Role.User,
            (int)Project_Role.RoleNames.Guest => Role.Guest,
            _ => Role.Guest
        };
    }
}