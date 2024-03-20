using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ResourceProvisioner.Infrastructure.UnitTests
{
    public class ConfigureServicesTest
    {
        private IServiceCollection _services;
        private IConfiguration _configuration;

        [SetUp]
        public void Setup()
        {
            var configData = new Dictionary<string, string>
            {
                {"InfrastructureRepository:Username","username" },
                {"InfrastructureRepository:Password","password" }
            };
            _configuration = new ConfigurationBuilder() 
                .AddInMemoryCollection(configData)
                .Build();
            _services = new ServiceCollection();
           
        }

        [Test]
        public void AddInfustructureServices_ShouldAddServices()
        { 

            ConfigureServices.AddInfrastructureServices(_services, _configuration);

            var service = _services.BuildServiceProvider();
            var httpClientFactory = service.GetRequiredService<IHttpClientFactory>();
            var client = httpClientFactory.CreateClient("InfrastructureHttpClient");

            Assert.IsNotNull(client);
            Assert.That(client.DefaultRequestHeaders.Authorization.ToString(), 
                Is.EqualTo("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("username:password"))));
        }
    }
}
