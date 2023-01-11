using Datahub.Application.Services;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services;
using Datahub.Core.Services.UserManagement;
using Datahub.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services;

public class ProjectUserManagementService : IProjectUserManagementService
{
    private readonly IUserInformationService _userInformationService;
    private readonly IMSGraphService _msGraphService;
    private readonly ILogger<ProjectUserManagementService> _logger;
    private readonly IDbContextFactory<DatahubProjectDBContext> _contextFactory;

    public ProjectUserManagementService(
        ILogger<ProjectUserManagementService> logger,
        IDbContextFactory<DatahubProjectDBContext> contextFactory,
        IUserInformationService userInformationService,
        IMSGraphService msGraphService)
    {
        _userInformationService = userInformationService;
        _msGraphService = msGraphService;
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
            .Any(u => u.User_ID == userGraphId && u.Project_ID == project.Project_ID);
        
        if (exists)
        {
            _logger.LogInformation("User {UserGraphId} already exists in project {ProjectAcronym}", userGraphId,
                projectAcronym);
            return;
        }

        var approvingUser = await _userInformationService.GetUserIdString();
        _logger.LogInformation("Adding user {UserGraphId} to project {ProjectAcronym} by user {ApprovingUser}",
            userGraphId, projectAcronym, approvingUser);
        var user = await _msGraphService.GetUserAsync(userGraphId);

        var newProjectUser = new Datahub_Project_User
        {
            Project_ID = project.Project_ID,
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
    }

    public Task RemoveUserFromProject(string projectAcronym, string userGraphId)
    {
        throw new NotImplementedException();
    }

    public Task UpdateUserInProject(string projectAcronym, Datahub_Project_User user)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Datahub_Project_User>> GetProjectUsers(string projectAcronym)
    {
        throw new NotImplementedException();
    }
}