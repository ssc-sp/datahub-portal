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