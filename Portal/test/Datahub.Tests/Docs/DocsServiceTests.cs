using Datahub.Core.Services.Docs;
using Datahub.Markdown;
using Datahub.Markdown.Model;
using Datahub.Portal.Components.Resources;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            var me = new Mock<IWebHostEnvironment>();
            me.Setup(e => e.EnvironmentName).Returns(Environments.Development);
            _service = new DocumentationService(config, logger, mockFactory.Object, me.Object, memCache);
        }

        [Fact]
        public async Task TestReadLastCommitTS()
        {
            var lastCommit = await _service.LastRepoCommitTs();
            Assert.NotNull(lastCommit);
        }

        [Fact]
        public void GivenIcon_ReadMudblazorIcon()
        {
            var iconData = DocItemHelper.GetMudblazorMaterialIcon("Outlined", "Workspaces");
            Assert.NotNull(iconData);
        }


        [Fact]
        public void TestCompareCultureStrings()
        {
            Assert.True(MarkdownTools.CompareCulture("en-us", "en"));
            Assert.True(MarkdownTools.CompareCulture("en-ca", "EN"));
            Assert.True(MarkdownTools.CompareCulture("fr-ca", "FR"));
            Assert.False(MarkdownTools.CompareCulture("fr-ca", "en"));
        }

        [Fact (Skip = "Needs to be validated")]
        public async Task Test1LoadEnglishSidebar()
        {
            var root = await _service.LoadResourceTree(DocumentationGuideRootSection.UserGuide,"en");
            Assert.NotNull(root);
            Assert.True(root.Children.Count > 5);
        }

        [Fact (Skip = "Needs to be validated")]
        public async Task TestLoadPage()
        {
            var root = await _service.LoadResourceTree(DocumentationGuideRootSection.UserGuide, "en");
            Assert.NotNull(root);
            Assert.True(root.Children.Count > 5);
            var pageId = root.Children[5].Id!;
            var loadedPage = _service.LoadPage(pageId, false);
            Assert.NotNull(loadedPage);
            var parent = _service.Parent(loadedPage);
            Assert.NotNull(parent);
        }
    }
}
