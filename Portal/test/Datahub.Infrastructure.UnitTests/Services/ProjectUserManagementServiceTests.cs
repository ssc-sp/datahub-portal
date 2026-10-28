using System.Linq.Dynamic.Core;
using System.Net.Mail;
using Datahub.Application.Commands;
using Datahub.Application.Services;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Data;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Users;
using Datahub.Core.Services;
using Datahub.Core.Services.Projects;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Security;
using Datahub.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
            .Setup(f => f.GetEntraUserAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => _dbContext.PortalUsers.First(u => u.EntraUser != null && u.EntraUser.GraphGuid == id));

        _mockUserInformationService
            .Setup(f => f.CreatePortalEntraUserAsync(It.IsAny<string>()))
            .Callback((string graphId) =>
            {
                var pu = new PortalUser()
                {
                    Email = TestUserEmail,
                    DisplayName = TestUserEmail
                };
                pu.EntraUser = new EntraUser
                {
                    GraphGuid = graphId,
                    PortalUser = pu
                };
                _dbContext.PortalUsers.Add(pu);
                _dbContext.SaveChanges();
            });

        _mockIMSGraphService = new Mock<IMSGraphService>();
        _mockIMSGraphService
            .Setup(f => f.GetUserAsync(It.Is<string>(s => _testUserIds.Contains(s) || s == TestUserGraphGuid),
                CancellationToken.None))
            .Returns((string id, CancellationToken _) => Task.FromResult(new GraphUser
                {
                    MailAddress = new MailAddress(TestUserEmail),
                    DisplayName = TestUserEmail,
                Id = id,
                })
            );

        _mockRequestManagementService = new Mock<IRequestManagementService>();
        _mockRequestManagementService
            .Setup(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(), It.IsAny<TerraformTemplate>(),
                It.IsAny<PortalUser>()))
            .ReturnsAsync(true);

        _mockResourceManagementService = new Mock<IResourceMessagingService>();

        _mockUserEnrollmentService = new Mock<IUserEnrollmentService>();
        _mockUserEnrollmentService
            .Setup(f => f.SendUserDatahubPortalInvite(It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(TestUserGraphGuid);

        _mockDatahubAuditingService = new Mock<IDatahubAuditingService>();

        var mockMemoryCache = new Mock<IMemoryCache>();

        _serviceAuthManager =
            new ServiceAuthManager(mockMemoryCache.Object, _mockFactory.Object, NullLogger<ServiceAuthManager>.Instance);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    [Test]
    public async Task GetProjectsForUser_WhenEntraMembershipIsActive_IncludesProjectWithRemovedMember()
    {
        var (project, userIdentifier) = await AddMembershipScenarioAsync(
            externalUser: false,
            currentUserRole: Project_Role.RoleNames.Collaborator,
            otherUserRole: Project_Role.RoleNames.Removed);

        var projects = await GetProjectUserManagementService().GetProjectsForUser(graphUserGuid: userIdentifier);

        Assert.That(projects.Select(p => p.Project_Acronym_CD), Does.Contain(project.Project_Acronym_CD));
    }

    [Test]
    public async Task GetProjectsForUser_WhenEntraMembershipIsRemoved_ExcludesProject()
    {
        var (project, userIdentifier) = await AddMembershipScenarioAsync(
            externalUser: false,
            currentUserRole: Project_Role.RoleNames.Removed,
            otherUserRole: Project_Role.RoleNames.Collaborator);

        var projects = await GetProjectUserManagementService().GetProjectsForUser(graphUserGuid: userIdentifier);

        Assert.That(projects.Select(p => p.Project_Acronym_CD), Does.Not.Contain(project.Project_Acronym_CD));
    }

    [Test]
    public async Task GetProjectsForUser_WhenExternalMembershipIsActive_IncludesProjectWithRemovedMember()
    {
        var (project, userIdentifier) = await AddMembershipScenarioAsync(
            externalUser: true,
            currentUserRole: Project_Role.RoleNames.Guest,
            otherUserRole: Project_Role.RoleNames.Removed);

        var projects = await GetProjectUserManagementService().GetProjectsForUser(gccfUserId: userIdentifier);

        Assert.That(projects.Select(p => p.Project_Acronym_CD), Does.Contain(project.Project_Acronym_CD));
    }

    [Test]
    public async Task GetProjectsForUser_WhenExternalMembershipIsRemoved_ExcludesProject()
    {
        var (project, userIdentifier) = await AddMembershipScenarioAsync(
            externalUser: true,
            currentUserRole: Project_Role.RoleNames.Removed,
            otherUserRole: Project_Role.RoleNames.Collaborator);

        var projects = await GetProjectUserManagementService().GetProjectsForUser(gccfUserId: userIdentifier);

        Assert.That(projects.Select(p => p.Project_Acronym_CD), Does.Not.Contain(project.Project_Acronym_CD));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task GetProjectsForUser_WhenExternalUserIsInactive_ExcludesProject(bool isDeactivated)
    {
        var (project, userIdentifier) = await AddMembershipScenarioAsync(
            externalUser: true,
            currentUserRole: Project_Role.RoleNames.Guest,
            otherUserRole: Project_Role.RoleNames.Collaborator,
            externalUserExpiry: isDeactivated ? DateTimeOffset.UtcNow.AddDays(1) : DateTimeOffset.UtcNow.AddDays(-1),
            externalUserDeactivatedAt: isDeactivated ? DateTimeOffset.UtcNow : null);

        var projects = await GetProjectUserManagementService().GetProjectsForUser(gccfUserId: userIdentifier);

        Assert.That(projects.Select(p => p.Project_Acronym_CD), Does.Not.Contain(project.Project_Acronym_CD));
    }

    [Test]
    public async Task GetProjectsForUser_WhenProjectIsDeleted_ExcludesProject()
    {
        var (project, userIdentifier) = await AddMembershipScenarioAsync(
            externalUser: false,
            currentUserRole: Project_Role.RoleNames.Collaborator,
            otherUserRole: Project_Role.RoleNames.Collaborator,
            projectIsDeleted: true);

        var projects = await GetProjectUserManagementService().GetProjectsForUser(graphUserGuid: userIdentifier);

        Assert.That(projects.Select(p => p.Project_Acronym_CD), Does.Not.Contain(project.Project_Acronym_CD));
    }

    [Test]
    [Ignore("Needs to be validated")]
    public async Task ShouldProcessEmptyProjectUserCommandsTest()
    {
        var projectUserManagementService = GetProjectUserManagementService();

        var result =
            await projectUserManagementService.ProcessProjectUserCommandsAsync(
                new List<ProjectUserUpdateCommand>(),
                new List<ProjectUserAddEntraUserCommand>(), "");

        Assert.That(result, Is.True);

        _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
            It.IsAny<TerraformTemplate>(), It.IsAny<PortalUser>()), Times.Never);
    }

    [Test]
    [TestCase((int)Project_Role.RoleNames.WorkspaceLead, Ignore = "Needs to be validated")]
    [TestCase((int)Project_Role.RoleNames.Admin, Ignore = "Needs to be validated")]
    [TestCase((int)Project_Role.RoleNames.Collaborator, Ignore = "Needs to be validated")]
    [TestCase((int)Project_Role.RoleNames.Guest, Ignore = "Needs to be validated")]
    [TestCase((int)Project_Role.RoleNames.Removed, Ignore = "Needs to be validated")]
    public async Task ShouldProcessAddExistingUserCommandTest(int roleId)
    {
        _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
            It.IsAny<TerraformTemplate>(), It.IsAny<PortalUser>()), Times.Never);

        var projectUserManagementService = GetProjectUserManagementService();

        var existingProjectUser = await _dbContext.UserRolesLinks
            .AsNoTracking()
            .Include(u => u.Project)
            .Include(u => u.PortalUser)
            .FirstAsync();

        var foreignProject = await _dbContext.Projects
            .FirstAsync(p => existingProjectUser.Project.Project_Acronym_CD != p.Project_Acronym_CD);

        var command = new ProjectUserAddEntraUserCommand
        {
            DisplayName = existingProjectUser.PortalUser.DisplayName,
            Email = existingProjectUser.PortalUser.Email,
            GraphGuid = existingProjectUser.PortalUser.EntraUser!.GraphGuid,
            ProjectAcronym = foreignProject.Project_Acronym_CD,
            RoleId = roleId,
        };

        var result =
            await projectUserManagementService.ProcessProjectUserCommandsAsync(
                new List<ProjectUserUpdateCommand>(),
                new List<ProjectUserAddEntraUserCommand> { command }, "");

        if (roleId == (int)Project_Role.RoleNames.Removed)
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

        var command = new ProjectUserAddEntraUserCommand
        {
            DisplayName = TestUserEmail,
            Email = TestUserEmail,
            GraphGuid = ProjectUserAddEntraUserCommand.NEW_USER_GUID,
            ProjectAcronym = firstProject.Project_Acronym_CD,
            RoleId = (int)Project_Role.RoleNames.Collaborator,
        };

        var result =
            await projectUserManagementService.ProcessProjectUserCommandsAsync(
                new List<ProjectUserUpdateCommand>(),
                new List<ProjectUserAddEntraUserCommand> { command }, "");

        Assert.That(result, Is.True);

        _mockUserEnrollmentService.Verify(f => f.SendUserDatahubPortalInvite(It.IsAny<string?>(), It.IsAny<string?>()),
            Times.Once);
        _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
            It.IsAny<TerraformTemplate>(), It.IsAny<PortalUser>()), Times.Once);

        var projectUser = _dbContext.UserRolesLinks
            .Include(u => u.Project)
            .Include(u => u.PortalUser)
            .First(u => u.PortalUser.EntraUser!.GraphGuid == TestUserGraphGuid);

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

        var existingProjectUser = await _dbContext.UserRolesLinks
            .AsNoTracking()
            .Include(u => u.Project)
            .Include(u => u.PortalUser)
            .FirstAsync();

        var command = new ProjectUserAddEntraUserCommand
        {
            DisplayName = existingProjectUser.PortalUser.DisplayName,
            Email = existingProjectUser.PortalUser.Email,
            GraphGuid = existingProjectUser.PortalUser.EntraUser!.GraphGuid,
            ProjectAcronym = "AbsolutelyNotARealProject",
            RoleId = (int)Project_Role.RoleNames.Collaborator,
        };

        var result =
            await projectUserManagementService.ProcessProjectUserCommandsAsync(
                new List<ProjectUserUpdateCommand>(),
                new List<ProjectUserAddEntraUserCommand> { command }, "");

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

        var existingProjectUser = await _dbContext.UserRolesLinks
            .AsNoTracking()
            .Include(u => u.Project)
            .Include(u => u.PortalUser)
            .FirstAsync();

        var datahubProject = await _dbContext.Projects
            .FirstAsync(p => existingProjectUser.Project.Project_Acronym_CD == p.Project_Acronym_CD);

        var command = new ProjectUserAddEntraUserCommand
        {
            DisplayName = existingProjectUser.PortalUser.DisplayName,
            Email = existingProjectUser.PortalUser.Email,
            GraphGuid = existingProjectUser.PortalUser.EntraUser!.GraphGuid,
            ProjectAcronym = datahubProject.Project_Acronym_CD,
            RoleId = (int)Project_Role.RoleNames.Collaborator,
        };

        var result =
            await projectUserManagementService.ProcessProjectUserCommandsAsync(
                new List<ProjectUserUpdateCommand>(),
                new List<ProjectUserAddEntraUserCommand> { command }, "");

        Assert.That(result, Is.False);

        _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
            It.IsAny<TerraformTemplate>(), It.IsAny<PortalUser>()), Times.Never);
    }

    [Test]
    [TestCase((int)Project_Role.RoleNames.WorkspaceLead, Ignore = "Needs to be validated")]
    [TestCase((int)Project_Role.RoleNames.Admin, Ignore = "Needs to be validated")]
    [TestCase((int)Project_Role.RoleNames.Collaborator, Ignore = "Needs to be validated")]
    [TestCase((int)Project_Role.RoleNames.Guest, Ignore = "Needs to be validated")]
    [TestCase((int)Project_Role.RoleNames.Removed, Ignore = "Needs to be validated")]
    public async Task ShouldProcessUpdateUserCommandTest(int roleId)
    {
        var projectUserManagementService = GetProjectUserManagementService();

        _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
            It.IsAny<TerraformTemplate>(), It.IsAny<PortalUser>()), Times.Never);

        var existingProjectUser = await _dbContext.UserRolesLinks
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
                new List<ProjectUserAddEntraUserCommand>(), "");

        Assert.That(result, Is.True);

        if (roleId == (int)Project_Role.RoleNames.Removed)
        {
            // double check that the project user is gone
            var projectUser = await _dbContext.UserRolesLinks
                .AsNoTracking()
                .Include(u => u.PortalUser).Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.PortalUser.EntraUser!.GraphGuid == existingProjectUser.PortalUser.EntraUser!.GraphGuid);

            Assert.That(projectUser.Role.Id, Is.EqualTo((int)Project_Role.RoleNames.Removed));
        }

        //_mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
        //    It.IsAny<string>()), Times.Once);
    }

    [Test]
    [Ignore("Needs to be validated")]
    public async Task ShouldGetProjectUsersAsync()
    {
        var projectUserManagementService = GetProjectUserManagementService();

        var seededProjectUsers = await _dbContext.UserRolesLinks
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
            })
            .ToList();

        var users = new List<PortalUser>();
        for (int i = 1; i <= count * usersPerProject; i++)
        {
            var pu = new PortalUser
            {
                Id = i,
                Email = $"{i}@email.com",
                DisplayName = $"{i} Smith"
            };
            pu.EntraUser = new EntraUser
            {
                GraphGuid = Guid.NewGuid().ToString(),
                PortalUser = pu
            };
            users.Add(pu);
        }

        var projectUsers = users.Select(u => new UserRoleLinks()
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

        var currentUser = new PortalUser()
        {
            Id = 999,
            Email = TestUserEmail,
        };
        currentUser.EntraUser = new EntraUser
        {
            GraphGuid = Guid.NewGuid().ToString(),
            PortalUser = currentUser
        };
        _testCurrentUser = currentUser;

        context.PortalUsers.Add(_testCurrentUser);
        context.SaveChanges();
    }

    private async Task<(Datahub_Project Project, string UserIdentifier)> AddMembershipScenarioAsync(
        bool externalUser,
        Project_Role.RoleNames currentUserRole,
        Project_Role.RoleNames otherUserRole,
        bool projectIsDeleted = false,
        DateTimeOffset? externalUserExpiry = null,
        DateTimeOffset? externalUserDeactivatedAt = null)
    {
        var userIdentifier = Guid.NewGuid().ToString();
        var currentUser = new PortalUser
        {
            Email = $"{userIdentifier}@email.com",
            DisplayName = "Current user"
        };

        if (externalUser)
        {
            var externalUserProfile = new ExternalUser
            {
                ExternalSubject = userIdentifier,
                FirstName = "Current",
                LastName = "User",
                Organization = "Datahub",
                UserExpiryDate = externalUserExpiry ?? DateTimeOffset.UtcNow.AddDays(1),
                UserDeactivatedAt = externalUserDeactivatedAt,
                PortalUser = currentUser
            };

            _dbContext.ExternalUsers.Add(externalUserProfile);
            await _dbContext.SaveChangesAsync();
            currentUser.ExternalUser = externalUserProfile;
        }
        else
        {
            currentUser.EntraUser = new EntraUser
            {
                GraphGuid = userIdentifier,
                PortalUser = currentUser
            };
        }

        var otherUser = new PortalUser
        {
            Email = $"{Guid.NewGuid()}@email.com",
            DisplayName = "Other user"
        };
        otherUser.EntraUser = new EntraUser
        {
            GraphGuid = Guid.NewGuid().ToString(),
            PortalUser = otherUser
        };

        var project = new Datahub_Project
        {
            Project_Name = "Membership filtering test project",
            Project_Acronym_CD = Guid.NewGuid().ToString("N")[..10].ToUpperInvariant(),
            Project_Status_Desc = "Active",
            Deleted_DT = projectIsDeleted ? DateTime.UtcNow.AddMinutes(-1) : null
        };

        _dbContext.UserRolesLinks.AddRange(
            new UserRoleLinks
            {
                PortalUser = currentUser,
                Project = project,
                RoleId = (int)currentUserRole
            },
            new UserRoleLinks
            {
                PortalUser = otherUser,
                Project = project,
                RoleId = (int)otherUserRole
            });
        await _dbContext.SaveChangesAsync();

        return (project, userIdentifier);
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
