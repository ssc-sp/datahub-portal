using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Datahub.Application.Configuration;
using Datahub.Application.Services.Cost;
using Datahub.Application.Services.Storage;
using Datahub.Functions;
using Datahub.Functions.Extensions;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Configuration;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;

namespace Datahub.Functions.UnitTests
{
    [TestFixture]
    public class ProjectUsageUpdaterTests
    {
        private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
        private readonly ILogger<ProjectUsageUpdater> _logger = Substitute.For<ILogger<ProjectUsageUpdater>>();

        private Mock<IWorkspaceCostManagementService> _workspaceCostMgmtServiceMock;
        private Mock<IWorkspaceBudgetManagementService> _workspaceBudgetMgmtServiceMock;
        private Mock<IWorkspaceStorageManagementService> _workspaceStorageMgmtServiceMock;
        private Mock<ISendEndpointProvider> _sendEndpointProviderMock;
        private Mock<IConfiguration> _configMock;
        private ProjectUsageUpdater _updater;

        [SetUp]
        public void SetUp()
        {
            _loggerFactory.CreateLogger<ProjectUsageUpdater>().Returns(_logger);
            _workspaceCostMgmtServiceMock = new Mock<IWorkspaceCostManagementService>();
            _workspaceBudgetMgmtServiceMock = new Mock<IWorkspaceBudgetManagementService>();
            _workspaceStorageMgmtServiceMock = new Mock<IWorkspaceStorageManagementService>();
            _sendEndpointProviderMock = new Mock<ISendEndpointProvider>();
            _configMock = new Mock<IConfiguration>();

            var datahubConfig = new DatahubPortalConfiguration();
            datahubConfig.AzureAd = new AzureAd
            {
                SubscriptionId = Guid.NewGuid().ToString(),
                TenantId = Guid.NewGuid().ToString(),
                InfraClientId = Guid.NewGuid().ToString(),
                InfraClientSecret = Guid.NewGuid().ToString()
            };

            _configMock.Setup(c => c.GetSection(It.IsAny<string>())).Returns(new Mock<IConfigurationSection>().Object);

            _updater = new ProjectUsageUpdater(
                _loggerFactory,
                _workspaceCostMgmtServiceMock.Object,
                _workspaceBudgetMgmtServiceMock.Object,
                _workspaceStorageMgmtServiceMock.Object,
                _sendEndpointProviderMock.Object,
                _configMock.Object
            );
        }

        [Test]
        public async Task Run_ShouldSucceed()
        {
            // Arrange
            var projectUpdateMessage = new ProjectUsageUpdateMessage("TEST", "costs.json", "totals.json", false);
            var messageEnvelope = new
            {
                message = projectUpdateMessage
            };
            var messageBody = System.Text.Json.JsonSerializer.Serialize(messageEnvelope);
            var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
                body: new BinaryData(messageBody));
            _updater.Mock = true;

            // Act
            Func<Task> act = async () => await _updater.Run(serviceBusReceivedMessage, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Test]
        public async Task Run_ShouldThrowError_WhenValidationFails()
        {
            // Arrange
            var projectUpdateMessage = new ProjectUsageUpdateMessage("TEST", "costs", "totals", false);
            var messageEnvelope = new
            {
                message = projectUpdateMessage
            };
            var messageBody = System.Text.Json.JsonSerializer.Serialize(messageEnvelope);
            var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
                body: new BinaryData(messageBody));

            // Act
            Func<Task> act = async () => await _updater.Run(serviceBusReceivedMessage, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Validation failed: \r\n -- CostsBlobName: The specified condition was not met for 'Costs Blob Name'. Severity: Error");
        }

        [Test]
        public async Task UpdateProjectCapacity_ShouldSucceed()
        {
            // Arrange
            var projectUpdateMessage = new ProjectCapacityUpdateMessage("TEST", false);
            var messageEnvelope = new
            {
                message = projectUpdateMessage
            };
            var messageBody = System.Text.Json.JsonSerializer.Serialize(messageEnvelope);
            var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
                body: new BinaryData(messageBody));
            _updater.Mock = true;

            // Act
            Func<Task> act = async () => await _updater.UpdateProjectCapacity(serviceBusReceivedMessage, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _loggerFactory?.Dispose();
        }
    }
}
