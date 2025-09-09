using System.Net;
using System.Threading.Tasks;
using Datahub.Functions;
using Datahub.Application.Services.Notification;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using NUnit.Framework;
using FluentAssertions;

namespace Datahub.Functions.UnitTests
{
    [TestFixture]
    public class FunctionsHealthCheckTests
    {
        private FunctionsHealthCheck _functionsHealthCheck;
        private ILoggerFactory _loggerFactory;
        private IGCNotifyService _gcNotifyService;
        private IMemoryCache _memoryCache;

        [SetUp]
        public void SetUp()
        {
            _loggerFactory = Substitute.For<ILoggerFactory>();
            _loggerFactory.CreateLogger<FunctionsHealthCheck>()
                .Returns(Substitute.For<ILogger<FunctionsHealthCheck>>());

            _gcNotifyService = Substitute.For<IGCNotifyService>();
            _gcNotifyService.CheckHealthAsync().Returns(Task.FromResult(true));

            // Use a real MemoryCache to exercise caching logic
            _memoryCache = new MemoryCache(new MemoryCacheOptions());

            _functionsHealthCheck = new FunctionsHealthCheck(_loggerFactory, _gcNotifyService, _memoryCache);
        }

        [Test]
        public async Task Run_ShouldReturnSuccessResponse_AndInvokeHealthCheck_WhenNotCached()
        {
            // Arrange
            var request = TestHelper.CreateHttpRequestData(string.Empty);

            // Act
            var result = await _functionsHealthCheck.Run(request);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Headers.Should().ContainKey("Content-Type");
            result.Headers.Should().ContainKey("X-Cache").WhoseValue.Should().ContainSingle(v => v == "MISS");
            result.Headers.TryGetValues("Content-Type", out var ctValues);
            ctValues.Should().ContainSingle(v => v == "text/plain; charset=utf-8");

            await _gcNotifyService.Received(1).CheckHealthAsync();
        }

        [Test]
        public async Task Run_ShouldUseCachedResult_OnSubsequentInvocation_AndNotCallHealthCheckAgain()
        {
            // Arrange
            var request1 = TestHelper.CreateHttpRequestData(string.Empty);
            var request2 = TestHelper.CreateHttpRequestData(string.Empty);

            // Act
            var first = await _functionsHealthCheck.Run(request1);
            var second = await _functionsHealthCheck.Run(request2);

            // Assert first call (MISS)
            first.Headers.Should().ContainKey("X-Cache").WhoseValue.Should().ContainSingle(v => v == "MISS");
            // Assert second call (HIT)
            second.Headers.Should().ContainKey("X-Cache").WhoseValue.Should().ContainSingle(v => v == "HIT");

            await _gcNotifyService.Received(1).CheckHealthAsync();
        }

        [TearDown]
        public void TearDown()
        {
            _loggerFactory?.Dispose();
            _memoryCache?.Dispose();
        }
    }
}
