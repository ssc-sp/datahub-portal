using System.Linq.Dynamic.Core;
using System.Net.Mail;
using Datahub.Application.Commands;
using Datahub.Application.Services;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Data;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Core.Services;
using Datahub.Core.Services.Projects;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Security;
using Datahub.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;

namespace Datahub.Infrastructure.UnitTests.Services;

using static Testing;

public class ProjectUserManagementServiceTests
{
    private Mock<IDbContextFactory<DatahubProjectDBContext>> _mockFactory = null!;
    private Mock<IUserInformationService> _mockUserInformationService = null!;

    // ReSharper disable once InconsistentNaming
    private Mock<IMSGraphService> _mockIMSGraphService = null!;
    private Mock<IRequestManagementService> _mockRequestManagementService = null!;
    private Mock<IResourceMessagingService> _mockResourceManagementService = null!;
    private Mock<IUserEnrollmentService> _mockUserEnrollmentService = null!;
    private IServiceAuthManager _serviceAuthManager = null!;
    private Mock<IDatahubAuditingService> _mockDatahubAuditingService = null!;

    private readonly string[] _testUserIds = TEST_USER_IDS;

    private DatahubProjectDBContext _dbContext = null!;

    private PortalUser _testCurrentUser = null!;

    [SetUp]
    public void Setup()
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<SqlServerDatahubContext>()
                .UseInMemoryDatabase("ProjectUserManagementServiceTests");
        _dbContext = new SqlServerDatahubContext(optionsBuilder.Options);
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();
        SeedDatabase(_dbContext);


        // create a mock factory to return the db context when CreateDbContextAsync is called
        _mockFactory = new Mock<IDbContextFactory<DatahubProjectDBContext>>();
        _mockFactory
            .Setup(f => f.CreateDbContextAsync(CancellationToken.None))
            .ReturnsAsync(() => new SqlServerDatahubContext(optionsBuilder.Options));

        // create a mock user information service to return the current (admin) user when GetUserIdString is called
        _mockUserInformationService = new Mock<IUserInformationService>();
        _mockUserInformationService
            .Setup(f => f.GetCurrentPortalUserAsync())
            .ReturnsAsync(_testCurrentUser);

        _mockUserInformationService
            .Setup(f => f.GetPortalUserAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => _dbContext.PortalUsers.First(u => u.GraphGuid == id));

        _mockUserInformationService
            .Setup(f => f.CreatePortalUserAsync(It.IsAny<string>()))
            .Callback((string graphId) =>
            {
                _dbContext.PortalUsers.Add(new PortalUser()
                {
                    GraphGuid = graphId,
                    Email = TestUserEmail,
                    DisplayName = TestUserEmail
                });
                _dbContext.SaveChanges();
            });

        _mockIMSGraphService = new Mock<IMSGraphService>();
        _mockIMSGraphService
            .Setup(f => f.GetUserAsync(It.Is<string>(s => _testUserIds.Contains(s) || s == TestUserGraphGuid),
                CancellationToken.None))
            .Returns((string id, CancellationToken _) => Task.FromResult(new GraphUser
                {
                    mailAddress = new MailAddress(TestUserEmail),
                    Id = id,
                })
            );

