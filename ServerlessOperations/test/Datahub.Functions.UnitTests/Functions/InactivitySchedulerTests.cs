using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Infrastructure.Extensions; // Add this line
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Configuration;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Datahub.Functions.UnitTests
{
    public class InactivitySchedulerTests
    {
        private InactivityScheduler _sut = null!;
        private IDbContextFactory<DatahubProjectDBContext> _dbContextFactory = Substitute.For<IDbContextFactory<DatahubProjectDBContext>>();
        private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
        private readonly ILogger<InactivityScheduler> _logger = Substitute.For<ILogger<InactivityScheduler>>();
        private readonly ISendEndpointProvider _sendEndpointProvider = Substitute.For<ISendEndpointProvider>();

        [SetUp]
        public async Task SetupAsync()
        {
            _loggerFactory.CreateLogger<InactivityScheduler>().Returns(_logger);
            _dbContextFactory = TestHelper.CreateMockDbContextFactory();
            await TestHelper.SeedDatabase(_dbContextFactory);
            _sut = new InactivityScheduler(_loggerFactory, _dbContextFactory, _sendEndpointProvider);
        }

        [Test]
        public async Task Run_ShouldScheduleProjectsAndUsers()
        {
            // Arrange
            var timerInfo = new TimerInfo();

            // Act
            await _sut.Run(timerInfo);

            // Assert
            _sendEndpointProvider.Received(3).SendDatahubServiceBusMessage(QueueConstants.ProjectInactivityNotificationQueueName, Arg.Any<ProjectInactivityNotificationMessage>());
        }

#if DEBUG
        [Test]
        public async Task RunHttp_ShouldScheduleProjectsAndUsers()
        {
            // Arrange
            var requestBody = JsonSerializer.Serialize(string.Empty);
            var httpRequestData = TestHelper.CreateHttpRequestData(requestBody);

            // Act
            await _sut.RunHttp(httpRequestData);

            // Assert
            _sendEndpointProvider.Received(3).SendDatahubServiceBusMessage(QueueConstants.ProjectInactivityNotificationQueueName, Arg.Any<ProjectInactivityNotificationMessage>());
        }
#endif

        [Test]
        public async Task ScheduleProjects_ShouldSendMessagesForEachProject()
        {
            // Act
            await _sut.ScheduleProjects();

            // Assert
            _sendEndpointProvider.Received(3).SendDatahubServiceBusMessage(QueueConstants.ProjectInactivityNotificationQueueName, Arg.Any<ProjectInactivityNotificationMessage>());
        }

        [Test]
        public async Task ScheduleUsers_ShouldSendMessagesForEachUser()
        {
            // Act
            await _sut.ScheduleUsers();

            // Assert
            _sendEndpointProvider.Received(1).SendDatahubServiceBusMessage(QueueConstants.UserInactivityNotification, Arg.Any<UserInactivityNotificationMessage>());
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _loggerFactory?.Dispose();
        }
    }
}
