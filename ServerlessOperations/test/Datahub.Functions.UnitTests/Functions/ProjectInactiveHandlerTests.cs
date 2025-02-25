using Azure.Messaging.ServiceBus;
using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq; 
using NSubstitute;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using System.Text.Json;

namespace Datahub.Functions.UnitTests.Functions
{
    public class ProjectInactiveHandlerTests
    {
        private ProjectInactiveHandler _sut = null!;
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory = Substitute.For<IDbContextFactory<DatahubProjectDBContext>>();
        private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
        private readonly ILogger<ProjectInactiveHandler> _logger = Substitute.For<ILogger<ProjectInactiveHandler>>();
        private readonly ISendEndpointProvider sendEndpointProvider = Substitute.For<ISendEndpointProvider>();
        private Mock<IQueuePongService> _pongService = null!;

        [SetUp]
        public void Setup()
        {
            _loggerFactory.CreateLogger<ProjectInactiveHandler>().Returns(_logger);
            _pongService = new Mock<IQueuePongService>();

            _sut = new ProjectInactiveHandler(_dbContextFactory, _pongService.Object, _loggerFactory);
        }

        [Test]
        public async Task RunAsync_ShouldReturn_WhenPongServiceReturnsFalse()
        {
            // Arrange
            var notification = new ProjectInactiveMessage("TEST");
            var messageBody = JsonSerializer.Serialize(new { message = notification });
            var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
                body: new BinaryData(messageBody));

            _pongService.Setup(x => x.Pong(It.IsAny<string>())).ReturnsAsync(false);

            // Act
            await _sut.RunAsync(message);

            // Assert
            _logger.Received().LogInformation("C# ServiceBus queue trigger started");
        }

        [Test]
        public async Task RunAsync_ShouldReturn_WhenPongServiceReturnsTrue()
        {
            // Arrange
            var notification = new ProjectInactiveMessage("TEST");
            var messageBody = JsonSerializer.Serialize(new { message = notification });
            var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
                body: new BinaryData(messageBody));

            _pongService.Setup(x => x.Pong(It.IsAny<string>())).ReturnsAsync(true);

            // Act
            await _sut.RunAsync(message);

            // Assert
            _logger.Received().LogInformation("C# ServiceBus queue trigger started");
        }

        [Test]
        public async Task RunAsync_ShouldThrowException_WhenHandleInactiveProjectThrowsException()
        {
            // Arrange
            var notification = new ProjectInactiveMessage("TEST");
            var messageBody = JsonSerializer.Serialize(new { message = notification });
            var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
                body: new BinaryData(messageBody));

            _pongService.Setup(x => x.Pong(It.IsAny<string>())).ReturnsAsync(false);

            _sut = Substitute.ForPartsOf<ProjectInactiveHandler>(_dbContextFactory, _pongService.Object, _loggerFactory);
            _sut.When(x => x.HandleInactiveProject(Arg.Any<ProjectInactiveMessage>())).Do(x => { throw new Exception("Test exception"); });

            // Act
            try
            {
                await _sut.RunAsync(message);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.Pass();
            }
            // Assert 
            _logger.Received().LogError(Arg.Any<Exception>(), "Error processing project inactive message");
        }

        [Test]
        public async Task HandleInactiveProject_ShouldLogInformation_WhenCalled()
        {
            // Arrange
            var output = new ProjectInactiveMessage("TEST");

            // Act
            await _sut.HandleInactiveProject(output);

            // Assert
            _logger.Received().LogInformation("This workspace should be set to be deleted, but functionality is not ready yet.");
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _loggerFactory?.Dispose();
        }
    }
}
