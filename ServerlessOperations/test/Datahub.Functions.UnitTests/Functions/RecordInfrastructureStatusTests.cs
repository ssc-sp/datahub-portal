using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Health;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using NUnit.Framework;
using Octokit;

namespace Datahub.Functions.UnitTests
{
    [TestFixture]
    public class RecordInfrastructureStatusTests
    {
        private ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
        private IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
        private RecordInfrastructureStatus _recordInfrastructureStatus;

        [SetUp]
        public async Task SetUp()
        {
            _dbContextFactory = TestHelper.CreateMockDbContextFactory();
            await TestHelper.SeedDatabase(_dbContextFactory);
            var ctx = await _dbContextFactory.CreateDbContextAsync();
            _recordInfrastructureStatus = new RecordInfrastructureStatus(_loggerFactory, ctx);
        }

        [Test]
        public async Task RecordHealthCheckHttp_ShouldReturnOkResult_WhenRequestIsValid()
        {
            // Arrange
            var requestMessage = new InfrastructureHealthCheckResultMessage(
                "TestGroup",
                "TestName",
                InfrastructureHealthResourceType.AzureWebApp,
                InfrastructureHealthStatus.Healthy,
                DateTime.UtcNow,
                "All systems operational"
            );
            var requestBody = System.Text.Json.JsonSerializer.Serialize(requestMessage);
            var httpRequest = TestHelper.CreateHttpRequestData(requestBody);

            // Act
            var result = await _recordInfrastructureStatus.RecordHealthCheckHttp(httpRequest);

            // Assert
            result.Should().BeOfType<OkResult>();
        }

        [Test]
        public async Task RecordHealthCheckHttp_ShouldReturnBadRequest_WhenRequestIsInvalid()
        {
            // Arrange
            var httpRequest = TestHelper.CreateHttpRequestData("");             

            // Act
            Func<Task> act = async () => await _recordInfrastructureStatus.RecordHealthCheckHttp(httpRequest);

            // Assert
            await act.Should().ThrowAsync<System.Text.Json.JsonException>();
        }
  

        [Test]
        public async Task RecordHealthCheckQueue_ShouldReturnOkResult_WhenMessageIsValid()
        {
            // Arrange
            var requestMessage = new InfrastructureHealthCheckResultMessage(
               "TestGroup",
               "TestName",
               InfrastructureHealthResourceType.AzureWebApp,
               InfrastructureHealthStatus.Healthy,
               DateTime.UtcNow,
               "All systems operational"
            );
            var messageEnvelope = new
            {
                message = requestMessage
            };
            var messageBody = System.Text.Json.JsonSerializer.Serialize(messageEnvelope);
            var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
                body: new BinaryData(messageBody));

            // Act
            var result = await _recordInfrastructureStatus.RecordHealthCheckQueue(serviceBusReceivedMessage);

            // Assert
            result.Should().BeOfType<OkResult>();
        }

        [Test]
        public async Task RecordHealthCheckQueue_ShouldReturnBadRequest_WhenMessageIsInvalid()
        {
            // Arrange 
            var messageEnvelope = new
            {
                message = "InvalidMessage"
            };
            var messageBody = System.Text.Json.JsonSerializer.Serialize(messageEnvelope);
            var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
                body: new BinaryData(messageBody));
             
            // Act
            Func<Task> act = async () => await _recordInfrastructureStatus.RecordHealthCheckQueue(serviceBusReceivedMessage);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("The JSON value could not be converted*");
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _loggerFactory?.Dispose();
        }
    }
}
