using System.Linq.Dynamic.Core;
using Datahub.Application.Commands;
using Datahub.Application.Services;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Data;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Services.Projects;
using Datahub.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services;

public class ProjectUserManagementService : IProjectUserManagementService
{
    private readonly IUserInformationService _userInformationService;
    private readonly IMSGraphService _msGraphService;
    private readonly IRequestManagementService _requestManagementService;
    private readonly ILogger<ProjectUserManagementService> _logger;
    private readonly IDbContextFactory<DatahubProjectDBContext> _contextFactory;
    private readonly IUserEnrollmentService _userEnrollmentService;
    private readonly IDatahubAuditingService _datahubAuditingService;
    private readonly IResourceMessagingService _resourceMessagingService;

    public ProjectUserManagementService(
        ILogger<ProjectUserManagementService> logger,
        IDbContextFactory<DatahubProjectDBContext> contextFactory,
        IUserInformationService userInformationService,
        IMSGraphService msGraphService,
        IRequestManagementService requestManagementService,
        IResourceMessagingService resourceMessagingService,
        IUserEnrollmentService userEnrollmentService,
        IDatahubAuditingService datahubAuditingService)
    {
        _userInformationService = userInformationService;
        _msGraphService = msGraphService;
        _requestManagementService = requestManagementService;
        _logger = logger;
        _contextFactory = contextFactory;
        _userEnrollmentService = userEnrollmentService;
        _datahubAuditingService = datahubAuditingService;
        _resourceMessagingService = resourceMessagingService;
    }

    public async Task<bool> ProcessProjectUserCommandsAsync(List<ProjectUserUpdateCommand> projectUserUpdateCommands,
        List<ProjectUserAddUserCommand> projectUserAddUserCommands, string requesterUserId)
    {
        // if there are no commands, return true
        if (!projectUserUpdateCommands.Any() && !projectUserAddUserCommands.Any())
        {
            return true;
        }

        try
        {
            if (projectUserAddUserCommands.Any())
            {
                await AddNewUsersToProjectAsync(projectUserAddUserCommands);
            }

            if (projectUserUpdateCommands.Any())
            {
                await UpdateProjectUsersAsync(projectUserUpdateCommands);
            }

            await PropagateUserUpdatesToExternalPermissions(projectUserUpdateCommands, projectUserAddUserCommands, requesterUserId);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating project users");
            return false;
        }
    }

