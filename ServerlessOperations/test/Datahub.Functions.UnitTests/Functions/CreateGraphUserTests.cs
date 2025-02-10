using Azure.Identity;
using Datahub.Functions.Services;
using Datahub.Infrastructure.Services.Azure;
using FluentAssertions;
using MassTransit;
using MassTransit.Transports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using Microsoft.Kiota.Serialization.Json;
using Moq;
using Moq.Protected;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes; 

namespace Datahub.Functions.UnitTests.Functions
{
    [TestFixture]
    public class CreateGraphUserTests
    {
        private IConfiguration _config = Substitute.For<IConfiguration>();
        private AzureConfig _azureConfig;
        private Mock<AzureManagementService> _azureManagementService;
        private ISendEndpointProvider _sendEndpointProvider;
        private IEmailService _emailService;
        private CreateGraphUser _function; 

        [SetUp]
        public void Setup()
        {
            var loggerFactory = Substitute.For<ILoggerFactory>(); 
            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            var httpClient = new HttpClient();

            httpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);

            _azureConfig = new AzureConfig(_config);
            _emailService = new EmailService(loggerFactory.CreateLogger<EmailService>());

            _sendEndpointProvider = Substitute.For<ISendEndpointProvider>();
            _emailService = Substitute.For<IEmailService>();
            
            var _mockGraphClient = MockGraphServiceClient();
            _azureManagementService = new Mock<AzureManagementService>(MockBehavior.Strict,_azureConfig, httpClientFactory);
            _azureManagementService.Setup(f => f.GetGraphServiceClientFromEnvVariables()).Returns(_mockGraphClient);

            _function = new CreateGraphUser(loggerFactory, _azureConfig, _azureManagementService.Object, _sendEndpointProvider, _emailService);

        }

        /// <summary>
        /// Mocking GraphServiceClient
        /// based on https://medium.com/@carlosedgarnovo_56347/unit-testing-microsoft-graphserviceclient-in-c-net-d86a33e9158b
        /// </summary>
        /// <returns>Mock GraphServiceClient</returns>
        private GraphServiceClient MockGraphServiceClient()
        {
            Mock<IRequestAdapter> _requestAdapterMock = new();
            Mock<ISerializationWriterFactory> _serializationWriterFactoryMock = new();

            _serializationWriterFactoryMock.Setup(factory => factory.GetSerializationWriter(It.IsAny<string>())).Returns(new JsonSerializationWriter());

            _requestAdapterMock.SetupGet(adapter => adapter.BaseUrl).Returns("http://graph.test.internal/mock");
            _requestAdapterMock.SetupSet(adapter => adapter.BaseUrl = It.IsAny<string>());
            _requestAdapterMock.Setup(adapter => adapter.EnableBackingStore(It.IsAny<IBackingStoreFactory>()));
            _requestAdapterMock.SetupGet(adapter => adapter.SerializationWriterFactory).Returns(_serializationWriterFactoryMock.Object);

            // Initializing the GraphServiceClient using the mocked request adapter
            return new GraphServiceClient(_requestAdapterMock.Object);
        }
        [Test]
        public async Task RunAsync_ShouldReturnBadRequest_WhenEmailIsInvalid()
        {
            // Arrange
            var request = new CreateUserRequest("invalid-email", "false", "datahub");
            var requestBody = JsonSerializer.Serialize(request);
            var httpRequestData = CreateHttpRequestData(requestBody);

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
            var httpRequestData = CreateHttpRequestData(requestBody);

            // Act
            var result = await _function.RunAsync(httpRequestData);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            var response = JsonSerializer.Deserialize<JsonObject>(okResult.Value.ToString());
            response["message"].ToString().Should().Contain("Successfully FAKE invited user@example.com");
        }

        [Test]
        public async Task RunAsync_ShouldReturnBadRequest_WhenExceptionIsThrown()
        {
            // Arrange
            var request = new CreateUserRequest("user@example.com", "false", "datahub");
            var requestBody = JsonSerializer.Serialize(request);
            var httpRequestData = CreateHttpRequestData(requestBody);
            

            // Act
            var result = await _function.RunAsync(httpRequestData);

            // Assert
            result.Should().BeOfType<BadRequestResult>();
        }

        private static HttpRequestData CreateHttpRequestData(string requestBody)
        {
            var context = Substitute.For<FunctionContext>();
            var request = Substitute.For<HttpRequestData>(context);
            request.Body.Returns(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody)));
            request.Headers.Returns(new HttpHeadersCollection());
            request.Method.Returns("POST");
            request.Url.Returns(new Uri("http://localhost"));
            return request;
        }

        record CreateUserRequest(string email, string mockInvite, string inviter);
    }
}
