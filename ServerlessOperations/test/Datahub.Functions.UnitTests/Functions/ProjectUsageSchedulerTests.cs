using System.Net;
using Datahub.Application.Configuration;
using Datahub.Application.Services.Cost;
using Datahub.Application.Services.ResourceGroups;
using Datahub.Application.Services.Storage;
using Datahub.Core.Model.Context;
using Datahub.Infrastructure.Queues.Messages;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;

namespace Datahub.Functions.UnitTests
{
    [TestFixture]
    public class ProjectUsageSchedulerTests
    {
        private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
        private readonly ILogger<ProjectUsageScheduler> _logger = Substitute.For<ILogger<ProjectUsageScheduler>>();
        private IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
        private Mock<ISendEndpointProvider> _sendEndpointProviderMock;
        private Mock<IWorkspaceCostManagementService> _workspaceCostMgmtServiceMock;
        private Mock<IWorkspaceStorageManagementService> _workspaceStorageMgmtServiceMock;
        private Mock<IWorkspaceResourceGroupsManagementService> _rgMgmtServiceMock; 
        private ProjectUsageScheduler _scheduler;

        [SetUp]
        public async Task SetUp()
        {
            _loggerFactory.CreateLogger<ProjectUsageScheduler>().Returns(_logger);
            _sendEndpointProviderMock = new Mock<ISendEndpointProvider>();
            _workspaceCostMgmtServiceMock = new Mock<IWorkspaceCostManagementService>();
            _workspaceStorageMgmtServiceMock = new Mock<IWorkspaceStorageManagementService>();
            _rgMgmtServiceMock = new Mock<IWorkspaceResourceGroupsManagementService>();

            var rgNames = new List<string> { "rg1", "rg2" };
            var costs = new List<DailyServiceCost> { new DailyServiceCost() };
            var totals = new List<DailyServiceCost> { new DailyServiceCost() };

            _rgMgmtServiceMock.Setup(s => s.GetAllSubscriptionResourceGroupsAsync(It.IsAny<string>()))
                .ReturnsAsync(rgNames);

            _workspaceCostMgmtServiceMock.Setup(s => s.QuerySubscriptionCostsAsync(It.IsAny<string>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(), QueryGranularity.Daily, rgNames))
                .ReturnsAsync(costs);

            _workspaceCostMgmtServiceMock.Setup(s => s.QuerySubscriptionCostsAsync(It.IsAny<string>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(), QueryGranularity.Total, rgNames))
                .ReturnsAsync(totals);

            var datahubConfig = new DatahubPortalConfiguration();
            datahubConfig.AzureAd = new AzureAd
            {
                SubscriptionId = Guid.NewGuid().ToString(),
                TenantId = Guid.NewGuid().ToString(),
                InfraClientId = Guid.NewGuid().ToString(),
                InfraClientSecret = Guid.NewGuid().ToString()
            };

            Testing._configuration.Bind(datahubConfig);
            _dbContextFactory = TestHelper.CreateMockDbContextFactory();
            await TestHelper.SeedDatabase(_dbContextFactory);

            _scheduler = new ProjectUsageScheduler(
                _loggerFactory,
                _dbContextFactory,
                _sendEndpointProviderMock.Object,
                _workspaceCostMgmtServiceMock.Object,
                _workspaceStorageMgmtServiceMock.Object,
                _rgMgmtServiceMock.Object,
                Testing._configuration
            );
        }

        [Test]
        public async Task RunScheduler_ShouldLogInformation_WhenNoProjectsToUpdate()
        {
            // Arrange
            var loggerFactoryTest = Substitute.For<ILoggerFactory>();
            var loggerTest = new Mock<ILogger<ProjectUsageScheduler>>();
            loggerFactoryTest.CreateLogger<ProjectUsageScheduler>().Returns(loggerTest.Object);

            var mockedScheduler = new ProjectUsageScheduler(
                loggerFactoryTest,
                _dbContextFactory,
                _sendEndpointProviderMock.Object,
                _workspaceCostMgmtServiceMock.Object,
                _workspaceStorageMgmtServiceMock.Object,
                _rgMgmtServiceMock.Object,
                Testing._configuration
            );

            // Act
            var result = await mockedScheduler.RunScheduler();

            // Assert
            result.Should().Be((0, 0));
            loggerTest.Verify(l => l.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No projects to update")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Test]
        public async Task RunScheduler_ShouldReturnCorrectCounts_WhenProjectsAreUpdated()
        {
            // Arrange
            var loggerFactoryTest = Substitute.For<ILoggerFactory>();
            var loggerTest = new Mock<ILogger<ProjectUsageScheduler>>();

            loggerFactoryTest.CreateLogger<ProjectUsageScheduler>().Returns(loggerTest.Object);
            var projects = new List<string>
            {
                TestHelper.ACTIVE_WEB_APP_PROJECT_ACRONYM,
                TestHelper.TEST_PROJECT_ACRONYM
            };
            _workspaceCostMgmtServiceMock.Setup(s => s.CheckUpdateNeeded(It.IsAny<string>(), It.IsAny<DatahubProjectDBContext>())).Returns(true);
            _workspaceStorageMgmtServiceMock.Setup(s => s.CheckUpdateNeeded(It.IsAny<string>(), It.IsAny<DatahubProjectDBContext>())).Returns(true);
            _scheduler.Mock = true;
            var mockedScheduler = new Mock<ProjectUsageScheduler>(
                loggerFactoryTest,
                _dbContextFactory,
                _sendEndpointProviderMock.Object,
                _workspaceCostMgmtServiceMock.Object,
                _workspaceStorageMgmtServiceMock.Object,
                _rgMgmtServiceMock.Object,
                Testing._configuration
            )
            {
                CallBase = true
            };
            mockedScheduler.Setup(s => s.SendMessagesIfNeeded(It.IsAny<ProjectUsageUpdateMessage>(), It.IsAny<DatahubProjectDBContext>()))
                .ReturnsAsync((true, true));
            mockedScheduler.Setup(s => s.PostToBlob(It.IsAny<List<DailyServiceCost>>(), It.IsAny<List<DailyServiceCost>>()))
                .ReturnsAsync(("2", "2"));

            // Act
            var result = await mockedScheduler.Object.RunScheduler(projects);

            // Assert
            result.Should().Be((2, 2));
        }

        [Test]
        public async Task RunHttp_ShouldReturnBadRequest_WhenRequestBodyIsEmpty()
        {
            // Arrange
            var reqMock = TestHelper.CreateHttpRequestData(string.Empty);
            
            // Act
            var response = await _scheduler.RunHttp(reqMock);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task RunHttp_ShouldReturnOk_WhenRequestIsProcessed()
        {
            // Arrange
            var reqBody = "{\"manualRollover\": false, \"acronyms\": [\"TEST1\", \"TEST2\"]}";
            var reqMock = TestHelper.CreateHttpRequestData(reqBody);
             
            // Act
            var response = await _scheduler.RunHttp(reqMock);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _loggerFactory?.Dispose();
        }
    }
}
