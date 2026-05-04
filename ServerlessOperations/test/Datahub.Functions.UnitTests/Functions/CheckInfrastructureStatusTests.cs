using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Shared.Entities;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Helpers;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using NUnit.Framework;
using Datahub.Application.Services.Security;

namespace Datahub.Functions.UnitTests
{
    [TestFixture]
    public class CheckInfrastructureStatusTests
    {
        private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
        private readonly ILogger<CheckInfrastructureStatus> _logger = Substitute.For<ILogger<CheckInfrastructureStatus>>();
        private readonly IHttpClientFactory _httpClientFactory = Substitute.For<IHttpClientFactory>();
        private Mock<HealthCheckHelper> _healthCheckHelperMock;
        private CheckInfrastructureStatus _checkInfrastructureStatus;

        [SetUp]
        public async Task SetUp()
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
            var tokenCredentialService = Substitute.For<ISystemTokenCredentialService>();
            var projectStorageConfigurationService = new ProjectStorageConfigurationService(datahubConfig,tokenCredentialService);

            var dbContextFactory = TestHelper.CreateMockDbContextFactory();
            await TestHelper.SeedDatabase(dbContextFactory);

            var sendProvider = Substitute.For<ISendEndpointProvider>();
            var webAppService = TestHelper.CreateMockWebAppManagementService();
            var workspaceVersionService = Substitute.For<IWorkspaceVersionService>(); 
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();

            var resourceMessagingService = new ResourceMessagingService(dbContextFactory, sendProvider, workspaceVersionService);

            var healthCheckHelper = new HealthCheckHelper(dbContextFactory, projectStorageConfigurationService, webAppService,
                Testing._configuration, _httpClientFactory, _loggerFactory, sendProvider, resourceMessagingService, datahubConfig, httpContextAccessor, null);

            _checkInfrastructureStatus = new CheckInfrastructureStatus(_loggerFactory, healthCheckHelper);
        }

        [Test]
        public async Task RunCheckTimer_ShouldReturnOkObjectResult()
        {
            // Arrange
            var timerInfo = new TimerInfo(); 

            // Act
            var result = await _checkInfrastructureStatus.RunCheckTimer(timerInfo);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().NotBeNull();
        }

        [Test]
        public async Task RunHealthCheckHttp_ShouldReturnOkObjectResult()
        {
            // Arrange
            var request = new InfrastructureHealthCheckMessage(InfrastructureHealthResourceType.AzureWebApp,"Group","Test");
            var requestBody = JsonSerializer.Serialize(request);
            var httpRequest = TestHelper.CreateHttpRequestData(requestBody);

            // Act
            var result = await _checkInfrastructureStatus.RunHealthCheckHttp(httpRequest);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().NotBeNull();
        }

        [Test]
        public async Task RunHealthCheckQueue_ShouldReturnOkObjectResult()
        {
            // Arrange
            var healthCheckMessage = new InfrastructureHealthCheckMessage(InfrastructureHealthResourceType.AzureWebApp, "Group", "Test");
            var messageBody = JsonSerializer.Serialize(new { message = healthCheckMessage });
            var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(body: new BinaryData(messageBody));    

            // Act
            var result = await _checkInfrastructureStatus.RunHealthCheckQueue(serviceBusReceivedMessage);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().NotBeNull();
        }

        [Test]
        public async Task ProcessRequest_ShouldReturnBadRequestResult_WhenRequestIsNull()
        {
            // Act
            var result = await _checkInfrastructureStatus.ProcessRequest(null);

            // Assert
            result.Should().BeOfType<BadRequestResult>();
        }

        [Test]
        public async Task ProcessRequest_ShouldReturnOkObjectResult_WhenRequestIsValid()
        {
            // Arrange
            var request = new InfrastructureHealthCheckMessage(InfrastructureHealthResourceType.AzureWebApp, "Group", "Test");
            var requestBody = JsonSerializer.Serialize(request);
            var httpRequest = TestHelper.CreateHttpRequestData(requestBody);

            // Act
            var result = await _checkInfrastructureStatus.ProcessRequest(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().NotBeNull();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _loggerFactory?.Dispose();
        }
    }
}
