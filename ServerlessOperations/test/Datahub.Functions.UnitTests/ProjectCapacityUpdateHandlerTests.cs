using Azure.Messaging.ServiceBus;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Queues;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Datahub.Functions.UnitTests;

public class ProjectCapacityUpdateHandlerTests
{
    private ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
    private IMessageReceiver _receiverMock = Substitute.For<IMessageReceiver>();
    private ServiceBusReceivedMessage _requestMessageMock = Substitute.For<ServiceBusReceivedMessage>();
    [Test]
    public async Task Run_ShouldHandleConsumer()
    {
        // Arrange 
        var cancellationToken = CancellationToken.None;

        var handler = new ProjectCapacityUpdateHandler(_loggerFactory, _receiverMock);

        // Act
        await handler.Run(_requestMessageMock, cancellationToken);

        // Assert
        await _receiverMock.Received(1).HandleConsumer<ProjectCapacityUpdateConsumer>(
            QueueConstants.QUEUE_PROJECT_CAPACITY_UPDATE,
            _requestMessageMock,
            cancellationToken);
    }
    [OneTimeTearDown]
    public void TearDown()
    {
        _loggerFactory?.Dispose();
        _receiverMock?.Dispose();
    }
} 