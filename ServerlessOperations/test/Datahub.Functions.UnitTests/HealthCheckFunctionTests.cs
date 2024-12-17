using Datahub.Application.Configuration;
using Datahub.Application.Services.WebApp;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Helpers;
using Datahub.Shared;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;


namespace Datahub.Functions.UnitTests
{
    public class HealthCheckFunctionTests
    {
        private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
        private readonly IHttpClientFactory _httpClientFactory = Substitute.For<IHttpClientFactory>();

        private const string TEST_PROJECT_ACRONYM = "TEST";
        private const string ACTIVE_WEB_APP_PROJECT_ACRONYM = "WAP";
        private const string INACTIVE_WEB_APP_PROJECT_ACRONYM = "IWAP";

        private const string ACTIVE_WEB_APP_SERVICE_ID = "active-webapp";

        private CheckInfrastructureStatus _checkInfrastructureStatusFunction;

        [SetUp]
        public async Task Setup()
        {
            var datahubConfig = new DatahubPortalConfiguration();
            Testing._configuration.Bind(datahubConfig);

            var projectStorageConfigurationService = new ProjectStorageConfigurationService(datahubConfig);

            var dbContextFactory = CreateMockDbContextFactory();
            await SeedDatabase(dbContextFactory);

            var sendProvider = Substitute.For<ISendEndpointProvider>();
            var webAppService = CreateMockWebAppManagementService();

            var resourceMessagingService = new ResourceMessagingService(dbContextFactory, sendProvider);

            var healthCheckHelper = new HealthCheckHelper(dbContextFactory, projectStorageConfigurationService, webAppService,
                Testing._configuration, _httpClientFactory, _loggerFactory, sendProvider, resourceMessagingService, datahubConfig);

            _checkInfrastructureStatusFunction = new CheckInfrastructureStatus(_loggerFactory, healthCheckHelper);
        }

