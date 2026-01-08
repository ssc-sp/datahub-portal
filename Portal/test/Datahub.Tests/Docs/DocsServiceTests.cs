using Datahub.Core.Services.Docs;
using Datahub.Markdown;
using Datahub.Markdown.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Xunit;
using Azure;
using System.Collections.Generic;

namespace Datahub.Tests.Docs
{
    public class DocsServiceTests
    {
        private DocumentationService _service;
        private readonly Mock<BlobServiceClient> _mockBlobServiceClient;
        private readonly Mock<BlobContainerClient> _mockBlobContainerClient;
        private readonly Mock<BlobClient> _mockBlobClient;
        private IConfiguration _configuration;
        private ILogger<DocumentationService> _logger;
        private Mock<IHttpClientFactory> _mockFactory = new Mock<IHttpClientFactory>();
        private Mock<IWebHostEnvironment> _webHostingEnvironment = new Mock<IWebHostEnvironment>();
        private MemoryCache _memCache;

        public DocsServiceTests()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"Media:StorageConnectionString", "UseDevelopmentStorage=true"},
                {"Media:SasToken", "your-sas-token"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();


            var services = new ServiceCollection();
            services.AddSingleton<DocumentationService>();
            services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
            services.AddSingleton<IConfiguration>(_configuration);
            var provider = services.BuildServiceProvider();
            using var logFactory = LoggerFactory.Create(builder => builder.AddConsole());
            _logger = logFactory.CreateLogger<DocumentationService>();
             
            var httpClient = new HttpClient();

            _mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            _memCache = new MemoryCache(new MemoryCacheOptions());

            _webHostingEnvironment.Setup(e => e.EnvironmentName).Returns(Environments.Development);

            _mockBlobServiceClient = new Mock<BlobServiceClient>();
            _mockBlobContainerClient = new Mock<BlobContainerClient>();
            _mockBlobClient = new Mock<BlobClient>();

            _service = new DocumentationService(_configuration, _logger, _mockFactory.Object, _webHostingEnvironment.Object, _memCache);
        }

        [Fact]
        public void TestCompareCultureStrings()
        {
            Assert.True(MarkdownTools.CompareCulture("en-us", "en"));
            Assert.True(MarkdownTools.CompareCulture("en-ca", "EN"));
            Assert.True(MarkdownTools.CompareCulture("fr-ca", "FR"));
            Assert.False(MarkdownTools.CompareCulture("fr-ca", "en"));
        }

        [Fact(Skip = "Needs to be validated")]
        public async Task Test1LoadEnglishSidebar()
        {
            var root = await _service.LoadResourceTree(DocumentationGuideRootSection.UserGuide, "en");
            Assert.NotNull(root);
            Assert.True(root.Children.Count > 5);
        }

        [Fact(Skip = "Needs to be validated")]
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

        [Fact]
        public async Task TestReadLastCommitTS()
        {
            // Arrange
            var expectedTimestamp = SetupMockBlobServiceClient();

            // new instance of DocumentationService
            var documentationService = new DocumentationService(
                _configuration,
                _logger,
                _mockFactory.Object,
                _webHostingEnvironment.Object,
                _memCache
            );
            // overwrite BlobServiceClient for test
            documentationService.InitBlobClient(_mockBlobServiceClient.Object);

            // Act
            var result = await documentationService.LastRepoCommitTs();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedTimestamp.UtcDateTime, result);
        }

        private DateTimeOffset SetupMockBlobServiceClient()
        {
            var timestamp = DateTimeOffset.UtcNow;
            var properties = BlobsModelFactory.BlobProperties(lastModified: timestamp);
            var response = new Mock<Response<BlobProperties>>();
            response.Setup(r => r.Value).Returns(properties);

            _mockBlobServiceClient
               .Setup(b => b.GetBlobContainerClient(It.IsAny<string>()))
               .Returns(_mockBlobContainerClient.Object);

            _mockBlobContainerClient
                .Setup(c => c.GetBlobClient(It.IsAny<string>()))
                .Returns(_mockBlobClient.Object);

            _mockBlobContainerClient
                .Setup(c => c.Uri)
                .Returns(new Uri("https://mockstorageaccount.blob.core.windows.net/container"));

            _mockBlobClient
                .Setup(b => b.GetPropertiesAsync(null, default))
                .ReturnsAsync(response.Object);

            return timestamp;
        }

    }
}
