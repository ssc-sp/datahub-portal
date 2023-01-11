using System.Net.Mail;
using Datahub.Core.Data;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services;
using Datahub.Core.Services.UserManagement;
using Datahub.Infrastructure.Services;
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
    private Mock<IMSGraphService> _mockIMSGraphService = null!;


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
            .Setup(f => f.GetUserAsync(It.Is<string>(s => s == TestUserId), CancellationToken.None))
            .ReturnsAsync(new GraphUser
            {
                mailAddress = new MailAddress(TestUserEmail),
                Id = TestUserId,
            });
    }

    [Test]
    public async Task ShouldAddUserToProject()
    {
        var projectUserManagementService = GetProjectUserManagementService();
        await SeedDatabase();
        
        await projectUserManagementService.AddUserToProject(TestProjectAcronym, TestUserId);

        await using var context = await _mockFactory.Object.CreateDbContextAsync();
        var projectUsers = await context.Project_Users.ToListAsync();
        var projectId = await context.Projects
            .Where(p => p.Project_Acronym_CD == TestProjectAcronym)
            .Select(p => p.Project_ID)
            .SingleAsync(); 
            
        Assert.That(projectUsers, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(projectUsers[0].ProjectId, Is.EqualTo(projectId));
            Assert.That(projectUsers[0].User_ID, Is.EqualTo(TestUserId));
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

        Assert.ThrowsAsync<ProjectNoFoundException>(async () =>
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

        Assert.ThrowsAsync<ProjectNoFoundException>(async () =>
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
        await SeedDatabase(Enumerable.Range(0, dummyCount).Select(i => $"{TestUserId}{i}").ToList(), dummyProjectAcronym);
        
        var projectUsers = await projectUserManagementService.GetUsersFromProject(TestProjectAcronym);
        
        var context = await _mockFactory.Object.CreateDbContextAsync();
        var totalUserCount = await context.Project_Users.CountAsync();
        Assert.Multiple(() =>
        {
            Assert.That(projectUsers.Count(), Is.EqualTo(userCount));
            Assert.That(totalUserCount, Is.EqualTo(userCount + dummyCount));
        });
    }

    private async Task SeedDatabase(IEnumerable<string>? userIds = null, string projectAcronym = TestProjectAcronym)
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
                User_ID = id
            })
            .ToList();

        await using var context = await _mockFactory.Object.CreateDbContextAsync();
        await context.Projects.AddAsync(project);
        await context.Project_Users.AddRangeAsync(projectUsers?? new List<Datahub_Project_User>());
        await context.SaveChangesAsync();
    }

    private ProjectUserManagementService GetProjectUserManagementService()
    {
        var projectUserManagementService = new ProjectUserManagementService(
            Mock.Of<ILogger<ProjectUserManagementService>>(),
            _mockFactory.Object,
            _mockUserInformationService.Object,
            _mockIMSGraphService.Object);

        return projectUserManagementService;
    }
}