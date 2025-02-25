using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using NSubstitute;
using Microsoft.Extensions.Configuration;

namespace Datahub.Functions.UnitTests
{
    public class GetUsersStatusTests
    {
        private ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
        private IConfiguration _config = Substitute.For<IConfiguration>();
        private AzureConfig _azureConfig;
        private Mock<GetUsersStatus> _functionMock;

        [SetUp]
        public void Setup()
        {
            _azureConfig = new AzureConfig(_config);
            _functionMock = new Mock<GetUsersStatus>(_loggerFactory, _azureConfig) { CallBase = true };

            var _mockGraphClient = TestHelper.MockGraphServiceClient();
            _functionMock.Setup(f => f.GetAuthenticatedGraphClient()).Returns(_mockGraphClient);
        }

        [Test]
        public async Task GetUsersDetails_ShouldReturnOk_WhenUsersAreFetchedSuccessfully()
        {
            // Arrange
            var httpRequestData = TestHelper.CreateHttpRequestData(string.Empty);

            // Act
            var result = await _functionMock.Object.GetUsersDetails(httpRequestData);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _loggerFactory?.Dispose();
        }
    }
}
