using Azure.Messaging.ServiceBus;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.Notification;
using Datahub.Core.Model.Context;
using Datahub.Functions.Services;
using Datahub.Functions.Validators;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services.Notification;
using Datahub.Shared.Entities;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Datahub.Functions.UnitTests
{
    [TestFixture]
    public class ProjectUsageNotifierTests
    {
        private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
        private readonly ILogger<ProjectUsageNotifier> _logger = Substitute.For<ILogger<ProjectUsageNotifier>>();

        private ProjectUsageNotifier _notifier;
        private IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
        private EmailValidator _emailValidatorMock;
        private ISendEndpointProvider _sendEndpointProviderMock;
        private IGCNotifyService _notifyService;
        private IResourceMessagingService _resourceMessagingServiceMock;
        private IConfiguration _config = Substitute.For<IConfiguration>();
        private AzureConfig _azureConfig;

        private IQueuePongService _pongService = null!;

        [SetUp]
        public async Task Setup()
        {
            _loggerFactory.CreateLogger<ProjectUsageNotifier>().Returns(_logger);
            _pongService = Substitute.For<IQueuePongService>();

            _emailValidatorMock = Substitute.For<EmailValidator>();
            _sendEndpointProviderMock = Substitute.For<ISendEndpointProvider>();
            _notifyService = Substitute.For<IGCNotifyService>();
            _resourceMessagingServiceMock = Substitute.For<IResourceMessagingService>();

            var sendEndpointMock = Substitute.For<ISendEndpoint>();
            sendEndpointMock.Send(Arg.Any<object>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

            _sendEndpointProviderMock.GetSendEndpoint(Arg.Any<Uri>()).Returns(Task.FromResult(sendEndpointMock));

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

            _azureConfig = new AzureConfig(_config);

            _notifier = new ProjectUsageNotifier(
                _loggerFactory,
                _azureConfig,
                _dbContextFactory,
                _pongService,
                _emailValidatorMock,
                _sendEndpointProviderMock,
                _notifyService,
                _resourceMessagingServiceMock
            );
        }

        [Test]
        public async Task Run_ShouldSucceed()
        {
            // Arrange
            var projectNotificationMessage = new ProjectUsageNotificationMessage("TEST");
            var messageEnvelope = new
            {
                message = projectNotificationMessage
            };
            var messageBody = System.Text.Json.JsonSerializer.Serialize(messageEnvelope);
            var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
                body: new BinaryData(messageBody));

            // Act
            Func<Task> act = async () => await _notifier.Run(serviceBusReceivedMessage, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Test]
        public async Task Run_ShouldThrowError_WhenMessageIsInvalid()
        {
            // Arrange
            var projectNotificationMessage = new ProjectUsageNotificationMessage("");
            var messageEnvelope = new { message = projectNotificationMessage };
            var messageBody = System.Text.Json.JsonSerializer.Serialize(messageEnvelope);
            var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
                body: new BinaryData(messageBody));

            // Act
            Func<Task> act = async () => await _notifier.Run(serviceBusReceivedMessage, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Invalid queue message:\n*");
        }

        [Test]
        public async Task VerifyOverBudgetIsDeleted_ShouldDeleteResources_WhenOverBudget()
        {
            // Arrange
            var projectAcronym = TestHelper.OVERBUDGET_WEB_APP_PROJECT_ACRONYM;
            var cancellationToken = CancellationToken.None;

            _resourceMessagingServiceMock.GetWorkspaceDefinition(Arg.Any<string>(), Arg.Any<string>())
                .Returns(new WorkspaceDefinition());

            // Act
            Func<Task> act = async () => await _notifier.VerifyOverBudgetIsDeleted(projectAcronym, cancellationToken);

            // Assert
            await act.Should().NotThrowAsync();
            _resourceMessagingServiceMock.Received(1).SendToTerraformQueue(Arg.Any<WorkspaceDefinition>());
        }

        [Test]
        public async Task VerifyOverBudgetIsDeleted_ShouldNotDeleteResources_WhenNotOverBudget()
        {
            // Arrange
            var projectAcronym = "TEST";
            var cancellationToken = CancellationToken.None;

            // Act
            Func<Task> act = async () => await _notifier.VerifyOverBudgetIsDeleted(projectAcronym, cancellationToken);

            // Assert
            await act.Should().NotThrowAsync();
            _resourceMessagingServiceMock.DidNotReceive().SendToTerraformQueue(Arg.Any<WorkspaceDefinition>());
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _loggerFactory?.Dispose();
        }
    }
}
