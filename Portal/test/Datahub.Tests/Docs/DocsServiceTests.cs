using Datahub.Core.Services.Docs;
using Datahub.Tests.Portal;
using Lucene.Net.Store;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Identity.Web.UI.Areas.MicrosoftIdentity.Controllers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Org.BouncyCastle.Math.EC.ECCurve;

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

            _service = new DocumentationService(config, logger, mockFactory.Object);
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
        public async Task Test1LoadLanguageRootEnglish()
        {
            var page = await _service.LoadLanguageRoot("en");
        }
    }
}
