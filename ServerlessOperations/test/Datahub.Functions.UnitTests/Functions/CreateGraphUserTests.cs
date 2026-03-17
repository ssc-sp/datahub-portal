using System.Text.Json;
using System.Text.Json.Nodes;
using Datahub.Application.Services.Notification;
using Datahub.Application.Services.UserManagement;
using Datahub.Infrastructure.Services.UserManagement;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Datahub.Functions.UnitTests.Functions
{
    [TestFixture]
    public class CreateGraphUserTests
    {
        private IConfiguration _config = Substitute.For<IConfiguration>();
        private AzureConfig _azureConfig;
        private IMSGraphService _azureManagementService;
        private ISendEndpointProvider _sendEndpointProvider;
        private IGCNotifyService _gcNotifyService;
        private CreateGraphUser _function; 

        [SetUp]
        public void Setup()
        {
            var loggerFactory = Substitute.For<ILoggerFactory>(); 
            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            var httpClient = new HttpClient();

            httpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);

            _azureConfig = new AzureConfig(_config);
            _gcNotifyService = Substitute.For<IGCNotifyService>();

            _sendEndpointProvider = Substitute.For<ISendEndpointProvider>();
            
            var _mockGraphClient = TestHelper.MockGraphServiceClient();
            _azureManagementService = Substitute.For<IMSGraphService>(); 
            _azureManagementService.GetAuthenticatedClient().Returns(_mockGraphClient);
            _function = new CreateGraphUser(loggerFactory, _azureConfig, _azureManagementService, _sendEndpointProvider, _gcNotifyService);
        }

        [Test]
        public async Task RunAsync_ShouldReturnBadRequest_WhenEmailIsInvalid()
        {
            // Arrange
            var request = new CreateUserRequest("invalid-email", "false", "datahub");
            var requestBody = JsonSerializer.Serialize(request);
            var httpRequestData = TestHelper.CreateHttpRequestData(requestBody);

            // Act
            var result = await _function.RunAsync(httpRequestData);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            ((BadRequestObjectResult)result).Value.Should().Be("Please pass a valid email address in the request body");
        }

        [Test]
        public async Task RunAsync_ShouldReturnOk_WhenMockInviteIsTrue()
        {
            // Arrange
            var request = new CreateUserRequest("user@example.com", "true", "datahub");
            var requestBody = JsonSerializer.Serialize(request);
            var httpRequestData = TestHelper.CreateHttpRequestData(requestBody);

            // Act
            var result = await _function.RunAsync(httpRequestData);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            var response = JsonSerializer.Deserialize<JsonObject>(okResult.Value.ToString());
            response["message"].ToString().Should().Contain("Successfully FAKE invited user@example.com");
        }

        [Test]
        public async Task RunAsync_ShouldReturnOk_WhenMockInviteIsFalse()
        {
            // Arrange
            var request = new CreateUserRequest("mockuser@example.com", "false", "datahub");
            var requestBody = JsonSerializer.Serialize(request);
            var httpRequestData = TestHelper.CreateHttpRequestData(requestBody);

            // Act
            var result = await _function.RunAsync(httpRequestData);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            var response = JsonSerializer.Deserialize<JsonObject>(okResult.Value?.ToString());
            response["message"].ToString().Should().Contain("Successfully invited mockuser@example.com and added to group");
        }

        [Test]
        public async Task RunAsync_ShouldReturnBadRequest_WhenExceptionIsThrown()
        {
            // Arrange
            var request = new CreateUserRequest("user_with_wrong_email", "false", "datahub");
            var requestBody = JsonSerializer.Serialize(request);
            var httpRequestData = TestHelper.CreateHttpRequestData(requestBody);
            

            // Act
            var result = await _function.RunAsync(httpRequestData);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        record CreateUserRequest(string email, string mockInvite, string inviter);
    }
}
