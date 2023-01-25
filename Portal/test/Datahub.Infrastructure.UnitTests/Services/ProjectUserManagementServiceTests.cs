using System.Net.Mail;
using Datahub.Core.Data;
using Datahub.Core.Data.Project;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services;
using Datahub.Core.Services.Projects;
using Datahub.Core.Services.UserManagement;
using Datahub.Infrastructure.Services;
using Datahub.Shared.Entities;
using Datahub.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Datahub.Infrastructure.UnitTests.Services;

using static Testing;

public class ProjectUserManagementServiceTests
{
    private Mock<IDbContextFactory<DatahubProjectDBContext>> _mockFactory = null!;
    private Mock<IUserInformationService> _mockUserInformationService = null!;
    // ReSharper disable once InconsistentNaming
    private Mock<IMSGraphService> _mockIMSGraphService = null!;
    private Mock<IRequestManagementService> _mockRequestManagementService = null!;


    [SetUp]
    public void Setup()
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<DatahubProjectDBContext>()
                .UseInMemoryDatabase("ProjectUserManagementServiceTests");
        var dbContext = new DatahubProjectDBContext(optionsBuilder.Options);
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();

        // create a mock factory to return the db context when CreateDbContextAsync is called
        _mockFactory = new Mock<IDbContextFactory<DatahubProjectDBContext>>();
        _mockFactory
            .Setup(f => f.CreateDbContextAsync(CancellationToken.None))
            .ReturnsAsync(() => new DatahubProjectDBContext(optionsBuilder.Options));

        // create a mock user information service to return the current (admin) user when GetUserIdString is called
        _mockUserInformationService = new Mock<IUserInformationService>();
        _mockUserInformationService
            .Setup(f => f.GetUserIdString())
            .ReturnsAsync(TestAdminUserId);

        _mockIMSGraphService = new Mock<IMSGraphService>();
        _mockIMSGraphService
            .Setup(f => f.GetUserAsync(It.Is<string>(s => TEST_USER_IDS.Contains(s) || s == TestUserId), CancellationToken.None))
            .Returns((string id, CancellationToken _) => Task.FromResult(new GraphUser
                {
                    mailAddress = new MailAddress(TestUserEmail),
                    Id = id,
                })
            );

