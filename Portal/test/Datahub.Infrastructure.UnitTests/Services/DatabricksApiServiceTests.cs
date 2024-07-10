using Azure.Core;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models;
using Moq;
using Moq.Protected;

namespace Datahub.Infrastructure.UnitTests.Services;
using static Testing;
public class DatabricksApiServiceTests
{
    private Mock<IDbContextFactory<DatahubProjectDBContext>> _mockFactory = null!;

    private Mock<IUserInformationService> _mockUserInformationService = null!;

    [SetUp]
    public void Setup()
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<SqlServerDatahubContext>()
                .UseInMemoryDatabase(new Guid().ToString());
        // create a mock factory to return the db context when CreateDbContextAsync is called
        var context = new SqlServerDatahubContext(optionsBuilder.Options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        _mockFactory = new Mock<IDbContextFactory<DatahubProjectDBContext>>();
        _mockFactory
            .Setup(f => f.CreateDbContextAsync(CancellationToken.None))
            .ReturnsAsync(() => new SqlServerDatahubContext(optionsBuilder.Options));

        // create a mock user information service to return the current (admin) user when GetCurrentGraphUserAsync is called
        _mockUserInformationService = new Mock<IUserInformationService>();
        _mockUserInformationService
            .Setup(f => f.GetCurrentGraphUserAsync())
            .ReturnsAsync(new User(){Id = TestUserGraphGuid, Mail = TestUserEmail});

        SetDatabricksApiService(_mockFactory.Object); 
    }

    [Test]
    [TestCase("TEST1", "https://test.azuredatabricks.net")]
    [TestCase("UNKNOWN", "")]
    public async Task ShouldReturnDatabricsWorkspaceUrl(string projectAcronym, string expectedResult)
    {
        await SeedDatabase();
        var dataBricksUrl = await _databricksApiService.GetDatabricsWorkspaceUrlAsync(projectAcronym);
        Assert.That(dataBricksUrl, Is.EqualTo(expectedResult));
    }

    [Test]
    [TestCase("TEST1", "https://test.azuredatabricks.net")] 
    public async Task ShouldUseDatabricksApi(string projectAcronym, string expectedUri)
    {
        await SeedDatabase();
        var accessToken = new AccessToken("", DateTime.Now.AddDays(1));
        var portalUser = new PortalUser { GraphGuid = TestUserGraphGuid };
        await _databricksApiService.AddAdminToDatabricsWorkspaceAsync(accessToken, projectAcronym, portalUser);

        _mockHandler.Protected().Verify(
           "SendAsync",
           Times.Exactly(1),  
           ItExpr.Is<HttpRequestMessage>(req =>
              req.Method == HttpMethod.Get 
           ),
           ItExpr.IsAny<CancellationToken>()
        );

        _mockHandler.Protected().Verify(
           "SendAsync",
           Times.Exactly(1),
           ItExpr.Is<HttpRequestMessage>(req =>
              req.Method == HttpMethod.Post
           ),
           ItExpr.IsAny<CancellationToken>()
        );
    }

    private async Task SeedDatabase()
    {
        await using var context = await _mockFactory.Object.CreateDbContextAsync();
        var projects = PROJECT_ACRONYMS.Select((acronym, index) => new Datahub_Project()
        {
            Project_Acronym_CD = acronym,
            Project_Name = PROJECT_NAMES[index],
            Project_Status_Desc = "Active",
            Sector_Name = "Test Sector",
            Resources = [
               new Project_Resources2 
               {
                   ResourceType = "terraform:azure-databricks", 
                   JsonContent="{\"workspace_url\":\"test.azuredatabricks.net\"}" 
               }
            ]
        }).ToArray();
        await context.Projects.AddRangeAsync(projects);
        await context.SaveChangesAsync();
    }
}