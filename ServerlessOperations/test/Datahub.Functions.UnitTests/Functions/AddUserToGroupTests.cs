using Azure.Identity;
using Datahub.Functions.Services;
using Datahub.Infrastructure.Services.Azure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Configuration;

namespace Datahub.Functions.UnitTests.Functions
{
    [TestFixture]
    public class AddUserToGroupTests
    {
        private Mock<AzureManagementService> _azureManagementService;
        private AzureConfig _azureConfig;
        private AddUserToGroupRequest _validRequest;
        private AddUserToGroupRequest _invalidRequest;
        private CreateGraphUser _function;

        [SetUp]
        public void Setup()
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            var httpClient = new HttpClient();

            httpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);

            _azureConfig = new AzureConfig(Substitute.For<IConfiguration>());
            _azureManagementService = new Mock<AzureManagementService>(MockBehavior.Strict, _azureConfig, httpClientFactory);
            _azureManagementService.Setup(f => f.GetGraphServiceClientFromEnvVariables())
                .Returns(TestHelper.MockGraphServiceClient());

            _function = new CreateGraphUser(loggerFactory, _azureConfig, _azureManagementService.Object, Substitute.For<ISendEndpointProvider>(), Substitute.For<IEmailService>());

            _validRequest = new AddUserToGroupRequest(Guid.NewGuid().ToString());
            _invalidRequest = new AddUserToGroupRequest("");
        }

        [Test]
        public async Task AddUserToGroup_ShouldReturnBadRequest_WhenUserIdIsInvalid()
        {
            // Arrange
            var requestBody = JsonSerializer.Serialize(_invalidRequest);
            var httpRequestData = TestHelper.CreateHttpRequestData(requestBody);

            // Act
            var result = await _function.AddUserToGroup(httpRequestData);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            ((BadRequestObjectResult)result).Value.Should().Be("Please pass a valid user ID in the request body");
        }

        [Test]
        public async Task AddUserToGroup_ShouldReturnOk_WhenUserIsAddedSuccessfully()
        {
            // Arrange
            var requestBody = JsonSerializer.Serialize(_validRequest);
            var httpRequestData = TestHelper.CreateHttpRequestData(requestBody);

            // Act
            var result = await _function.AddUserToGroup(httpRequestData);

            // Assert
            result.Should().BeOfType<OkResult>();
        }

        [Test]
        public async Task AddUserToGroup_ShouldReturnBadRequest_WhenExceptionIsThrown()
        {
            // Arrange
            _azureManagementService.Setup(f => f.GetGraphServiceClientFromEnvVariables())
                .Throws(new Exception("Graph API error"));
            var requestBody = JsonSerializer.Serialize(_validRequest);
            var httpRequestData = TestHelper.CreateHttpRequestData(requestBody);

            // Act
            var result = await _function.AddUserToGroup(httpRequestData);

            // Assert
            result.Should().BeOfType<BadRequestResult>();
        }

        record AddUserToGroupRequest(string userId);
    }
}
