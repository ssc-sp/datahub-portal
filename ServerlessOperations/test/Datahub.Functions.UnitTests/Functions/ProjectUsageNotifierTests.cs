using Azure.Messaging.ServiceBus;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Datahub.Functions.Services;
using Datahub.Functions.Validators;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Entities;
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
    public class ProjectUsageNotifierTests
    {
        private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
        private readonly ILogger<ProjectUsageNotifier> _logger = Substitute.For<ILogger<ProjectUsageNotifier>>();

        private ProjectUsageNotifier _notifier;
        private IDbContextFactory<DatahubProjectDBContext> _dbContextFactory; 
        private Mock<EmailValidator> _emailValidatorMock;
        private Mock<ISendEndpointProvider> _sendEndpointProviderMock;
        private Mock<IEmailService> _emailServiceMock;
        private Mock<IResourceMessagingService> _resourceMessagingServiceMock; 
        private IConfiguration _config = Substitute.For<IConfiguration>();
        private AzureConfig _azureConfig;

        private Mock<IQueuePongService> _pongService = null!;

        [SetUp]
        public async Task Setup()
        {
            _loggerFactory.CreateLogger<ProjectUsageNotifier>().Returns(_logger);
            _pongService = new Mock<IQueuePongService>();
             
            _emailValidatorMock = new Mock<EmailValidator>();
            _sendEndpointProviderMock = new Mock<ISendEndpointProvider>();
            _emailServiceMock = new Mock<IEmailService>();
            _resourceMessagingServiceMock = new Mock<IResourceMessagingService>();

            var sendEndpointMock = new Mock<ISendEndpoint>();
            sendEndpointMock.Setup(endpoint => endpoint.Send(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _sendEndpointProviderMock.Setup(provider => provider.GetSendEndpoint(It.IsAny<Uri>()))
                .ReturnsAsync(sendEndpointMock.Object);

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
                _pongService.Object,
                _emailValidatorMock.Object,
                _sendEndpointProviderMock.Object,
                _emailServiceMock.Object,
                _resourceMessagingServiceMock.Object
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

            _resourceMessagingServiceMock.Setup(service => service.GetWorkspaceDefinition(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new WorkspaceDefinition());           

            // Act
            Func<Task> act = async () => await _notifier.VerifyOverBudgetIsDeleted(projectAcronym, cancellationToken);

            // Assert
            await act.Should().NotThrowAsync();
            _resourceMessagingServiceMock.Verify(service => service.SendToTerraformQueue(It.IsAny<WorkspaceDefinition>()), Times.Once);
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
            _resourceMessagingServiceMock.Verify(service => service.SendToTerraformQueue(It.IsAny<WorkspaceDefinition>()), Times.Never);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _loggerFactory?.Dispose();
        }
    }
}
