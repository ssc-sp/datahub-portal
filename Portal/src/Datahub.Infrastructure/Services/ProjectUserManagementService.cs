using System.Transactions;
using Datahub.Application.Services;
using Datahub.Core.Data.Project;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services;
using Datahub.Core.Services.Projects;
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

    public ProjectUserManagementService(
        ILogger<ProjectUserManagementService> logger,
        IDbContextFactory<DatahubProjectDBContext> contextFactory,
        IUserInformationService userInformationService,
        IMSGraphService msGraphService,
        IRequestManagementService requestManagementService
    )
    {
        _userInformationService = userInformationService;
        _msGraphService = msGraphService;
        _requestManagementService = requestManagementService;
        _logger = logger;
        _contextFactory = contextFactory;
    }

    public async Task AddUsersToProject(string projectAcronym, IEnumerable<string> userGraphIds)
    {
        using var scope = new TransactionScope(
            TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
        await using var context = await _contextFactory.CreateDbContextAsync();
        var project = await context.Projects
            .Include(p => p.Users)
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym);
        if (project == null)
        {
            _logger.LogError("Project {ProjectAcronym} not found when trying to add users {Join}", projectAcronym, string.Join(", ", userGraphIds));
            throw new ProjectNotFoundException($"Project {projectAcronym} not found");
        }
        foreach (var userGraphId in userGraphIds)
        {
            _logger.LogInformation("Preparing to add user {UserGraphId} to project {ProjectAcronym}", userGraphId,
                projectAcronym);

            var exists = context.Project_Users
                .Any(u => u.User_ID == userGraphId && u.Project.Project_ID == project.Project_ID);

            if (exists)
            {
                _logger.LogInformation("User {UserGraphId} already exists in project {ProjectAcronym}", userGraphId,
                    projectAcronym);
            }
            else
            {
                try
                {
                    
                    var approvingUser = await _userInformationService.GetUserIdString();
                    _logger.LogInformation(
                        "Adding user {UserGraphId} to project {ProjectAcronym} by user {ApprovingUser}",
                        userGraphId, projectAcronym, approvingUser);
                    var user = await _msGraphService.GetUserAsync(userGraphId);

                    var newProjectUser = new Datahub_Project_User
                    {
                        Project = project,
                        User_ID = userGraphId,
                        User_Name = user.Mail,

                        IsAdmin = false,
                        IsDataApprover = false,

                        Approved_DT = DateTime.Now,
                        ApprovedUser = approvingUser
                    };

                    context.Project_Users.Add(newProjectUser);
                    _logger.LogInformation("User {UserGraphId} added to project {ProjectAcronym}", userGraphId,
                        projectAcronym);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding user {UserGraphId} to project {ProjectAcronym}", userGraphId,
                        projectAcronym);
                    throw;
                }
            }
        }
        try
        {
            
            await context.SaveChangesAsync();
            await _requestManagementService.HandleTerraformRequestServiceAsync(project, TerraformTemplate.VariableUpdate);
            _logger.LogInformation("Terraform variable update request created for project {ProjectAcronym}", projectAcronym);
            scope.Complete();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling Terraform request for project {ProjectAcronym}", projectAcronym);
            throw;
        }
    }
    
    public async Task AddUserToProject(string projectAcronym, string userGraphId)
    {
        await AddUsersToProject(projectAcronym, new List<string> {userGraphId});
    }

    public async Task RemoveUserFromProject(string projectAcronym, string userGraphId)
    {
        _logger.LogInformation("Preparing to remove user {UserGraphId} from project {ProjectAcronym}", userGraphId,
            projectAcronym);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var project = await context.Projects
            .Include(p => p.Users)
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym);

        if (project == null)
        {
            _logger.LogError("Project {ProjectAcronym} not found when trying to remove user {UserGraphId}",
                projectAcronym,
                userGraphId);
            throw new ProjectNotFoundException($"Project {projectAcronym} not found");
        }

        var exists = await context.Project_Users
            .FirstOrDefaultAsync(u => u.User_ID == userGraphId && u.Project.Project_ID == project.Project_ID);

        if (exists is null)
        {
            _logger.LogInformation("User {UserGraphId} does not exist in project {ProjectAcronym}", userGraphId,
                projectAcronym);
            return;
        }
        
        using var scope = new TransactionScope(
            TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);

        try
        {
            context.Project_Users.Remove(exists);
            await context.SaveChangesAsync();
            _logger.LogInformation("User {UserGraphId} removed from project {ProjectAcronym}", userGraphId, projectAcronym);
            
            await _requestManagementService.HandleTerraformRequestServiceAsync(project, TerraformTemplate.VariableUpdate);
            _logger.LogInformation("Terraform variable update request created for project {ProjectAcronym}", projectAcronym);
            scope.Complete();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user {UserGraphId} from project {ProjectAcronym}", userGraphId,
                projectAcronym);
            throw;
        }

    }

    public async Task UpdateUserInProject(string projectAcronym, ProjectMember projectMember)
    {
        _logger.LogInformation("Preparing to remove user {UserGraphId} from project {ProjectAcronym}", projectMember.UserId,
            projectAcronym);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var project = await context.Projects
            .Include(p => p.Users)
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym);

        if (project == null)
        {
            _logger.LogError("Project {ProjectAcronym} not found when trying to remove user {UserGraphId}",
                projectAcronym,
                projectMember.UserId);
            throw new ProjectNotFoundException($"Project {projectAcronym} not found");
        }

        var exists = await context.Project_Users
            .FirstOrDefaultAsync(u => u.User_ID == projectMember.UserId && u.Project.Project_ID == project.Project_ID);

        if (exists is null)
        {
            _logger.LogInformation("User {UserGraphId} does not exist in project {ProjectAcronym}", projectMember.UserId,
                projectAcronym);
            throw new UserNotFoundException($"User {projectMember.UserId} not found in project {projectAcronym}");
        }

        if (!ShouldUpdateUser(exists, projectMember.Role))
        {
            _logger.LogInformation("User {UserGraphId} already has role {Role} in project {ProjectAcronym}", projectMember.UserId,
                projectMember.Role, projectAcronym);
            return;
        }
        
        using var scope = new TransactionScope(
            TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);

        try
        {
            exists.IsDataApprover = projectMember.Role == ProjectMemberRole.Publisher;
            exists.IsAdmin = exists.IsDataApprover || projectMember.Role == ProjectMemberRole.Admin;
            await context.SaveChangesAsync();
            _logger.LogInformation("User {UserGraphId} removed from project {ProjectAcronym}", projectMember.UserId, projectAcronym);
            await _requestManagementService.HandleTerraformRequestServiceAsync(project, TerraformTemplate.VariableUpdate);
            _logger.LogInformation("Terraform variable update request created for project {ProjectAcronym}", projectAcronym);
            scope.Complete();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user {UserGraphId} from project {ProjectAcronym}", projectMember.UserId,
                projectAcronym);
            throw;
        }
    }

    public async Task<IEnumerable<Datahub_Project_User>> GetUsersFromProject(string projectAcronym)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Project_Users
            .Include(u => u.Project)
            .Where(u => u.Project.Project_Acronym_CD == projectAcronym)
            .ToListAsync();
    }

    private bool ShouldUpdateUser(Datahub_Project_User projectUser, ProjectMemberRole projectMemberRole)
    {
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault - Remove case is frontend only
        switch (projectMemberRole)
        {
            case ProjectMemberRole.Publisher when !projectUser.IsDataApprover || !projectUser.IsAdmin:
            case ProjectMemberRole.Admin when !projectUser.IsAdmin || projectUser.IsDataApprover:
                return true;
            case ProjectMemberRole.Contributor:
            default:
                return projectMemberRole == ProjectMemberRole.Contributor && (projectUser.IsDataApprover || projectUser.IsAdmin);
        }
    }
}