    private async Task PropagateUserUpdatesToExternalPermissions(IEnumerable<ProjectUserUpdateCommand> projectUserUpdateCommands,
        IEnumerable<ProjectUserAddUserCommand> projectUserAddUserCommands, string requesterUserId)
    {
        // get all the distinct projects that have been modified
        var projectAcronyms = projectUserUpdateCommands
            .Select(p => p.ProjectUser.Project.Project_Acronym_CD)
            .Distinct()
            .Union(projectUserAddUserCommands
                .Select(p => p.ProjectAcronym)
                .Distinct())
            .ToList();
        
        var currentUser = await _userInformationService.GetCurrentPortalUserAsync();

        // update each project
        foreach (var projectAcronym in projectAcronyms)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var project = await context.Projects
                .Include(p => p.Users)
                .ThenInclude(u => u.PortalUser)
                .FirstAsync(p => p.Project_Acronym_CD == projectAcronym);
            
            project.Last_Updated_DT = DateTime.Now;
            project.Last_Updated_UserId = requesterUserId;
            context.Projects.Update(project);
            await context.SaveChangesAsync();

            await _requestManagementService.HandleUserUpdatesToExternalPermissions(project, currentUser);
        }
    }

    private async Task UpdateProjectUsersAsync(List<ProjectUserUpdateCommand> projectUserUpdateCommands)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        foreach (var projectUserUpdateCommand in projectUserUpdateCommands)
        {
            var userToUpdate = await context.Project_Users
                .FirstOrDefaultAsync(pu => pu.ProjectUser_ID == projectUserUpdateCommand.ProjectUser.ProjectUser_ID);

            if (userToUpdate == null)
            {
                throw new InvalidOperationException("Cannot update a user that is not already a member of the project");
            }

            //if (projectUserUpdateCommand.NewRoleId == (int)Project_Role.RoleNames.Remove)
            //{
            //    context.Project_Users.Remove(userToUpdate);
            //}
            //else
            //{
                userToUpdate.RoleId = projectUserUpdateCommand.NewRoleId;
                context.Update(userToUpdate);
            //}
        }

        await context.TrackSaveChangesAsync(_datahubAuditingService);
    }

    private async Task AddNewUsersToProjectAsync(List<ProjectUserAddUserCommand> projectUserAddUserCommands)
    {
        foreach (var projectUserAddUserCommand in projectUserAddUserCommands)
        {
            if (projectUserAddUserCommand.RoleId == (int)Project_Role.RoleNames.Remove)
            {
                throw new InvalidOperationException("Cannot remove a user that is not already a member of the project");
            }

            var currentUser = await _userInformationService.GetCurrentPortalUserAsync();

            if (projectUserAddUserCommand.GraphGuid == ProjectUserAddUserCommand.NEW_USER_GUID)
            {
                projectUserAddUserCommand.GraphGuid =
                    await _userEnrollmentService.SendUserDatahubPortalInvite(projectUserAddUserCommand.Email,
                        currentUser.DisplayName);
                await _userInformationService.CreatePortalUserAsync(projectUserAddUserCommand.GraphGuid);
            }

            var portalUser = await _userInformationService.GetPortalUserAsync(projectUserAddUserCommand.GraphGuid);

            await using var context = await _contextFactory.CreateDbContextAsync();
            var project = await context.Projects
                .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectUserAddUserCommand.ProjectAcronym);

            // Verify that the project exists
            if (project == null)
            {
                _logger.LogError("Project {ProjectAcronym} not found", projectUserAddUserCommand.ProjectAcronym);
                throw new ProjectNotFoundException($"Project {projectUserAddUserCommand.ProjectAcronym} not found");
            }

            var projectUser = await context.Project_Users
                .FirstOrDefaultAsync(u => u.Project.Project_Acronym_CD == projectUserAddUserCommand.ProjectAcronym
                                          && u.PortalUser.GraphGuid == projectUserAddUserCommand.GraphGuid);

            // Double check that the user is not already a member of the project
            if (projectUser is not null)
            {
                projectUser.RoleId = projectUserAddUserCommand.RoleId;
                _logger.LogInformation("Changing role of removed user {GraphGuid} of project {ProjectAcronym}",
                    projectUserAddUserCommand.GraphGuid, projectUserAddUserCommand.ProjectAcronym);
            }
            else
            {

                var newProjectUser = new Datahub_Project_User()
                {
                    Project_ID = project.Project_ID,
                    PortalUserId = portalUser.Id,
                    ApprovedPortalUserId = currentUser.Id,
                    Approved_DT = DateTime.UtcNow,
                    RoleId = projectUserAddUserCommand.RoleId,
                };
                await context.Project_Users.AddAsync(newProjectUser);
            }
           
            // If current user is not the user being added to the project
            if (projectUser?.PortalUserId != currentUser.Id)
            {
                context.Attach(currentUser);
            }

            context.Attach(portalUser);

            await context.TrackSaveChangesAsync(_datahubAuditingService);
        }
    }

    public async Task<List<Datahub_Project_User>> GetProjectUsersAsync(string projectAcronym)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Project_Users
            .AsNoTracking()
            .Include(u => u.Project)
            .Include(u => u.PortalUser)
            .Include(u => u.Role)
            .Where(u => u.Project.Project_Acronym_CD == projectAcronym)
            .Where(u => u.PortalUser != null)
            .ToListAsync();
    }

    public async Task<List<string>> GetProjectListForPortalUser(int portalUserId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var projectAcronyms = await context.Project_Users
            .Include(pu => pu.Project)
            .Where(pu => pu.PortalUserId == portalUserId)
            .Select(pu => pu.Project.Project_Acronym_CD)
            .ToListAsync();

        return projectAcronyms;
    }

    public async Task<Datahub_Project_User?> GetProjectLeadAsync(string projectAcronym)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var users = await GetProjectUsersAsync(projectAcronym);
        var admin = users?.Where(u => RoleConstants.GetRoleConstants(u.Role) == RoleConstants.WORKSPACE_LEAD_SUFFIX);

        return admin?.FirstOrDefault();
    }

    public async Task<bool> RunWorkspaceSync(string projectAcronym)
    {
        try
        {
            var workspaceDefinition = await _resourceMessagingService.GetWorkspaceDefinition(projectAcronym);
            await _resourceMessagingService.SendToUserQueue(workspaceDefinition);

            _logger.LogInformation($"Triggered workspace sync for {projectAcronym}" );
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error triggerring workspace sync for {projectAcronym}");
            return false;
        }
    }
}