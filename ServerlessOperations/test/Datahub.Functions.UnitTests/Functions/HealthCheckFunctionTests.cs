using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.WebApp;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Helpers;
using Datahub.Shared;
using Datahub.Shared.Clients;
using Datahub.Shared.Entities;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;


namespace Datahub.Functions.UnitTests.Functions
{
    public class HealthCheckFunctionTests
    {
        private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
        private readonly IHttpClientFactory _httpClientFactory = Substitute.For<IHttpClientFactory>();

        private CheckInfrastructureStatus _checkInfrastructureStatusFunction;

        [SetUp]
        public async Task Setup()
        {
            var datahubConfig = new DatahubPortalConfiguration();
            datahubConfig.AzureAd = new AzureAd
            {
                SubscriptionId = Guid.NewGuid().ToString(),
                TenantId = Guid.NewGuid().ToString(),
                InfraClientId = Guid.NewGuid().ToString(),
                InfraClientSecret = Guid.NewGuid().ToString()
            };

            Testing._configuration.Bind(datahubConfig);
            var keyVaultUserService = Substitute.For<IKeyVaultUserService>();

            var projectStorageConfigurationService = new ProjectStorageConfigurationService(datahubConfig, keyVaultUserService);

            var dbContextFactory = TestHelper.CreateMockDbContextFactory();
            await TestHelper.SeedDatabase(dbContextFactory);

            var sendProvider = Substitute.For<ISendEndpointProvider>();
            var webAppService = TestHelper.CreateMockWebAppManagementService();
            var workspaceVersionService = Substitute.For<IWorkspaceVersionService>();
            var mockSubnetPoolService = Substitute.For<ISubnetPoolService>();
            var resourceMessagingService = new ResourceMessagingService(dbContextFactory, sendProvider, workspaceVersionService, mockSubnetPoolService);
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            var tokenManager = Substitute.For<AzAccessTokenManager>();
            var healthCheckHelper = new HealthCheckHelper(dbContextFactory, projectStorageConfigurationService, webAppService,
                Testing._configuration, _httpClientFactory, _loggerFactory, tokenManager, sendProvider, resourceMessagingService, datahubConfig, httpContextAccessor, null);

            _checkInfrastructureStatusFunction = new CheckInfrastructureStatus(_loggerFactory, healthCheckHelper);
        }

        [Test]
        public async Task TestCoreAzureSQLDatabaseHealthCheck()
        {
            var healthCheckRequest = new InfrastructureHealthCheckMessage(InfrastructureHealthResourceType.AzureSqlDatabase,
                InfrastructureHealthCheckConstants.CoreRequestGroup, InfrastructureHealthCheckConstants.CoreRequestGroup);
            var response = await _checkInfrastructureStatusFunction.ProcessRequest(healthCheckRequest);

            var results = GetHealthCheckResults(response);
            var firstResult = results.FirstOrDefault();
            VerifyHealthyResult(firstResult);
        }

        [Test]
        public async Task TestWorkspaceAzureSQLDatabaseHealthCheck()
        {
            var request = new InfrastructureHealthCheckMessage(InfrastructureHealthResourceType.AzureSqlDatabase,
                InfrastructureHealthCheckConstants.WorkspacesRequestGroup, TestHelper.TEST_PROJECT_ACRONYM);
            var response = await _checkInfrastructureStatusFunction.ProcessRequest(request);

            var results = GetHealthCheckResults(response);
            var firstResult = results.FirstOrDefault();
            VerifyHealthyResult(firstResult);
        }

        [Test]
        public async Task TestWorkspaceAzureFunctionHealthCheck()
        {
            var request = new InfrastructureHealthCheckMessage(InfrastructureHealthResourceType.AzureFunction,
                InfrastructureHealthCheckConstants.WorkspacesRequestGroup, TestHelper.TEST_PROJECT_ACRONYM);
            var response = await _checkInfrastructureStatusFunction.ProcessRequest(request);

            var results = GetHealthCheckResults(response);
            var firstResult = results.FirstOrDefault();
            var expectedError = "Error while checking Azure Function health: ClientSecretCredential authentication failed";
            VerifyUnhealthyResult(firstResult, expectedError);
        }

        [Test]
        public async Task TestInvalidWorkspaceSQLDatabaseHealthCheck()
        {
            var request = new InfrastructureHealthCheckMessage(InfrastructureHealthResourceType.AzureSqlDatabase,
                InfrastructureHealthCheckConstants.WorkspacesRequestGroup, "NOPE");
            var response = await _checkInfrastructureStatusFunction.ProcessRequest(request);

            var results = GetHealthCheckResults(response);
            var firstResult = results.FirstOrDefault();
            firstResult.Should().NotBeNull();
            firstResult.Check.Should().NotBeNull();
            firstResult.Check.Status.Should().Be(InfrastructureHealthStatus.Degraded);
            firstResult.Errors.Should().HaveCount(1);
            firstResult.Errors[0].Should().Contain("Cannot retrieve project");
        }

        [Test]
        public async Task TestUndefinedWebAppHealthCheck()
        {
            var request = new InfrastructureHealthCheckMessage(InfrastructureHealthResourceType.AzureWebApp,
                InfrastructureHealthCheckConstants.WorkspacesRequestGroup, TestHelper.TEST_PROJECT_ACRONYM);
            var response = await _checkInfrastructureStatusFunction.ProcessRequest(request);

            var results = GetHealthCheckResults(response);
            var firstResult = results.FirstOrDefault();
            firstResult.Should().NotBeNull();
            firstResult.Check.Should().NotBeNull();
            firstResult.Check.Status.Should().Be(InfrastructureHealthStatus.Undefined);
        }

        [Test]
        public async Task TestRunningWebAppHealthCheck()
        {
            var request = new InfrastructureHealthCheckMessage(InfrastructureHealthResourceType.AzureWebApp,
                InfrastructureHealthCheckConstants.WorkspacesRequestGroup, TestHelper.ACTIVE_WEB_APP_PROJECT_ACRONYM);
            var response = await _checkInfrastructureStatusFunction.ProcessRequest(request);

            var results = GetHealthCheckResults(response);
            var firstResult = results.FirstOrDefault();
            VerifyHealthyResult(firstResult);
        }

        [Test]
        public async Task TestNotRunningWebAppHealthCheck()
        {
            var request = new InfrastructureHealthCheckMessage(InfrastructureHealthResourceType.AzureWebApp,
                InfrastructureHealthCheckConstants.WorkspacesRequestGroup, TestHelper.INACTIVE_WEB_APP_PROJECT_ACRONYM);
            var response = await _checkInfrastructureStatusFunction.ProcessRequest(request);

            var results = GetHealthCheckResults(response);
            var firstResult = results.FirstOrDefault();
            firstResult.Should().NotBeNull();
            firstResult.Check.Should().NotBeNull();
            firstResult.Check.Status.Should().Be(InfrastructureHealthStatus.Degraded);
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
            result.Check.Status.Should().Be(InfrastructureHealthStatus.Healthy);
            result.Errors.Should().BeEmpty();
        }

        private static void VerifyUnhealthyResult(InfrastructureHealthCheckResponse? result, string expectedError)
        {
            result.Should().NotBeNull();
            result.Check.Should().NotBeNull();
            result.Check.Status.Should().Be(InfrastructureHealthStatus.Unhealthy);
            result.Errors?.Count().Should().BeGreaterThan(0);
            result.Errors?[0].Should().Contain(expectedError);
        }
        [OneTimeTearDown]
        public void TearDown()
        {
            _loggerFactory?.Dispose();
        }
    }
}