        _mockRequestManagementService = new Mock<IRequestManagementService>();
        _mockRequestManagementService
            .Setup(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(), It.IsAny<TerraformTemplate>(),
                It.IsAny<PortalUser>()))
            .ReturnsAsync(true);

        _mockUserEnrollmentService = new Mock<IUserEnrollmentService>();
        _mockUserEnrollmentService
            .Setup(f => f.SendUserDatahubPortalInvite(It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(TestUserGraphGuid);

        _mockDatahubAuditingService = new Mock<IDatahubAuditingService>();

        var mockMemoryCache = new Mock<IMemoryCache>();

        _serviceAuthManager =
            new ServiceAuthManager(mockMemoryCache.Object, _mockFactory.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    [Test]
    [Ignore("Needs to be validated")]
    public async Task ShouldProcessEmptyProjectUserCommandsTest()
    {
        var projectUserManagementService = GetProjectUserManagementService();

        var result =
            await projectUserManagementService.ProcessProjectUserCommandsAsync(
                new List<ProjectUserUpdateCommand>(),
                new List<ProjectUserAddUserCommand>(), "");

        Assert.That(result, Is.True);

        _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
            It.IsAny<TerraformTemplate>(), It.IsAny<PortalUser>()), Times.Never);
    }

    #region ProjectUserAddUserCommand

    [Test]
    [TestCase((int)Project_Role.RoleNames.WorkspaceLead, Ignore = "Needs to be validated")]
    [TestCase((int)Project_Role.RoleNames.Admin, Ignore = "Needs to be validated")]
    [TestCase((int)Project_Role.RoleNames.Collaborator, Ignore = "Needs to be validated")]
    [TestCase((int)Project_Role.RoleNames.Guest, Ignore = "Needs to be validated")]
    [TestCase((int)Project_Role.RoleNames.Remove, Ignore = "Needs to be validated")]
    public async Task ShouldProcessAddExistingUserCommandTest(int roleId)
    {
        _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
            It.IsAny<TerraformTemplate>(), It.IsAny<PortalUser>()), Times.Never);

        var projectUserManagementService = GetProjectUserManagementService();

        var existingProjectUser = await _dbContext.Project_Users
            .AsNoTracking()
            .Include(u => u.Project)
            .Include(u => u.PortalUser)
            .FirstAsync();

        var foreignProject = await _dbContext.Projects
            .FirstAsync(p => existingProjectUser.Project.Project_Acronym_CD != p.Project_Acronym_CD);

        var command = new ProjectUserAddUserCommand
        {
            DisplayName = existingProjectUser.PortalUser.DisplayName,
            Email = existingProjectUser.PortalUser.Email,
            GraphGuid = existingProjectUser.PortalUser.GraphGuid,
            ProjectAcronym = foreignProject.Project_Acronym_CD,
            RoleId = roleId,
        };

        var result =
            await projectUserManagementService.ProcessProjectUserCommandsAsync(
                new List<ProjectUserUpdateCommand>(),
                new List<ProjectUserAddUserCommand> { command }, "");

        if (roleId == (int)Project_Role.RoleNames.Remove)
        {
            Assert.That(result, Is.False);
            _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
                It.IsAny<TerraformTemplate>(), It.IsAny<PortalUser>()), Times.Never);
        }
        else
        {
            Assert.That(result, Is.True);
            _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
                It.IsAny<TerraformTemplate>(), It.IsAny<PortalUser>()), Times.Once);
        }
    }

    [Test]
    [Ignore("Needs to be validated")]
    public async Task ShouldSendInviteIfNewUserTest()
    {
        var projectUserManagementService = GetProjectUserManagementService();

        _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
            It.IsAny<TerraformTemplate>(), It.IsAny<PortalUser>()), Times.Never);
        _mockUserEnrollmentService.Verify(f => f.SendUserDatahubPortalInvite(It.IsAny<string?>(), It.IsAny<string?>()),
            Times.Never);

        var firstProject = await _dbContext.Projects.FirstAsync();

        var command = new ProjectUserAddUserCommand
        {
            DisplayName = TestUserEmail,
            Email = TestUserEmail,
            GraphGuid = ProjectUserAddUserCommand.NEW_USER_GUID,
            ProjectAcronym = firstProject.Project_Acronym_CD,
            RoleId = (int)Project_Role.RoleNames.Collaborator,
        };

        var result =
            await projectUserManagementService.ProcessProjectUserCommandsAsync(
                new List<ProjectUserUpdateCommand>(),
                new List<ProjectUserAddUserCommand> { command }, "");

        Assert.That(result, Is.True);

        _mockUserEnrollmentService.Verify(f => f.SendUserDatahubPortalInvite(It.IsAny<string?>(), It.IsAny<string?>()),
            Times.Once);
        _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
            It.IsAny<TerraformTemplate>(), It.IsAny<PortalUser>()), Times.Once);

        var projectUser = _dbContext.Project_Users
            .Include(u => u.Project)
            .Include(u => u.PortalUser)
            .First(u => u.PortalUser.GraphGuid == TestUserGraphGuid);

        Assert.Multiple(() =>
        {
            Assert.That(projectUser.Project.Project_Acronym_CD, Is.EqualTo(firstProject.Project_Acronym_CD));
            Assert.That(projectUser.RoleId, Is.EqualTo((int)Project_Role.RoleNames.Collaborator));
            Assert.That(projectUser.PortalUser.Email, Is.EqualTo(TestUserEmail));
        });
    }

    [Test]
    [Ignore("Needs to be validated")]
    public async Task ShouldFailIfProjectDoesNotExist()
    {
        var projectUserManagementService = GetProjectUserManagementService();

        _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
            It.IsAny<TerraformTemplate>(), It.IsAny<PortalUser>()), Times.Never);

        var existingProjectUser = await _dbContext.Project_Users
            .AsNoTracking()
            .Include(u => u.Project)
            .Include(u => u.PortalUser)
            .FirstAsync();

        var command = new ProjectUserAddUserCommand
        {
            DisplayName = existingProjectUser.PortalUser.DisplayName,
            Email = existingProjectUser.PortalUser.Email,
            GraphGuid = existingProjectUser.PortalUser.GraphGuid,
            ProjectAcronym = "AbsolutelyNotARealProject",
            RoleId = (int)Project_Role.RoleNames.Collaborator,
        };

        var result =
            await projectUserManagementService.ProcessProjectUserCommandsAsync(
                new List<ProjectUserUpdateCommand>(),
                new List<ProjectUserAddUserCommand> { command }, "");

        Assert.That(result, Is.False);
        _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
            It.IsAny<TerraformTemplate>(), It.IsAny<PortalUser>()), Times.Never);
    }

    [Test]
    [Ignore("Needs to be validated")]
    public async Task ShouldFailIfUserAlreadyOnProject()
    {
        var projectUserManagementService = GetProjectUserManagementService();

        _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
            It.IsAny<TerraformTemplate>(), It.IsAny<PortalUser>()), Times.Never);

        var existingProjectUser = await _dbContext.Project_Users
            .AsNoTracking()
            .Include(u => u.Project)
            .Include(u => u.PortalUser)
            .FirstAsync();

        var datahubProject = await _dbContext.Projects
            .FirstAsync(p => existingProjectUser.Project.Project_Acronym_CD == p.Project_Acronym_CD);

        var command = new ProjectUserAddUserCommand
        {
            DisplayName = existingProjectUser.PortalUser.DisplayName,
            Email = existingProjectUser.PortalUser.Email,
            GraphGuid = existingProjectUser.PortalUser.GraphGuid,
            ProjectAcronym = datahubProject.Project_Acronym_CD,
            RoleId = (int)Project_Role.RoleNames.Collaborator,
        };

        var result =
            await projectUserManagementService.ProcessProjectUserCommandsAsync(
                new List<ProjectUserUpdateCommand>(),
                new List<ProjectUserAddUserCommand> { command }, "");

        Assert.That(result, Is.False);

        _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
            It.IsAny<TerraformTemplate>(), It.IsAny<PortalUser>()), Times.Never);
    }

    #endregion

    #region ProjectUserUpdateCommand

    [Test]
    [TestCase((int)Project_Role.RoleNames.WorkspaceLead, Ignore = "Needs to be validated")]
    [TestCase((int)Project_Role.RoleNames.Admin, Ignore = "Needs to be validated")]
    [TestCase((int)Project_Role.RoleNames.Collaborator, Ignore = "Needs to be validated")]
    [TestCase((int)Project_Role.RoleNames.Guest, Ignore = "Needs to be validated")]
    [TestCase((int)Project_Role.RoleNames.Remove, Ignore = "Needs to be validated")]
    public async Task ShouldProcessUpdateUserCommandTest(int roleId)
    {
        var projectUserManagementService = GetProjectUserManagementService();

        _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
            It.IsAny<TerraformTemplate>(), It.IsAny<PortalUser>()), Times.Never);

        var existingProjectUser = await _dbContext.Project_Users
            .AsNoTracking()
            .Include(u => u.Project)
            .Include(u => u.PortalUser)
            .FirstAsync(u => u.RoleId != roleId);

        var command = new ProjectUserUpdateCommand
        {
            ProjectUser = existingProjectUser,
            NewRoleId = roleId,
        };

        var result =
            await projectUserManagementService.ProcessProjectUserCommandsAsync(
                new List<ProjectUserUpdateCommand>() { command },
                new List<ProjectUserAddUserCommand>(), "");

        Assert.That(result, Is.True);

        if (roleId == (int)Project_Role.RoleNames.Remove)
        {
            // double check that the project user is gone
            var projectUser = await _dbContext.Project_Users
                .AsNoTracking()
                .Include(u => u.PortalUser).Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.PortalUser.GraphGuid == existingProjectUser.PortalUser.GraphGuid);

            Assert.That(projectUser.Role.Id, Is.EqualTo((int)Project_Role.RoleNames.Remove));
        }

        //_mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
        //    It.IsAny<string>()), Times.Once);
    }

    #endregion


    #region GetProjectUsersAsync

    [Test]
    [Ignore("Needs to be validated")]
    public async Task ShouldGetProjectUsersAsync()
    {
        var projectUserManagementService = GetProjectUserManagementService();

        var seededProjectUsers = await _dbContext.Project_Users
            .AsNoTracking()
            .Include(u => u.Project)
            .Include(u => u.PortalUser)
            .ToListAsync();

        foreach (var seededProject in seededProjectUsers.Select(u => u.Project).Distinct())
        {
            var projectUsers =
                await projectUserManagementService.GetProjectUsersAsync(seededProject.Project_Acronym_CD);

            Assert.That(projectUsers, Is.Not.Null);
            Assert.That(projectUsers,
                Has.Count.EqualTo(seededProjectUsers.Count(u =>
                    u.Project.Project_Acronym_CD == seededProject.Project_Acronym_CD)));
        }
    }

    #endregion

    //
    // [Test]
    
    // public async Task ShouldAddUserToProject()
    // {
    //     var projectUserManagementService = GetProjectUserManagementService();
    //     await SeedDatabase();
    //
    //     await projectUserManagementService.AddUserToProject(TestProjectAcronym, TestUserId);
    //
    //     await using var context = await _mockFactory.Object.CreateDbContextAsync();
    //     var projectUsers = await context.Project_Users
    //         .Include(p => p.Project)
    //         .ToListAsync();
    //     var projectId = await context.Projects
    //         .Where(p => p.Project_Acronym_CD == TestProjectAcronym)
    //         .Select(p => p.Project_ID)
    //         .SingleAsync();
    //
    //     Assert.That(projectUsers, Has.Count.EqualTo(1));
    //     Assert.Multiple(() =>
    //     {
    //         Assert.That(projectUsers[0].Project.Project_ID, Is.EqualTo(projectId));
    //         Assert.That(projectUsers[0].User_ID, Is.EqualTo(TestUserId));
    //     });
    // }
    //
    // [Test]
    // public async Task ShouldAddMultipleUsersToProject()
    // {
    //     var projectUserManagementService = GetProjectUserManagementService();
    //     await SeedDatabase();
    //     await projectUserManagementService.AddUsersToProject(TestProjectAcronym, _testUserIds);
    //
    //     await using var context = await _mockFactory.Object.CreateDbContextAsync();
    //     var projectUsers = await context.Project_Users
    //         .Include(p => p.Project)
    //         .ToListAsync();
    //     var projectId = await context.Projects
    //         .Where(p => p.Project_Acronym_CD == TestProjectAcronym)
    //         .Select(p => p.Project_ID)
    //         .SingleAsync();
    //
    //    
    //     Assert.Multiple(() =>
    //     { 
    //         Assert.That(projectUsers, Has.Count.EqualTo(_testUserIds.Length));
    //         Assert.That(projectUsers[0].Project.Project_ID, Is.EqualTo(projectId));
    //         Assert.That(projectUsers.Select(p => p.User_ID), Is.EquivalentTo(_testUserIds));
    //         
    //     });
    // }
    //
    // [Test]
    
    // public async Task ShouldThrowException_WhenProjectNotFoundOnUserAdd()
    // {
    //     const string nonExistentProjectAcronym = "NOTFOUND";
    //     var projectUserManagementService = GetProjectUserManagementService();
    //     await SeedDatabase(new List<string>
    //     {
    //         TestUserId
    //     });
    //
    //     Assert.ThrowsAsync<ProjectNotFoundException>(async () =>
    //     {
    //         await projectUserManagementService.AddUserToProject(nonExistentProjectAcronym, TestUserId);
    //     });
    // }
    //
    // [Test]
    
    // public async Task ShouldDoNothing_WhenUserAlreadyAdded()
    // {
    //     var projectUserManagementService = GetProjectUserManagementService();
    //     await SeedDatabase(new List<string>
    //     {
    //         TestUserId
    //     });
    //
    //     await projectUserManagementService.AddUserToProject(TestProjectAcronym, TestUserId);
    //     await projectUserManagementService.AddUserToProject(TestProjectAcronym, TestUserId);
    //     await projectUserManagementService.AddUserToProject(TestProjectAcronym, TestUserId);
    //
    //     await using var context = await _mockFactory.Object.CreateDbContextAsync();
    //     var projectUsers = await context.Project_Users.ToListAsync();
    //     Assert.That(projectUsers, Has.Count.EqualTo(1));
    // }
    //
    // [Test]
    
    // public async Task ShouldRemoveUserFromProject()
    // {
    //     var projectUserManagementService = GetProjectUserManagementService();
    //     await SeedDatabase(new List<string>
    //     {
    //         TestUserId
    //     });
    //
    //     await projectUserManagementService.RemoveUserFromProject(TestProjectAcronym, TestUserId);
    //
    //     await using var context = await _mockFactory.Object.CreateDbContextAsync();
    //     var projectUsers = await context.Project_Users.ToListAsync();
    //     Assert.That(projectUsers, Has.Count.EqualTo(0));
    // }
    //
    // [Test]
    
    // public async Task ShouldThrowException_WhenProjectNotFoundOnUserRemove()
    // {
    //     const string nonExistentProjectAcronym = "NOTFOUND";
    //     var projectUserManagementService = GetProjectUserManagementService();
    //     await SeedDatabase(new List<string>
    //     {
    //         TestUserId
    //     });
    //
    //     Assert.ThrowsAsync<ProjectNotFoundException>(async () =>
    //     {
    //         await projectUserManagementService.RemoveUserFromProject(nonExistentProjectAcronym, TestUserId);
    //     });
    // }
    //
    // [Test]
    
    // public async Task ShouldDoNothing_WhenUserAlreadyRemoved()
    // {
    //     var projectUserManagementService = GetProjectUserManagementService();
    //     await SeedDatabase();
    //
    //     await projectUserManagementService.RemoveUserFromProject(TestProjectAcronym, TestUserId);
    //     await projectUserManagementService.RemoveUserFromProject(TestProjectAcronym, TestUserId);
    //     await projectUserManagementService.RemoveUserFromProject(TestProjectAcronym, TestUserId);
    //
    //     await using var context = await _mockFactory.Object.CreateDbContextAsync();
    //     var projectUsers = await context.Project_Users.ToListAsync();
    //     Assert.That(projectUsers, Has.Count.EqualTo(0));
    // }
    //
    // [Test]
    
    // [TestCase(ProjectMemberRole.Collaborator, ProjectMemberRole.Admin)]
    // [TestCase(ProjectMemberRole.Collaborator, ProjectMemberRole.WorkspaceLead)]
    // [TestCase(ProjectMemberRole.Admin, ProjectMemberRole.Collaborator)]
    // [TestCase(ProjectMemberRole.Admin, ProjectMemberRole.WorkspaceLead)]
    // [TestCase(ProjectMemberRole.WorkspaceLead, ProjectMemberRole.Collaborator)]
    // [TestCase(ProjectMemberRole.WorkspaceLead, ProjectMemberRole.Admin)]
    // public async Task ShouldUpdateUserInProject(ProjectMemberRole currentRole, ProjectMemberRole newRole)
    // {
    //     var projectUserManagementService = GetProjectUserManagementService();
    //     await SeedDatabase(new List<string>
    //     {
    //         TestUserId,
    //     }, TestProjectAcronym, currentRole);
    //     var projectMember = new ProjectMember(TestUserId, newRole);
    //     await projectUserManagementService.UpdateUserInProject(TestProjectAcronym, projectMember);
    //
    //     await using var context = await _mockFactory.Object.CreateDbContextAsync();
    //     var projectUser = await context.Project_Users.FirstAsync();
    //     Assert.Multiple(() =>
    //     {
    //         Assert.That(projectUser.IsAdmin, Is.EqualTo(newRole is ProjectMemberRole.Admin or ProjectMemberRole.WorkspaceLead));
    //         Assert.That(projectUser.IsDataApprover, Is.EqualTo(newRole == ProjectMemberRole.WorkspaceLead));
    //     });
    // }
    //
    // [Test]
    
    // public async Task ShouldThrowException_WhenProjectNotFoundOnUserUpdate()
    // {
    //     const string nonExistentProjectAcronym = "NOTFOUND";
    //     var projectUserManagementService = GetProjectUserManagementService();
    //     await SeedDatabase(new List<string>
    //     {
    //         TestUserId
    //     });
    //
    //     Assert.ThrowsAsync<ProjectNotFoundException>(async () =>
    //     {
    //         await projectUserManagementService.UpdateUserInProject(nonExistentProjectAcronym, 
    //             new ProjectMember(TestUserId, ProjectMemberRole.Collaborator));
    //     });
    // }
    //
    // [Test]
    
    // public async Task ShouldThrowException_WhenUserNotFoundOnUserUpdate()
    // {
    //     const string nonExistentUserId = "THIS_IS_NOT_AN_ID";
    //     var projectUserManagementService = GetProjectUserManagementService();
    //     await SeedDatabase(new List<string>
    //     {
    //         TestUserId,
    //     });
    //     var projectMember = new ProjectMember(nonExistentUserId, ProjectMemberRole.Admin);
    //     Assert.ThrowsAsync<UserNotFoundException>(async () =>
    //     {
    //         await projectUserManagementService.UpdateUserInProject(TestProjectAcronym, projectMember);
    //     });
    // }
    //
    // [Test]
    
    // [TestCase(ProjectMemberRole.Collaborator, ProjectMemberRole.Collaborator)]
    // [TestCase(ProjectMemberRole.Admin, ProjectMemberRole.Admin)]
    // [TestCase(ProjectMemberRole.WorkspaceLead, ProjectMemberRole.WorkspaceLead)]
    // public async Task ShouldDoNothing_WhenUserRoleHasNotChanged(ProjectMemberRole currenRole, ProjectMemberRole newRole)
    // {
    //     var projectUserManagementService = GetProjectUserManagementService();
    //     await SeedDatabase(new List<string>
    //     {
    //         TestUserId,
    //     }, TestProjectAcronym, currenRole);
    //     var projectMember = new ProjectMember(TestUserId, newRole);
    //     await using var context = await _mockFactory.Object.CreateDbContextAsync();
    //     var projectUser = await context.Project_Users.FirstAsync();
    //     Assert.Multiple(() =>
    //     {
    //         Assert.That(projectUser.IsAdmin, Is.EqualTo(currenRole is ProjectMemberRole.Admin or ProjectMemberRole.WorkspaceLead));
    //         Assert.That(projectUser.IsDataApprover, Is.EqualTo(currenRole is ProjectMemberRole.WorkspaceLead));
    //     });
    //     await projectUserManagementService.UpdateUserInProject(TestProjectAcronym, projectMember);
    //     await projectUserManagementService.UpdateUserInProject(TestProjectAcronym, projectMember);
    //     await projectUserManagementService.UpdateUserInProject(TestProjectAcronym, projectMember);
    //     projectUser = await context.Project_Users.FirstAsync();
    //     Assert.Multiple(() =>
    //     {
    //         Assert.That(projectUser.IsAdmin, Is.EqualTo(newRole is ProjectMemberRole.Admin or ProjectMemberRole.WorkspaceLead));
    //         Assert.That(projectUser.IsDataApprover, Is.EqualTo(newRole == ProjectMemberRole.WorkspaceLead));
    //     });
    // }
    //
    // [Test]
    
    // [TestCase(11, 0)]
    // [TestCase(12, 10)]
    // [TestCase(0, 11)]
    // [TestCase(0, 0)]
    // [TestCase(6, 6)]
    // public async Task ShouldGetUsersFromProject(int userCount, int dummyCount)
    // {
    //     var projectUserManagementService = GetProjectUserManagementService();
    //     await SeedDatabase(Enumerable.Range(0, userCount).Select(i => $"{TestUserId}{i}").ToList());
    //
    //     var dummyProjectAcronym = $"{TestProjectAcronym}DUMMY";
    //     await SeedDatabase(Enumerable.Range(0, dummyCount).Select(i => $"{TestUserId}{i}").ToList(),
    //         dummyProjectAcronym);
    //
    //     var projectUsers = await projectUserManagementService.GetProjectUsersAsync(TestProjectAcronym);
    //
    //     var context = await _mockFactory.Object.CreateDbContextAsync();
    //     var totalUserCount = await context.Project_Users.CountAsync();
    //     Assert.Multiple(() =>
    //     {
    //         Assert.That(projectUsers.Count(), Is.EqualTo(userCount));
    //         Assert.That(totalUserCount, Is.EqualTo(userCount + dummyCount));
    //     });
    // }
    //
    // [Test]
    
    // [TestCase(ProjectMemberRole.Remove)]
    // [TestCase(ProjectMemberRole.Collaborator)]
    // [TestCase(ProjectMemberRole.Admin)]
    // [TestCase(ProjectMemberRole.WorkspaceLead)]
    // public async Task AddingNewUsersWithBatchShouldAddNewUsers(ProjectMemberRole role)
    // {
    //     var projectUserManagementService = GetProjectUserManagementService();
    //     await SeedDatabase();
    //     var testUsers = _testUserIds.Select(t => new ProjectMember(t, role)).ToList();
    //     await projectUserManagementService.BatchUpdateUsersInProject(TestProjectAcronym, testUsers);
    //
    //     await using var context = await _mockFactory.Object.CreateDbContextAsync();
    //     var projectUsers = await context.Project_Users
    //         .Include(p => p.Project)
    //         .ToListAsync();
    //     var projectId = await context.Projects
    //         .Where(p => p.Project_Acronym_CD == TestProjectAcronym)
    //         .Select(p => p.Project_ID)
    //         .SingleAsync();
    //
    //     if (role == ProjectMemberRole.Remove)
    //     {
    //         Assert.That(projectUsers, Has.Count.EqualTo(0));
    //     }
    //     else
    //     {
    //         Assert.Multiple(() =>
    //         { 
    //             Assert.That(projectUsers, Has.Count.EqualTo(testUsers.Count));
    //             Assert.That(projectUsers[0].Project.Project_ID, Is.EqualTo(projectId));
    //             Assert.That(projectUsers.Select(p => p.User_ID), Is.EquivalentTo(testUsers.Select(t => t.UserId)));
    //             Assert.That(projectUsers.All(t => t.IsAdmin), Is.EqualTo(role is ProjectMemberRole.Admin or ProjectMemberRole.WorkspaceLead));
    //             Assert.That(projectUsers.All(t => t.IsDataApprover), Is.EqualTo(role is ProjectMemberRole.WorkspaceLead));
    //         
    //         });
    //     }
    // }
    //
    // [Test]
    
    // [TestCase(ProjectMemberRole.Collaborator, ProjectMemberRole.Admin)]
    // [TestCase(ProjectMemberRole.Collaborator, ProjectMemberRole.WorkspaceLead)]
    // [TestCase(ProjectMemberRole.Admin, ProjectMemberRole.Collaborator)]
    // [TestCase(ProjectMemberRole.Admin, ProjectMemberRole.WorkspaceLead)]
    // [TestCase(ProjectMemberRole.WorkspaceLead, ProjectMemberRole.Collaborator)]
    // [TestCase(ProjectMemberRole.WorkspaceLead, ProjectMemberRole.Admin)]
    // public async Task UpdatingNewUsersWithBatchShouldUpdateUsers(ProjectMemberRole currentRole, ProjectMemberRole newRole)
    // {
    //     var projectUserManagementService = GetProjectUserManagementService();
    //     await SeedDatabase(_testUserIds, TestProjectAcronym, currentRole);
    //     var testUsers = _testUserIds.Select(t => new ProjectMember(t, newRole)).ToList();
    //     await projectUserManagementService.BatchUpdateUsersInProject(TestProjectAcronym, testUsers);
    //
    //     await using var context = await _mockFactory.Object.CreateDbContextAsync();
    //     var projectUsers = await context.Project_Users
    //         .Include(p => p.Project)
    //         .ToListAsync();
    //     var projectId = await context.Projects
    //         .Where(p => p.Project_Acronym_CD == TestProjectAcronym)
    //         .Select(p => p.Project_ID)
    //         .SingleAsync();
    //
    //     Assert.Multiple(() =>
    //     { 
    //         Assert.That(projectUsers, Has.Count.EqualTo(testUsers.Count));
    //         Assert.That(projectUsers[0].Project.Project_ID, Is.EqualTo(projectId));
    //         Assert.That(projectUsers.Select(p => p.User_ID), Is.EquivalentTo(testUsers.Select(t => t.UserId)));
    //         Assert.That(projectUsers.All(t => t.IsAdmin), Is.EqualTo(newRole is ProjectMemberRole.Admin or ProjectMemberRole.WorkspaceLead));
    //         Assert.That(projectUsers.All(t => t.IsDataApprover), Is.EqualTo(newRole is ProjectMemberRole.WorkspaceLead));
    //     
    //     });
    //     
    // }
    //
    // [Test]
    
    // [TestCase(ProjectMemberRole.Collaborator)]
    // [TestCase(ProjectMemberRole.Admin)]
    // [TestCase(ProjectMemberRole.WorkspaceLead)]
    // public async Task RemovingUsersWithBatchUpdateShouldRemoveAllUsers(ProjectMemberRole currentRole)
    // {
    //     var projectUserManagementService = GetProjectUserManagementService();
    //     await SeedDatabase(_testUserIds, TestProjectAcronym, currentRole);
    //     var testUsers = _testUserIds.Select(t => new ProjectMember(t, ProjectMemberRole.Remove)).ToList();
    //     await projectUserManagementService.BatchUpdateUsersInProject(TestProjectAcronym, testUsers);
    //
    //     await using var context = await _mockFactory.Object.CreateDbContextAsync();
    //     var projectUsers = await context.Project_Users
    //         .Include(p => p.Project)
    //         .ToListAsync();
    //     var projectId = await context.Projects
    //         .Where(p => p.Project_Acronym_CD == TestProjectAcronym)
    //         .Select(p => p.Project_ID)
    //         .SingleAsync();
    //     Assert.That(projectUsers, Has.Count.EqualTo(0));
    //     
    // }
    // // each test case should have 5 roles to match 5 user ids set in Testings.cs
    // // there are 16 possible combinations of roles, and one extra test case to test for remove all
    // [Test]
    
    // [TestCase(new[] {ProjectMemberRole.Admin, ProjectMemberRole.Admin, ProjectMemberRole.Collaborator, ProjectMemberRole.Remove, ProjectMemberRole.Remove}, 
    //     new[] {ProjectMemberRole.Collaborator, ProjectMemberRole.Remove, ProjectMemberRole.Admin, ProjectMemberRole.Collaborator, ProjectMemberRole.WorkspaceLead})]
    // [TestCase(new[] {ProjectMemberRole.Admin, ProjectMemberRole.Admin, ProjectMemberRole.Collaborator, ProjectMemberRole.Collaborator, ProjectMemberRole.Remove}, 
    //     new[] {ProjectMemberRole.Admin, ProjectMemberRole.WorkspaceLead, ProjectMemberRole.Remove, ProjectMemberRole.Collaborator, ProjectMemberRole.WorkspaceLead})]
    // [TestCase(new[] {ProjectMemberRole.WorkspaceLead, ProjectMemberRole.WorkspaceLead, ProjectMemberRole.WorkspaceLead, ProjectMemberRole.WorkspaceLead, ProjectMemberRole.Remove}, 
    //     new[] {ProjectMemberRole.Remove, ProjectMemberRole.Collaborator, ProjectMemberRole.Admin, ProjectMemberRole.WorkspaceLead, ProjectMemberRole.Admin})]
    // [TestCase(new[] {ProjectMemberRole.Remove, ProjectMemberRole.Remove, ProjectMemberRole.Remove, ProjectMemberRole.Remove, ProjectMemberRole.Remove}, 
    //     new[] {ProjectMemberRole.Remove, ProjectMemberRole.Remove, ProjectMemberRole.Remove, ProjectMemberRole.Remove, ProjectMemberRole.Remove})]
    // public async Task BatchUpdateShouldBeAbleToHandleMixtureOfAddUpdateAndRemove(ProjectMemberRole[] currentRoles, ProjectMemberRole[] newRoles)
    // {
    //     var projectUserManagementService = GetProjectUserManagementService();
    //     var existingUsers = _testUserIds.Select((id, index) => currentRoles[index] != ProjectMemberRole.Remove ? (id, currentRoles[index]) : (null, ProjectMemberRole.Remove))
    //         .Where(u => u.id is not null).Cast<(string id, ProjectMemberRole role)>().ToList();
    //     await SeedDatabase(existingUsers);
    //     var newUsers = _testUserIds.Select((id, index) => new ProjectMember(id, newRoles[index])).ToList();
    //     await projectUserManagementService.BatchUpdateUsersInProject(TestProjectAcronym, newUsers);
    //     await using var context = await _mockFactory.Object.CreateDbContextAsync();
    //     var projectUsers = await context.Project_Users
    //         .Include(p => p.Project)
    //         .ToListAsync();
    //     var projectId = await context.Projects
    //         .Where(p => p.Project_Acronym_CD == TestProjectAcronym)
    //         .Select(p => p.Project_ID)
    //         .SingleAsync();
    //
    //     Assert.Multiple(() =>
    //     { 
    //         Assert.That(projectUsers, Has.Count.EqualTo(newRoles.Count(role => role != ProjectMemberRole.Remove)));
    //         Assert.That(projectUsers.FirstOrDefault()?.Project.Project_ID, Is.EqualTo(projectUsers.IsNullOrEmpty() ? null : projectId));
    //         Assert.That(projectUsers.Select(p => p.User_ID), Is.EquivalentTo(newUsers.Where(user => user.Role != ProjectMemberRole.Remove).Select(t => t.UserId)));
    //         Assert.That(projectUsers.Count(t => t.IsAdmin), Is.EqualTo(newRoles.Count(role => role is ProjectMemberRole.Admin or ProjectMemberRole.WorkspaceLead)));
    //         Assert.That(projectUsers.Count(t => t.IsDataApprover), Is.EqualTo(newRoles.Count(role => role is ProjectMemberRole.WorkspaceLead)));
    //     
    //     });
    //     
    // }
    //
    // [Test]
    
    // [TestCase(ProjectMemberRole.Remove)]
    // [TestCase(ProjectMemberRole.Collaborator)]
    // [TestCase(ProjectMemberRole.Admin)]
    // [TestCase(ProjectMemberRole.WorkspaceLead)]
    // public async Task BatchUpdateShouldSendInviteToNewUserAndAddToProject(ProjectMemberRole role)
    // {
    //     var projectUserManagementService = GetProjectUserManagementService();
    //     await SeedDatabase();
    //     var testUser = new ProjectMember("ID_DOES_NOT_EXIST", role)
    //     {
    //         UserHasBeenInvitedToDatahub = false,
    //     };
    //     await projectUserManagementService.BatchUpdateUsersInProject(TestProjectAcronym, new List<ProjectMember>(){testUser});
    //
    //     await using var context = await _mockFactory.Object.CreateDbContextAsync();
    //     var projectUsers = await context.Project_Users
    //         .Include(p => p.Project)
    //         .ToListAsync();
    //     var projectId = await context.Projects
    //         .Where(p => p.Project_Acronym_CD == TestProjectAcronym)
    //         .Select(p => p.Project_ID)
    //         .SingleAsync();
    //
    //     if (role == ProjectMemberRole.Remove)
    //     {
    //         Assert.Multiple(() =>
    //         {
    //             Assert.That(projectUsers, Has.Count.EqualTo(0));
    //             Assert.That(testUser.UserId, Is.EqualTo("ID_DOES_NOT_EXIST"));
    //         });
    //     }
    //     else
    //     {
    //         Assert.Multiple(() =>
    //         { 
    //             Assert.That(projectUsers, Has.Count.EqualTo(1));
    //             Assert.That(projectUsers[0].Project.Project_ID, Is.EqualTo(projectId));
    //             Assert.That(projectUsers.First().User_ID, Is.EqualTo(TestUserId));
    //             Assert.That(projectUsers.All(t => t.IsAdmin), Is.EqualTo(role is ProjectMemberRole.Admin or ProjectMemberRole.WorkspaceLead));
    //             Assert.That(projectUsers.All(t => t.IsDataApprover), Is.EqualTo(role is ProjectMemberRole.WorkspaceLead));
    //         
    //         });
    //     }
    // }
    //
    // [Test]
    
    // public async Task ShouldSendTerraformVariableUpdateOnUserAdd()
    // {
    //     var projectUserManagementService = GetProjectUserManagementService();
    //     
    //     _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
    //         It.IsAny<string>()), Times.Never);
    //     
    //     await SeedDatabase();
    //     await projectUserManagementService.AddUserToProject(TestProjectAcronym, TestUserId);
    //
    //     _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
    //         It.Is<string>(s => s == TerraformTemplate.VariableUpdate)), Times.Once);
    // }
    //
    // [Test]
    
    // public async Task ShouldSendTerraformVariableUpdateOnUserRemove()
    // {
    //     var projectUserManagementService = GetProjectUserManagementService();
    //     
    //     _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
    //         It.IsAny<string>()), Times.Never);
    //     
    //     await SeedDatabase(new List<string>{TestUserId});
    //     await projectUserManagementService.RemoveUserFromProject(TestProjectAcronym, TestUserId);
    //
    //     _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
    //         It.Is<string>(s => s == TerraformTemplate.VariableUpdate)), Times.Once);
    // }
    //
    // [Test]
    
    // public async Task ShouldSendTerraformVariableUpdateOnUserUpdated()
    // {
    //     
    //     var projectUserManagementService = GetProjectUserManagementService();
    //
    //     _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
    //         It.IsAny<string>()), Times.Never);
    //
    //     await SeedDatabase(new List<string>{TestUserId});
    //     var projectMember = new ProjectMember(TestUserId, ProjectMemberRole.WorkspaceLead);
    //     await projectUserManagementService.UpdateUserInProject(TestProjectAcronym, projectMember);
    //
    //     _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
    //         It.Is<string>(s => s == TerraformTemplate.VariableUpdate)), Times.Once);
    // }
    private void SeedDatabase(DatahubProjectDBContext context)
    {
        const int count = 5;
        const int usersPerProject = 10;

        var projects = Enumerable.Range(1, count)
            .Select(i => new Datahub_Project
            {
                Project_Name = $"Project {i}",
                Project_Acronym_CD = $"{i}",
                Project_Status_Desc = "Active",
                Sector_Name = $"Sector {i}",
            })
            .ToList();

        var users = Enumerable.Range(1, count * usersPerProject)
            .Select(i => new PortalUser()
            {
                Id = i,
                GraphGuid = Guid.NewGuid().ToString(),
                Email = $"{i}@email.com",
                DisplayName = $"{i} Smith"
            })
            .ToList();

        var projectUsers = users.Select(u => new Datahub_Project_User()
            {
                PortalUser = u,
                Project = projects[u.Id % count],
                RoleId = u.Id % 2 == 0
                    ? (int)Project_Role.RoleNames.WorkspaceLead
                    : (int)Project_Role.RoleNames.Collaborator
            })
            .ToList();

        context.AddRange(projects);
        context.AddRange(users);
        context.AddRange(projectUsers);

        _testCurrentUser = new PortalUser()
        {
            Id = 999,
            GraphGuid = Guid.NewGuid().ToString(),
            Email = TestUserEmail,
        };

        context.PortalUsers.Add(_testCurrentUser);
        context.SaveChanges();
    }

    private ProjectUserManagementService GetProjectUserManagementService()
    {
        var projectUserManagementService = new ProjectUserManagementService(
            Mock.Of<ILogger<ProjectUserManagementService>>(),
            _mockFactory.Object,
            _mockUserInformationService.Object,
            _mockIMSGraphService.Object,
            _mockRequestManagementService.Object,
            _mockResourceManagementService.Object,
            _mockUserEnrollmentService.Object,
            _mockDatahubAuditingService.Object);

        return projectUserManagementService;
    }
}