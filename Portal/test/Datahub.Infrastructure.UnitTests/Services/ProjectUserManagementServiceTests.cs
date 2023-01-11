using System.Net.Mail;
using Datahub.Core.Data;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services;
using Datahub.Core.Services.UserManagement;
using Datahub.Infrastructure.Services;
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
    public async Task ShouldAddUserToProjectTest()
    {
        var project = new Datahub_Project
        {
            Project_Name = "Test Project",
            Project_Acronym_CD = TestProjectAcronym,
            Project_Status_Desc = "Active",
            Sector_Name = "Test Sector",
        };

        await using (var context = await _mockFactory.Object.CreateDbContextAsync())
        {
            context.Add(project);
            await context.SaveChangesAsync();
        }
        
        var projectUserManagementService = new ProjectUserManagementService(
            Mock.Of<ILogger<ProjectUserManagementService>>(), 
            _mockFactory.Object, 
            _mockUserInformationService.Object,
            _mockIMSGraphService.Object);
        
        await projectUserManagementService.AddUserToProject(TestProjectAcronym, TestUserId);
        
        await using (var context = await _mockFactory.Object.CreateDbContextAsync())
        {
            var projectUsers = await context.Project_Users.ToListAsync();
            var projectId = await context.Projects
                .Where(p => p.Project_Acronym_CD == TestProjectAcronym)
                .Select(p => p.Project_ID)
                .SingleAsync(); 
            
            Assert.That(projectUsers, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(projectUsers[0].Project_ID, Is.EqualTo(projectId));
                Assert.That(projectUsers[0].User_ID, Is.EqualTo(TestUserId));
            });
        }
    }
}