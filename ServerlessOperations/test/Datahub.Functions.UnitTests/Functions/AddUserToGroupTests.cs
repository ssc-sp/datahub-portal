using System.Text.Json;
using Azure.Identity;
using Datahub.Application.Services.UserManagement;
using Datahub.Functions.Services;
using Datahub.Infrastructure.Services.Azure;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Datahub.Functions.UnitTests.Functions
{
    [TestFixture]
    public class AddUserToGroupTests
    {
        private IMSGraphService _graphService;
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
            _graphService = Substitute.For<IMSGraphService>();
            _graphService.GetAuthenticatedClient().Returns(TestHelper.MockGraphServiceClient());

            _function = new CreateGraphUser(loggerFactory, _azureConfig, _graphService, Substitute.For<ISendEndpointProvider>(), null);

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
            _graphService.GetAuthenticatedClient().Throws(new Exception("Graph API error"));
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
