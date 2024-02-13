using Datahub.Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Infrastructure.UnitTests.CertValidation
{
    public class HttpClientSSLCertsTests
    {
        [Test]
        public async Task ConfigureSSLValidationAndCallURL()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<DataTransferTestService>();
            serviceCollection.AddHttpClientWithTrustedCertificate("test");
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetRequiredService<DataTransferTestService>();
            await service.CallURL();
        }
    }

    public class DataTransferTestService
    {
        private IHttpClientFactory _httpClientFactory;

        public DataTransferTestService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task CallURL()
        {
            var client = _httpClientFactory.CreateClient("test");
            var response = await client.GetAsync("https://raw.githubusercontent.com/ssc-sp/datahub-docs/main/DeveloperGuide/Accessibility/Overview.md");
            response.EnsureSuccessStatusCode();
        }
    }
}
