using System.Net;
using System.Threading.Tasks;
using Datahub.Functions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using NUnit.Framework;
using FluentAssertions;

namespace Datahub.Functions.UnitTests
{
    [TestFixture]
    public class FunctionsHealthCheckTests
    {
        private FunctionsHealthCheck _functionsHealthCheck; 
        private ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();

        [SetUp]
        public void SetUp()
        { 

            _functionsHealthCheck = new FunctionsHealthCheck(_loggerFactory);
        }

        [Test]
        public async Task Run_ShouldReturnSuccessResponse()
        {
            // Arrange
            var context = new Mock<FunctionContext>();
            var request = TestHelper.CreateHttpRequestData(string.Empty);
            var response = new Mock<HttpResponseData>(context.Object);

            response.SetupProperty(res => res.Headers);
            //response.Setup(res => res.WriteString(It.IsAny<string>(), It.IsAny<System.Text.Encoding>())).Callback<string>(s => { });

            // Act
            var result = _functionsHealthCheck.Run(request);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Headers.Should().ContainKey("Content-Type");

            result.Headers.TryGetValues("Content-Type", out var ctValues);
            ctValues.Should().HaveCount(1);
            ctValues.Should().Contain("text/plain; charset=utf-8");
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _loggerFactory?.Dispose();
        }
    }
}
