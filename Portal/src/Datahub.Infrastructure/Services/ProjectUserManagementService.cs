using System.Transactions;
using Datahub.Application.Services;
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

    public async Task AddUserToProject(string projectAcronym, string userGraphId)
    {
        _logger.LogInformation("Preparing to add user {UserGraphId} to project {ProjectAcronym}", userGraphId,
            projectAcronym);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var project = await context.Projects
            .Include(p => p.Users)
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym);

        if (project == null)
        {
            _logger.LogError("Project {ProjectAcronym} not found when trying to add user {UserGraphId}", projectAcronym,
                userGraphId);
            throw new ProjectNoFoundException($"Project {projectAcronym} not found");
        }

        var exists = context.Project_Users
            .Any(u => u.User_ID == userGraphId && u.ProjectId == project.Project_ID);

        if (exists)
        {
            _logger.LogInformation("User {UserGraphId} already exists in project {ProjectAcronym}", userGraphId,
                projectAcronym);
            return;
        }

        using var scope = new TransactionScope(
            TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);

        try
        {
            var approvingUser = await _userInformationService.GetUserIdString();
            _logger.LogInformation("Adding user {UserGraphId} to project {ProjectAcronym} by user {ApprovingUser}",
                userGraphId, projectAcronym, approvingUser);
            var user = await _msGraphService.GetUserAsync(userGraphId);

            var newProjectUser = new Datahub_Project_User
            {
                ProjectId = project.Project_ID,
                User_ID = userGraphId,
                User_Name = user.Mail,

                IsAdmin = false,
                IsDataApprover = false,

                Approved_DT = DateTime.Now,
                ApprovedUser = approvingUser
            };

            context.Project_Users.Add(newProjectUser);
            await context.SaveChangesAsync();

            _logger.LogInformation("User {UserGraphId} added to project {ProjectAcronym}", userGraphId, projectAcronym);

            await _requestManagementService.HandleTerraformRequestServiceAsync(project, TerraformTemplate.VariableUpdate);
            scope.Complete();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user {UserGraphId} to project {ProjectAcronym}", userGraphId,
                projectAcronym);
            throw;
        }
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
            throw new ProjectNoFoundException($"Project {projectAcronym} not found");
        }

        var exists = await context.Project_Users
            .FirstOrDefaultAsync(u => u.User_ID == userGraphId && u.ProjectId == project.Project_ID);

        if (exists is null)
        {
            _logger.LogInformation("User {UserGraphId} does not exist in project {ProjectAcronym}", userGraphId,
                projectAcronym);
            return;
        }

        context.Project_Users.Remove(exists);
        await context.SaveChangesAsync();

        _logger.LogInformation("User {UserGraphId} removed from project {ProjectAcronym}", userGraphId, projectAcronym);
    }

    public Task UpdateUserInProject(string projectAcronym, Datahub_Project_User user)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Datahub_Project_User>> GetUsersFromProject(string projectAcronym)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Project_Users
            .Include(u => u.Project)
            .Where(u => u.Project.Project_Acronym_CD == projectAcronym)
            .ToListAsync();
    }
}