        private static IDbContextFactory<DatahubProjectDBContext> CreateMockDbContextFactory()
        {
            var optionsBuilder = new DbContextOptionsBuilder<SqlServerDatahubContext>().UseInMemoryDatabase(new Guid().ToString());
            // create a mock factory to return the db context when CreateDbContextAsync is called
            var context = new SqlServerDatahubContext(optionsBuilder.Options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            var mockFactory = new Mock<IDbContextFactory<DatahubProjectDBContext>>();
            mockFactory
                .Setup(f => f.CreateDbContextAsync(CancellationToken.None))
                .ReturnsAsync(() => new SqlServerDatahubContext(optionsBuilder.Options));

            return mockFactory.Object;
        }

        private static IWorkspaceWebAppManagementService CreateMockWebAppManagementService()
        {
            var mockWebAppService = new Mock<IWorkspaceWebAppManagementService>();
            mockWebAppService
                .Setup(w => w.GetState(It.IsAny<string>()))
                .ReturnsAsync((string s) => s == ACTIVE_WEB_APP_SERVICE_ID);

            return mockWebAppService.Object;
        }

        private static async Task SeedDatabase(IDbContextFactory<DatahubProjectDBContext> contextFactory)
        {
            await using var context = await contextFactory.CreateDbContextAsync();

            var projects = new List<Datahub_Project>
            {
                new()
                {
                    Project_Acronym_CD = TEST_PROJECT_ACRONYM,
                    Project_Name = "Test Workspace",
                    Project_Status_Desc = "Active",
                    Sector_Name = "Test Sector"
                },
                new()
                {
                    Project_Acronym_CD = ACTIVE_WEB_APP_PROJECT_ACRONYM,
                    Project_Name = "WebApp Test Project",
                    Project_Status_Desc = "Active",
                    Sector_Name = "Test Sector",
                    Resources =
                    [
                        new Project_Resources2
                        {
                            ResourceType = "terraform:azure-app-service",
                            JsonContent = "{\n" +
                                $"  \"app_service_id\": \"{ACTIVE_WEB_APP_SERVICE_ID}\",\n" +
                                "  \"app_service_hostname\": \"example.azurewebsites.net\",\n" +
                                "  \"app_service_rg\": \"test_rg\"\n" +
                                "}",
                            Status = TerraformStatus.Completed
                        }
                    ]
                },
                new()
                {
                    Project_Acronym_CD = INACTIVE_WEB_APP_PROJECT_ACRONYM,
                    Project_Name = "Inactive WebApp Test Project",
                    Project_Status_Desc = "Active",
                    Sector_Name = "Test Sector",
                    Resources =
                    [
                        new Project_Resources2
                        {
                            ResourceType = "terraform:azure-app-service",
                            JsonContent = "{\n" +
                                "  \"app_service_id\": \"inactive_webapp\",\n" +
                                "  \"app_service_hostname\": \"example.azurewebsites.net\",\n" +
                                "  \"app_service_rg\": \"test_rg\"\n" +
                                "}",
                            Status = TerraformStatus.Completed
                        }
                    ]
                }
            };

            await context.Projects.AddRangeAsync(projects);
            await context.SaveChangesAsync();
        }

        [Test]
        public async Task TestCoreAzureSQLDatabaseHealthCheck()
        {
            var healthCheckRequest = new InfrastructureHealthCheckMessage(Core.Model.Health.InfrastructureHealthResourceType.AzureSqlDatabase,
                InfrastructureHealthCheckConstants.CoreRequestGroup, InfrastructureHealthCheckConstants.CoreRequestGroup);
            var response = await _checkInfrastructureStatusFunction.ProcessRequest(healthCheckRequest);

            var results = GetHealthCheckResults(response);
            var firstResult = results.FirstOrDefault();
            VerifyHealthyResult(firstResult);
        }

        [Test]
        public async Task TestWorkspaceAzureSQLDatabaseHealthCheck()
        {
            var request = new InfrastructureHealthCheckMessage(Core.Model.Health.InfrastructureHealthResourceType.AzureSqlDatabase, 
                InfrastructureHealthCheckConstants.WorkspacesRequestGroup, TEST_PROJECT_ACRONYM);
            var response = await _checkInfrastructureStatusFunction.ProcessRequest(request);

            var results = GetHealthCheckResults(response);
            var firstResult = results.FirstOrDefault();
            VerifyHealthyResult(firstResult);
        }

        [Test]
        public async Task TestInvalidWorkspaceSQLDatabaseHealthCheck()
        {
            var request = new InfrastructureHealthCheckMessage(Core.Model.Health.InfrastructureHealthResourceType.AzureSqlDatabase, 
                InfrastructureHealthCheckConstants.WorkspacesRequestGroup, "NOPE");
            var response = await _checkInfrastructureStatusFunction.ProcessRequest(request);

            var results = GetHealthCheckResults(response);
            var firstResult = results.FirstOrDefault();
            firstResult.Should().NotBeNull();
            firstResult.Check.Should().NotBeNull();
            firstResult.Check.Status.Should().Be(Core.Model.Health.InfrastructureHealthStatus.Degraded);
            firstResult.Errors.Should().HaveCount(1);
            firstResult.Errors[0].Should().Contain("Cannot retrieve project");
        }

        [Test]
        public async Task TestUndefinedWebAppHealthCheck()
        {
            var request = new InfrastructureHealthCheckMessage(Core.Model.Health.InfrastructureHealthResourceType.AzureWebApp,
                InfrastructureHealthCheckConstants.WorkspacesRequestGroup, TEST_PROJECT_ACRONYM);
            var response = await _checkInfrastructureStatusFunction.ProcessRequest(request);

            var results = GetHealthCheckResults(response);
            var firstResult = results.FirstOrDefault();
            firstResult.Should().NotBeNull();
            firstResult.Check.Should().NotBeNull();
            firstResult.Check.Status.Should().Be(Core.Model.Health.InfrastructureHealthStatus.Undefined);
        }

        [Test]
        public async Task TestRunningWebAppHealthCheck()
        {
            var request = new InfrastructureHealthCheckMessage(Core.Model.Health.InfrastructureHealthResourceType.AzureWebApp, 
                InfrastructureHealthCheckConstants.WorkspacesRequestGroup, ACTIVE_WEB_APP_PROJECT_ACRONYM);
            var response = await _checkInfrastructureStatusFunction.ProcessRequest(request);

            var results = GetHealthCheckResults(response);
            var firstResult = results.FirstOrDefault();
            VerifyHealthyResult(firstResult);
        }

        [Test]
        public async Task TestNotRunningWebAppHealthCheck()
        {
            var request = new InfrastructureHealthCheckMessage(Core.Model.Health.InfrastructureHealthResourceType.AzureWebApp,
                InfrastructureHealthCheckConstants.WorkspacesRequestGroup, INACTIVE_WEB_APP_PROJECT_ACRONYM);
            var response = await _checkInfrastructureStatusFunction.ProcessRequest(request);

            var results = GetHealthCheckResults(response);
            var firstResult = results.FirstOrDefault();
            firstResult.Should().NotBeNull();
            firstResult.Check.Should().NotBeNull();
            firstResult.Check.Status.Should().Be(Core.Model.Health.InfrastructureHealthStatus.Degraded);
            firstResult.Errors.Should().HaveCount(1);
            firstResult.Errors[0].Should().Contain("not running");
        }

        private static IEnumerable<InfrastructureHealthCheckResponse> GetHealthCheckResults(IActionResult? response)
        {
            response.Should().BeAssignableTo<OkObjectResult>("The health check should return an OK response, whether the resources are healthy or not.");
            var okObjectResult = response as OkObjectResult;
            okObjectResult.Should().NotBeNull();
            var objectValue = okObjectResult.Value;

            objectValue.Should().BeAssignableTo<IEnumerable<InfrastructureHealthCheckResponse>>("The result should be one or more InfrastructureHealthCheckResponses");
            var results = objectValue as IEnumerable<InfrastructureHealthCheckResponse>;
            results.Should().NotBeNull();
            return results;
        }

        private static void VerifyHealthyResult(InfrastructureHealthCheckResponse? result)
        {
            result.Should().NotBeNull();
            result.Check.Should().NotBeNull();
            result.Check.Status.Should().Be(Core.Model.Health.InfrastructureHealthStatus.Healthy);
            result.Errors.Should().BeEmpty();
        }

        [OneTimeTearDown]
        public void TearDown() 
        { 
            _loggerFactory?.Dispose();
        }
    }
}