        _mockRequestManagementService = new Mock<IRequestManagementService>();
        _mockRequestManagementService
            .Setup(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(), It.IsAny<string>()))
            .ReturnsAsync(true);
    }

    [Test]
    public async Task ShouldAddUserToProject()
    {
        var projectUserManagementService = GetProjectUserManagementService();
        await SeedDatabase();

        await projectUserManagementService.AddUserToProject(TestProjectAcronym, TestUserId);

        await using var context = await _mockFactory.Object.CreateDbContextAsync();
        var projectUsers = await context.Project_Users
            .Include(p => p.Project)
            .ToListAsync();
        var projectId = await context.Projects
            .Where(p => p.Project_Acronym_CD == TestProjectAcronym)
            .Select(p => p.Project_ID)
            .SingleAsync();

        Assert.That(projectUsers, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(projectUsers[0].Project.Project_ID, Is.EqualTo(projectId));
            Assert.That(projectUsers[0].User_ID, Is.EqualTo(TestUserId));
        });
    }
    
    [Test]
    public async Task ShouldAddMultipleUsersToProject()
    {
        var projectUserManagementService = GetProjectUserManagementService();
        await SeedDatabase();
        await projectUserManagementService.AddUsersToProject(TestProjectAcronym, TEST_USER_IDS);

        await using var context = await _mockFactory.Object.CreateDbContextAsync();
        var projectUsers = await context.Project_Users
            .Include(p => p.Project)
            .ToListAsync();
        var projectId = await context.Projects
            .Where(p => p.Project_Acronym_CD == TestProjectAcronym)
            .Select(p => p.Project_ID)
            .SingleAsync();

       
        Assert.Multiple(() =>
        { 
            Assert.That(projectUsers, Has.Count.EqualTo(TEST_USER_IDS.Length));
            Assert.That(projectUsers[0].Project.Project_ID, Is.EqualTo(projectId));
            Assert.That(projectUsers.Select(p => p.User_ID), Is.EquivalentTo(TEST_USER_IDS));
            
        });
    }

    [Test]
    public async Task ShouldThrowException_WhenProjectNotFoundOnUserAdd()
    {
        const string nonExistentProjectAcronym = "NOTFOUND";
        var projectUserManagementService = GetProjectUserManagementService();
        await SeedDatabase(new List<string>
        {
            TestUserId
        });

        Assert.ThrowsAsync<ProjectNotFoundException>(async () =>
        {
            await projectUserManagementService.AddUserToProject(nonExistentProjectAcronym, TestUserId);
        });
    }

    [Test]
    public async Task ShouldDoNothing_WhenUserAlreadyAdded()
    {
        var projectUserManagementService = GetProjectUserManagementService();
        await SeedDatabase(new List<string>
        {
            TestUserId
        });

        await projectUserManagementService.AddUserToProject(TestProjectAcronym, TestUserId);
        await projectUserManagementService.AddUserToProject(TestProjectAcronym, TestUserId);
        await projectUserManagementService.AddUserToProject(TestProjectAcronym, TestUserId);

        await using var context = await _mockFactory.Object.CreateDbContextAsync();
        var projectUsers = await context.Project_Users.ToListAsync();
        Assert.That(projectUsers, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task ShouldRemoveUserFromProject()
    {
        var projectUserManagementService = GetProjectUserManagementService();
        await SeedDatabase(new List<string>
        {
            TestUserId
        });

        await projectUserManagementService.RemoveUserFromProject(TestProjectAcronym, TestUserId);

        await using var context = await _mockFactory.Object.CreateDbContextAsync();
        var projectUsers = await context.Project_Users.ToListAsync();
        Assert.That(projectUsers, Has.Count.EqualTo(0));
    }

    [Test]
    public async Task ShouldThrowException_WhenProjectNotFoundOnUserRemove()
    {
        const string nonExistentProjectAcronym = "NOTFOUND";
        var projectUserManagementService = GetProjectUserManagementService();
        await SeedDatabase(new List<string>
        {
            TestUserId
        });

        Assert.ThrowsAsync<ProjectNotFoundException>(async () =>
        {
            await projectUserManagementService.RemoveUserFromProject(nonExistentProjectAcronym, TestUserId);
        });
    }

    [Test]
    public async Task ShouldDoNothing_WhenUserAlreadyRemoved()
    {
        var projectUserManagementService = GetProjectUserManagementService();
        await SeedDatabase();

        await projectUserManagementService.RemoveUserFromProject(TestProjectAcronym, TestUserId);
        await projectUserManagementService.RemoveUserFromProject(TestProjectAcronym, TestUserId);
        await projectUserManagementService.RemoveUserFromProject(TestProjectAcronym, TestUserId);

        await using var context = await _mockFactory.Object.CreateDbContextAsync();
        var projectUsers = await context.Project_Users.ToListAsync();
        Assert.That(projectUsers, Has.Count.EqualTo(0));
    }
    
    [Test]
    [TestCase(ProjectMemberRole.Contributor, ProjectMemberRole.Admin)]
    [TestCase(ProjectMemberRole.Contributor, ProjectMemberRole.Publisher)]
    [TestCase(ProjectMemberRole.Admin, ProjectMemberRole.Contributor)]
    [TestCase(ProjectMemberRole.Admin, ProjectMemberRole.Publisher)]
    [TestCase(ProjectMemberRole.Publisher, ProjectMemberRole.Contributor)]
    [TestCase(ProjectMemberRole.Publisher, ProjectMemberRole.Admin)]
    public async Task ShouldUpdateUserInProject(ProjectMemberRole currentRole, ProjectMemberRole newRole)
    {
        var projectUserManagementService = GetProjectUserManagementService();
        await SeedDatabase(new List<string>
        {
            TestUserId,
        }, TestProjectAcronym, currentRole);
        var projectMember = new ProjectMember(TestUserId, newRole);
        await projectUserManagementService.UpdateUserInProject(TestProjectAcronym, projectMember);

        await using var context = await _mockFactory.Object.CreateDbContextAsync();
        var projectUser = await context.Project_Users.FirstAsync();
        Assert.Multiple(() =>
        {
            Assert.That(projectUser.IsAdmin, Is.EqualTo(newRole is ProjectMemberRole.Admin or ProjectMemberRole.Publisher));
            Assert.That(projectUser.IsDataApprover, Is.EqualTo(newRole == ProjectMemberRole.Publisher));
        });
    }

    [Test]
    public async Task ShouldThrowException_WhenProjectNotFoundOnUserUpdate()
    {
        const string nonExistentProjectAcronym = "NOTFOUND";
        var projectUserManagementService = GetProjectUserManagementService();
        await SeedDatabase(new List<string>
        {
            TestUserId
        });

        Assert.ThrowsAsync<ProjectNotFoundException>(async () =>
        {
            await projectUserManagementService.UpdateUserInProject(nonExistentProjectAcronym, 
                new ProjectMember(TestUserId, ProjectMemberRole.Contributor));
        });
    }
    
    [Test]
    public async Task ShouldThrowException_WhenUserNotFoundOnUserUpdate()
    {
        const string nonExistentUserId = "THIS_IS_NOT_AN_ID";
        var projectUserManagementService = GetProjectUserManagementService();
        await SeedDatabase(new List<string>
        {
            TestUserId,
        });
        var projectMember = new ProjectMember(nonExistentUserId, ProjectMemberRole.Admin);
        Assert.ThrowsAsync<UserNotFoundException>(async () =>
        {
            await projectUserManagementService.UpdateUserInProject(TestProjectAcronym, projectMember);
        });
    }

    [Test]
    [TestCase(ProjectMemberRole.Contributor, ProjectMemberRole.Contributor)]
    [TestCase(ProjectMemberRole.Admin, ProjectMemberRole.Admin)]
    [TestCase(ProjectMemberRole.Publisher, ProjectMemberRole.Publisher)]
    public async Task ShouldDoNothing_WhenUserRoleHasNotChanged(ProjectMemberRole currenRole, ProjectMemberRole newRole)
    {
        var projectUserManagementService = GetProjectUserManagementService();
        await SeedDatabase(new List<string>
        {
            TestUserId,
        }, TestProjectAcronym, currenRole);
        var projectMember = new ProjectMember(TestUserId, newRole);
        await using var context = await _mockFactory.Object.CreateDbContextAsync();
        var projectUser = await context.Project_Users.FirstAsync();
        Assert.Multiple(() =>
        {
            Assert.That(projectUser.IsAdmin, Is.EqualTo(currenRole is ProjectMemberRole.Admin or ProjectMemberRole.Publisher));
            Assert.That(projectUser.IsDataApprover, Is.EqualTo(currenRole is ProjectMemberRole.Publisher));
        });
        await projectUserManagementService.UpdateUserInProject(TestProjectAcronym, projectMember);
        await projectUserManagementService.UpdateUserInProject(TestProjectAcronym, projectMember);
        await projectUserManagementService.UpdateUserInProject(TestProjectAcronym, projectMember);
        projectUser = await context.Project_Users.FirstAsync();
        Assert.Multiple(() =>
        {
            Assert.That(projectUser.IsAdmin, Is.EqualTo(newRole is ProjectMemberRole.Admin or ProjectMemberRole.Publisher));
            Assert.That(projectUser.IsDataApprover, Is.EqualTo(newRole == ProjectMemberRole.Publisher));
        });
    }

    [Test]
    [TestCase(11, 0)]
    [TestCase(12, 10)]
    [TestCase(0, 11)]
    [TestCase(0, 0)]
    [TestCase(6, 6)]
    public async Task ShouldGetUsersFromProject(int userCount, int dummyCount)
    {
        var projectUserManagementService = GetProjectUserManagementService();
        await SeedDatabase(Enumerable.Range(0, userCount).Select(i => $"{TestUserId}{i}").ToList());

        var dummyProjectAcronym = $"{TestProjectAcronym}DUMMY";
        await SeedDatabase(Enumerable.Range(0, dummyCount).Select(i => $"{TestUserId}{i}").ToList(),
            dummyProjectAcronym);

        var projectUsers = await projectUserManagementService.GetUsersFromProject(TestProjectAcronym);

        var context = await _mockFactory.Object.CreateDbContextAsync();
        var totalUserCount = await context.Project_Users.CountAsync();
        Assert.Multiple(() =>
        {
            Assert.That(projectUsers.Count(), Is.EqualTo(userCount));
            Assert.That(totalUserCount, Is.EqualTo(userCount + dummyCount));
        });
    }

    [Test]
    public async Task ShouldSendTerraformVariableUpdateOnUserAdd()
    {
        var projectUserManagementService = GetProjectUserManagementService();
        
        _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
            It.IsAny<string>()), Times.Never);
        
        await SeedDatabase();
        await projectUserManagementService.AddUserToProject(TestProjectAcronym, TestUserId);

        _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
            It.Is<string>(s => s == TerraformTemplate.VariableUpdate)), Times.Once);
    }
    
    [Test]
    public async Task ShouldSendTerraformVariableUpdateOnUserRemove()
    {
        var projectUserManagementService = GetProjectUserManagementService();
        
        _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
            It.IsAny<string>()), Times.Never);
        
        await SeedDatabase(new List<string>{TestUserId});
        await projectUserManagementService.RemoveUserFromProject(TestProjectAcronym, TestUserId);

        _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
            It.Is<string>(s => s == TerraformTemplate.VariableUpdate)), Times.Once);
    }

    [Test]
    public async Task ShouldSendTerraformVariableUpdateOnUserUpdated()
    {
        
        var projectUserManagementService = GetProjectUserManagementService();
    
        _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
            It.IsAny<string>()), Times.Never);
    
        await SeedDatabase(new List<string>{TestUserId});
        var projectMember = new ProjectMember(TestUserId, ProjectMemberRole.Publisher);
        await projectUserManagementService.UpdateUserInProject(TestProjectAcronym, projectMember);

        _mockRequestManagementService.Verify(f => f.HandleTerraformRequestServiceAsync(It.IsAny<Datahub_Project>(),
            It.Is<string>(s => s == TerraformTemplate.VariableUpdate)), Times.Once);
    }

    private async Task SeedDatabase(IEnumerable<string>? userIds = null, string projectAcronym = TestProjectAcronym, ProjectMemberRole role = ProjectMemberRole.Contributor)
    {
        var project = new Datahub_Project
        {
            Project_Name = "Test Project",
            Project_Acronym_CD = projectAcronym,
            Project_Status_Desc = "Active",
            Sector_Name = "Test Sector",
        };

        var projectUsers = userIds?
            .Select(id => new Datahub_Project_User
            {
                Project = project,
                User_ID = id,
                IsAdmin = role is ProjectMemberRole.Admin or ProjectMemberRole.Publisher,
                IsDataApprover = role is ProjectMemberRole.Publisher
            })
            .ToList();

        await using var context = await _mockFactory.Object.CreateDbContextAsync();
        await context.Projects.AddAsync(project);
        await context.Project_Users.AddRangeAsync(projectUsers ?? new List<Datahub_Project_User>());
        await context.SaveChangesAsync();
    }

    private ProjectUserManagementService GetProjectUserManagementService()
    {
        var projectUserManagementService = new ProjectUserManagementService(
            Mock.Of<ILogger<ProjectUserManagementService>>(),
            _mockFactory.Object,
            _mockUserInformationService.Object,
            _mockIMSGraphService.Object,
            _mockRequestManagementService.Object);

        return projectUserManagementService;
    }
}