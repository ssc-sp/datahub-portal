using System.Transactions;
using Datahub.Application.Services;
using Datahub.Core.Data.Project;
using Datahub.Core.Model.Datahub;
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

    public async Task<IEnumerable<ProjectMember>> BatchUpdateUsersInProject(string projectAcronym, IEnumerable<ProjectMember> projectMembers)
    {
        
        var projectMembersList = projectMembers.ToList();
        var errorSendingInvites = new List<ProjectMember>();
        var currentUser = await _userInformationService.GetCurrentGraphUserAsync();
        var currentUserName = currentUser?.DisplayName ?? currentUser?.UserPrincipalName ?? "";
        // send invites to users that are not members of Datahub
        foreach (var member in projectMembersList.Where(member => !member.UserHasBeenInvitedToDatahub && member.Role != ProjectMemberRole.Remove))
        {
            try
            {
                member.UserId = await _userEnrollmentService.SendUserDatahubPortalInvite(member.Email, currentUserName );
            }
            catch (Exception e)
            {
                errorSendingInvites.Add(member);
                _logger.LogError(e, "Error sending invite to user {UserGraphId} for project {ProjectAcronym}", member.UserId, projectAcronym);
            }
        }
        using var scope = new TransactionScope(
            TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
        await using var context = await _contextFactory.CreateDbContextAsync();
        var project = await context.Projects
            .Include(p => p.Users)
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym);
        
        if (project == null)
        {
            _logger.LogError("Project {ProjectAcronym} not found when trying to batch update users {Join}", 
                projectAcronym, string.Join(", ", projectMembersList.Select(p => p.UserId)));
            throw new ProjectNotFoundException($"Project {projectAcronym} not found");
        }
        // Remove users that are no longer in the project
        var usersToRemoveFromProject =
            projectMembersList.Where(p => p.Role == ProjectMemberRole.Remove).ToList();
        foreach (var user in usersToRemoveFromProject)
        {
            await RemoveUserFromProject(context, project, user.UserId);
        }
        
        // Add users that are not already in the project
        // Except usersToRemove here to handle edge case where user is added to project and then removed in same batch
        var usersToAddToProject = projectMembersList.Except(usersToRemoveFromProject)
            .Where(p => project.Users.All(u => u.User_ID != p.UserId)).ToList();
        foreach (var user in usersToAddToProject)
        {
            _logger.LogInformation("Preparing to add user {UserGraphId} to project {ProjectAcronym}", user.UserId,
                projectAcronym);
                await AddUserToProject(context, project, user.UserId, user.Role);

        }
        
        // Update users that are already in the project
        var usersToUpdateInProject = projectMembersList.Except(usersToAddToProject).Except(usersToRemoveFromProject).ToList();
        foreach (var user in usersToUpdateInProject)
        {
            await UpdateUserInProject(context, project, user.UserId, user.Role);
        }
        try
        {
            
            await context.TrackSaveChangesAsync(_datahubAuditingService);
            await _requestManagementService.HandleTerraformRequestServiceAsync(project, TerraformTemplate.VariableUpdate);
            _serviceAuthManager.InvalidateAuthCache();
            _logger.LogInformation("Terraform variable update request created for project {ProjectAcronym}", projectAcronym);
            scope.Complete();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling Terraform request for project {ProjectAcronym} when preforming batch update", projectAcronym);
            throw;
        }

        return errorSendingInvites;

    }
    
    
    public async Task AddUserToProject(string projectAcronym, string userGraphId)
    {
        await AddUsersToProject(projectAcronym, new List<string> {userGraphId});
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
                await AddUserToProject(context, project, userGraphId);
            }
        }
        try
        {
            await context.TrackSaveChangesAsync(_datahubAuditingService);
            await _requestManagementService.HandleTerraformRequestServiceAsync(project, TerraformTemplate.VariableUpdate);
            _serviceAuthManager.InvalidateAuthCache();
            _logger.LogInformation("Terraform variable update request created for project {ProjectAcronym}", projectAcronym);
            
            scope.Complete();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling Terraform request for project {ProjectAcronym}", projectAcronym);
            throw;
        }
        
    }
    
    private async Task AddUserToProject(DatahubProjectDBContext context, Datahub_Project project, string userGraphId, ProjectMemberRole role = ProjectMemberRole.Collaborator)
    {
        try
        {
                
            var approvingUser = await _userInformationService.GetUserIdString();
            _logger.LogInformation(
                "Adding user {UserGraphId} to project {ProjectAcronym} by user {ApprovingUser}",
                userGraphId, project.Project_Acronym_CD, approvingUser);
            var user = await _msGraphService.GetUserAsync(userGraphId);
            if (string.IsNullOrWhiteSpace(userGraphId)) throw new InvalidOperationException("Cannot add user without user ID");
            var newProjectUser = new Datahub_Project_User
            {
                Project = project,
                User_ID = userGraphId,
                User_Name = user.Mail,

                IsAdmin = role is ProjectMemberRole.WorkspaceLead or ProjectMemberRole.Admin,
                IsDataApprover = role == ProjectMemberRole.WorkspaceLead,

                Approved_DT = DateTime.Now,
                ApprovedUser = approvingUser
            };

            context.Project_Users.Add(newProjectUser);
            _logger.LogInformation("User {UserGraphId} added to project {ProjectAcronym}", userGraphId,
                project.Project_Acronym_CD);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user {UserGraphId} to project {ProjectAcronym}", userGraphId,
                project.Project_Acronym_CD);
            throw;
        }
    }
    
    public async Task RemoveUserFromProject(string projectAcronym, string userGraphId)
    {
        _logger.LogInformation("Preparing to remove user {UserGraphId} from project {ProjectAcronym}", userGraphId,
            projectAcronym);

        await using var context = await _contextFactory.CreateDbContextAsync();
        
        using var scope = new TransactionScope(
            TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
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

        try
        {
            await RemoveUserFromProject(context, project, userGraphId);
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

    private async Task RemoveUserFromProject(DatahubProjectDBContext context, Datahub_Project project, string userGraphId)
    {
        var exists = await context.Project_Users
            .FirstOrDefaultAsync(u => u.User_ID == userGraphId && u.Project.Project_ID == project.Project_ID);

        if (exists is null)
        {
            _logger.LogInformation("User {UserGraphId} does not exist in project {ProjectAcronym}", userGraphId,
                project.Project_Acronym_CD);
            return;
        }
        context.Project_Users.Remove(exists);
        await context.TrackSaveChangesAsync(_datahubAuditingService);
        _logger.LogInformation("User {UserGraphId} removed from project {ProjectAcronym}", userGraphId, project.Project_Acronym_CD);
    }

    public async Task UpdateUserInProject(string projectAcronym, ProjectMember projectMember)
    {
        _logger.LogInformation("Preparing to remove user {UserGraphId} from project {ProjectAcronym}", projectMember.UserId,
            projectAcronym);

        await using var context = await _contextFactory.CreateDbContextAsync();
        
        using var scope = new TransactionScope(
            TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
        
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
        try
        {
            await UpdateUserInProject(context, project, projectMember.UserId, projectMember.Role);
            await context.TrackSaveChangesAsync(_datahubAuditingService);
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

    private async Task UpdateUserInProject(DatahubProjectDBContext context, Datahub_Project project, string userGraphId, ProjectMemberRole role)
    {
        var exists = await context.Project_Users
            .FirstOrDefaultAsync(u => u.User_ID == userGraphId && u.Project.Project_ID == project.Project_ID);

        if (exists is null)
        {
            _logger.LogInformation("User {UserGraphId} does not exist in project {ProjectAcronym}", userGraphId,
                project.Project_Acronym_CD);
            throw new UserNotFoundException($"User {userGraphId} not found in project { project.Project_Acronym_CD}");
        }
        if (!ShouldUpdateUser(exists, role))
        {
            _logger.LogInformation("User {UserGraphId} already has role {Role} in project {ProjectAcronym}", userGraphId,
                role, project.Project_Acronym_CD);
            return;
        } 
        exists.IsDataApprover = role == ProjectMemberRole.WorkspaceLead;
        exists.IsAdmin = exists.IsDataApprover || role == ProjectMemberRole.Admin;
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
            case ProjectMemberRole.WorkspaceLead when !projectUser.IsDataApprover || !projectUser.IsAdmin:
            case ProjectMemberRole.Admin when !projectUser.IsAdmin || projectUser.IsDataApprover:
                return true;
            case ProjectMemberRole.Collaborator:
            default:
                return projectMemberRole == ProjectMemberRole.Collaborator && (projectUser.IsDataApprover || projectUser.IsAdmin);
        }
    }
}