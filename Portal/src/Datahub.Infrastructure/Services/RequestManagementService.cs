using Datahub.Core.Model.Datahub;
using Datahub.Core.Services;
using Datahub.Core.Services.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Transactions;
using Datahub.Application.Services;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Projects;
using Datahub.Shared.Entities;
using Datahub.Shared.Enums;
using Datahub.Core.Model.Context;
using Datahub.Shared;

namespace Datahub.Infrastructure.Services;

public class RequestManagementService : IRequestManagementService
{
    private readonly ILogger<RequestManagementService> _logger;
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
    private readonly IDatahubAuditingService _datahubAuditingService;
    private readonly IResourceMessagingService _resourceMessagingService;

    public RequestManagementService(
        ILogger<RequestManagementService> logger,
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
        IDatahubAuditingService datahubAuditingService,
        IResourceMessagingService resourceMessagingService)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _datahubAuditingService = datahubAuditingService;
        _resourceMessagingService = resourceMessagingService;
    }


    public async Task HandleUserUpdatesToExternalPermissions(Datahub_Project project, PortalUser currentPortalUser)
    {
        var workspaceDefinition =
            await _resourceMessagingService.GetWorkspaceDefinition(project.Project_Acronym_CD, currentPortalUser.Email);
        await _resourceMessagingService.SendToUserQueue(workspaceDefinition);
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
        await using var ctx = await _dbContextFactory.CreateDbContextAsync();
        ctx.Projects.Attach(project);

        await ctx.Entry(project)
            .Collection(p => p.Resources)
            .LoadAsync();

        var resource = project.Resources
            .FirstOrDefault(r => r.ResourceType == TerraformTemplate.GetTerraformServiceType(requestedTemplate.Name));

        if (resource is not null)
        {
            resource.Status = requestedTemplate.Status;
        }
        else
        {
            resource = new Project_Resources2
            {
                ProjectId = project.Project_ID,
                RequestedById = requestingUser.Id,
                ResourceType = TerraformTemplate.GetTerraformServiceType(requestedTemplate.Name),
                Status = requestedTemplate.Status,
            };

            await ctx.Project_Resources2.AddAsync(resource);
        }

        await ctx.TrackSaveChangesAsync(_datahubAuditingService);
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
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();
            var project = await ctx.Projects
                .Include(p => p.Resources)
                .Include(p => p.Users)
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
                await _resourceMessagingService.GetWorkspaceDefinition(project.Project_Acronym_CD,
                    requestingUser.Email);

            await _resourceMessagingService.SendToTerraformQueue(workspaceDefinition);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating resource {TerraformTemplate} for {DatahubProjectProjectAcronymCd}",
                terraformTemplate, datahubProject.Project_Acronym_CD);
            return false;
        }
    }

    public static Role GetTerraformUserRole(Datahub_Project_User projectUser)
    {
        return projectUser.RoleId switch
        {
            (int)Project_Role.RoleNames.Remove => Role.Removed,
            (int)Project_Role.RoleNames.WorkspaceLead => Role.Owner,
            (int)Project_Role.RoleNames.Admin => Role.Admin,
            (int)Project_Role.RoleNames.Collaborator => Role.User,
            (int)Project_Role.RoleNames.Guest => Role.Guest,
            _ => Role.Guest
        };
    }
}