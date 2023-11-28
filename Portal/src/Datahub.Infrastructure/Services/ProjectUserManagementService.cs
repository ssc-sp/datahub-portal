using System.Transactions;
using Datahub.Application.Commands;
using Datahub.Application.Services;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Core.Services;
using Datahub.Core.Services.Projects;
using Datahub.Core.Services.Security;
using Datahub.Core.Services.UserManagement;
using Datahub.Shared.Entities;
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
    private readonly ServiceAuthManager _serviceAuthManager;
    private readonly IUserEnrollmentService _userEnrollmentService;
    private readonly IDatahubAuditingService _datahubAuditingService;

    public ProjectUserManagementService(
        ILogger<ProjectUserManagementService> logger,
        IDbContextFactory<DatahubProjectDBContext> contextFactory,
        IUserInformationService userInformationService,
        IMSGraphService msGraphService,
        IRequestManagementService requestManagementService,
        ServiceAuthManager serviceAuthManager,
        IUserEnrollmentService userEnrollmentService,
        IDatahubAuditingService datahubAuditingService)
    {
        _serviceAuthManager = serviceAuthManager;
        _userInformationService = userInformationService;
        _msGraphService = msGraphService;
        _requestManagementService = requestManagementService;
        _logger = logger;
        _contextFactory = contextFactory;
        _userEnrollmentService = userEnrollmentService;
        _datahubAuditingService = datahubAuditingService;
    }

    public async Task<bool> ProcessProjectUserCommandsAsync(List<ProjectUserUpdateCommand> projectUserUpdateCommands,
        List<ProjectUserAddUserCommand> projectUserAddUserCommands)
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

            await PropagateUserUpdatesToExternalPermissions(projectUserUpdateCommands, projectUserAddUserCommands);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating project users");
            return false;
        }
    }

    private async Task PropagateUserUpdatesToExternalPermissions(IEnumerable<ProjectUserUpdateCommand> projectUserUpdateCommands,
        IEnumerable<ProjectUserAddUserCommand> projectUserAddUserCommands)
    {
        // get all the distinct projects that have been modified
        var projectAcronyms = projectUserUpdateCommands
            .Select(p => p.ProjectUser.Project.Project_Acronym_CD)
            .Distinct()
            .Union(projectUserAddUserCommands
                .Select(p => p.ProjectAcronym)
                .Distinct())
            .ToList();

        // update each project
        foreach (var projectAcronym in projectAcronyms)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var project = await context.Projects
                .AsNoTracking()
                .Include(p => p.Users)
                .ThenInclude(u => u.PortalUser)
                .FirstAsync(p => p.Project_Acronym_CD == projectAcronym);

            await _requestManagementService.HandleUserUpdatesToExternalPermissions(project);
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
                _logger.LogError("User {GraphGuid} is already a member of project {ProjectAcronym}",
                    projectUserAddUserCommand.GraphGuid, projectUserAddUserCommand.ProjectAcronym);
                throw new InvalidOperationException(
                    $"User {projectUserAddUserCommand.GraphGuid} is already a member of project {projectUserAddUserCommand.ProjectAcronym}");
            }

            var newProjectUser = new Datahub_Project_User()
            {
                Project = project,
                PortalUser = portalUser,
                ApprovedPortalUser = currentUser,
                Approved_DT = DateTime.UtcNow,
                RoleId = projectUserAddUserCommand.RoleId,
            };

            context.Attach(currentUser);
            context.Attach(portalUser);

            await context.Project_Users.AddAsync(newProjectUser);
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
}