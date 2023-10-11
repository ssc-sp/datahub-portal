using Datahub.Core.Services.Docs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Datahub.Tests.Docs
{
    public class DocsServiceTests
    {
        private DocumentationService _service;

        public DocsServiceTests()
        {
            var builder = new ConfigurationBuilder().AddJsonFile($"testsettings.json", optional: true);
            var config = builder.Build();

            var services = new ServiceCollection();
            services.AddSingleton<DocumentationService>();
            services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
            services.AddSingleton<IConfiguration>(config);
            var provider = services.BuildServiceProvider();
            using var logFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = logFactory.CreateLogger<DocumentationService>();
            var mockFactory = new Mock<IHttpClientFactory>();
            var httpClient = new HttpClient();

            //httpClient.BaseAddress = new Uri("http://nonexisting.domain"); //New code

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            // Arrange
            var memCache = new MemoryCache(new MemoryCacheOptions());
            _service = new DocumentationService(config, logger, mockFactory.Object,memCache);
        }

        [Fact]
        public async void TestReadLastCommitTS()
        {
            var lastCommit = await _service.GetLastRepoCommitTS();
            Assert.NotNull(lastCommit);
        }


        [Fact]
        public void TestCompareCultureStrings()
        {
            Assert.True(DocumentationService.CompareCulture("en-us", "en"));
            Assert.True(DocumentationService.CompareCulture("en-ca", "EN"));
            Assert.True(DocumentationService.CompareCulture("fr-ca", "FR"));
            Assert.False(DocumentationService.CompareCulture("fr-ca", "en"));
        }

        [Fact]
        public async Task Test1LoadEnglishSidebar()
        {
            var root = await _service.GetLanguageRoot(DocumentationGuideRootSection.UserGuide,"en");
            Assert.NotNull(root);
            Assert.True(root.Children.Count > 10);
        }

        [Fact]
        public async Task TestLoadPage()
        {
            var root = await _service.GetLanguageRoot(DocumentationGuideRootSection.UserGuide, "en");
            Assert.NotNull(root);
            Assert.True(root.Children.Count > 10);
            var pageId = root.Children[9].Id!;
            var loadedPage = _service.LoadPage(pageId, false);
            Assert.NotNull(loadedPage);
            var parent = _service.GetParent(loadedPage);
            Assert.NotNull(parent);
        }
    }
}
