using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Core.Services;
using Datahub.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models;
using Moq;
namespace Datahub.Infrastructure.UnitTests.Services;
using static Testing;
public class ProjectResourcingWhitelistServiceTests
{
    private Mock<IDbContextFactory<DatahubProjectDBContext>> _mockFactory = null!;

    private Mock<IUserInformationService> _mockUserInformationService = null!;

    [SetUp]
    public void Setup()
    {

        var optionsBuilder =
            new DbContextOptionsBuilder<DatahubProjectDBContext>()
                .UseInMemoryDatabase((new Guid()).ToString());
        // create a mock factory to return the db context when CreateDbContextAsync is called
        var context = new DatahubProjectDBContext(optionsBuilder.Options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        _mockFactory = new Mock<IDbContextFactory<DatahubProjectDBContext>>();
        _mockFactory
            .Setup(f => f.CreateDbContextAsync(CancellationToken.None))
            .ReturnsAsync(() => new DatahubProjectDBContext(optionsBuilder.Options));
        // create a mock user information service to return the current (admin) user when GetCurrentGraphUserAsync is called
        _mockUserInformationService = new Mock<IUserInformationService>();
        _mockUserInformationService
            .Setup(f => f.GetCurrentGraphUserAsync())
            .ReturnsAsync(new User() { Id = TestUserGraphGuid, Mail = TestUserEmail });
    }
    /*
     * Test Cases:
     * - Get all whitelists, ensure there are only two
     * - Get whitelist for project 1 and project 2, ensure settings are set 
     */
    [Test]
    [Ignore("Needs to be validated")]
    public async Task ShouldBeTwoWhitelists()
    {
        var whitelistService = GetProjectResourcingWhitelistService();
        await SeedDatabase();
        var projectWhitelists = await whitelistService.GetAllProjectResourceWhitelistAsync();
        var datahubProjectResourcesWhitelists = projectWhitelists.ToList();
        Assert.That(datahubProjectResourcesWhitelists.Where(wl => wl.Id > 0).ToList(), Has.Count.EqualTo(2));
    }

    [Test]
    [TestCase("TEST1", true, false, false, Ignore = "Needs to be validated")]
    [TestCase("TEST2", true, true, true, Ignore = "Needs to be validated")]
    public async Task ShouldReturnProperWhitelist(string projectAcronym, bool allowStorage, bool allowVMs, bool allowDatabricks)
    {
        var whitelistService = GetProjectResourcingWhitelistService();
        await SeedDatabase();
        await using var context = await _mockFactory.Object.CreateDbContextAsync();
        var project = await context.Projects.FirstOrDefaultAsync(p => p.ProjectAcronymCD == projectAcronym);
        Assert.That(project, Is.Not.Null);
        var whitelist = await whitelistService.GetProjectResourceWhitelistByProjectAsync(project!.ProjectID);
        Assert.Multiple(() =>
        {
            Assert.That(project.ProjectAcronymCD, Is.EqualTo(whitelist.Project.ProjectAcronymCD));
            Assert.That(allowStorage, Is.EqualTo(whitelist.AllowStorage));
            Assert.That(allowVMs, Is.EqualTo(whitelist.AllowVMs));
            Assert.That(allowDatabricks, Is.EqualTo(whitelist.AllowDatabricks));
        });

    }

    [TestCase("TEST1", Ignore = "Needs to be validated")]
    [TestCase("TEST2", Ignore = "Needs to be validated")]
    [TestCase("TEST3", Ignore = "Needs to be validated")]
    public async Task ShouldUpdateWhitelist(string projectAcronym)
    {
        var whitelistService = GetProjectResourcingWhitelistService();
        await SeedDatabase();
        await using var context = await _mockFactory.Object.CreateDbContextAsync();
        var project = await context.Projects.FirstOrDefaultAsync(p => p.ProjectAcronymCD == projectAcronym);
        Assert.That(project, Is.Not.Null);
        var whitelist = await whitelistService.GetProjectResourceWhitelistByProjectAsync(project!.ProjectID);
        // if whitelist id is greater than 0, assert that admin user is old user
        if (whitelist.Id > 0)
        {
            Assert.Multiple(() =>
            {
                Assert.That(whitelist.AdminLastUpdatedID, Is.EqualTo(OldUserId));
                Assert.That(whitelist.AdminLastUpdatedUserName, Is.EqualTo(OldUserEmail));
            });
        }
        // allow all resources on whitelist
        whitelist.AllowStorage = true;
        whitelist.AllowDatabricks = true;
        whitelist.AllowVMs = true;
        await whitelistService.UpdateProjectResourceWhitelistAsync(whitelist);
        //var whitelist = await whitelistService.GetProjectResourceWhitelistByProjectAsync(project!.Project_ID);
        Assert.Multiple(() =>
        {
            Assert.That(whitelist.AdminLastUpdatedID, Is.EqualTo(TestUserGraphGuid));
            Assert.That(whitelist.AdminLastUpdatedUserName, Is.EqualTo(TestUserEmail));
        });

    }

    private async Task SeedDatabase()
    {
        await using var context = await _mockFactory.Object.CreateDbContextAsync();
        var projects = PROJECT_ACRONYMS.Select((acronym, index) => new DatahubProject()
        {
            ProjectAcronymCD = acronym,
            Project_Name = PROJECT_NAMES[index],
            ProjectStatusDesc = "Active",
            SectorName = "Test Sector",
        }).ToArray();
        await context.Projects.AddRangeAsync(projects);
        await context.SaveChangesAsync();
        var whitelists = new List<ProjectWhitelist>
        {
            new()
            {
                ProjectId = projects[0].ProjectID,
                AllowStorage = true,
                AdminLastUpdatedID = OldUserId,
                AdminLastUpdatedUserName = OldUserEmail,
                LastUpdated = DateTime.Now.Subtract(TimeSpan.FromDays(3)),
            },
            new()
            {
                ProjectId = projects[1].ProjectID,
                AllowStorage = true,
                AllowVMs = true,
                AllowDatabricks = true,
                AdminLastUpdatedID = OldUserId,
                AdminLastUpdatedUserName = OldUserEmail,
                // set last updated to a few days before no
                LastUpdated = DateTime.Now.Subtract(TimeSpan.FromDays(3)),
            }
        };
        // purposely not creating a whitelist for the third project

        await context.ProjectWhitelists.AddRangeAsync(whitelists);
        await context.SaveChangesAsync();
    }

    private ProjectResourcingWhitelistService GetProjectResourcingWhitelistService()
    {

        var projectUserManagementService = new ProjectResourcingWhitelistService(
            _mockFactory.Object,
            _mockUserInformationService.Object,
            null!);
        return projectUserManagementService;
    }
}