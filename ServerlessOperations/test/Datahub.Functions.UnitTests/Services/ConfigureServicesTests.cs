using Datahub.Application.Configuration;
using Datahub.Functions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;

namespace Datahub.Functions.UnitTests.Services
{
    [TestFixture]
    public class ConfigureServicesTests
    {
        private IServiceCollection _services;

        private const string TENANT_ID = "tenant-id";
        private const string CLIENT_ID = "client-id";
        private const string CLIENT_SECRET = "client-secret";
        private const string INFRA_CLIENT_ID = "infra-client-id";
        private const string INFRA_CLIENT_SECRET = "infra-client-secret";
        private const string CONNECTION_STRING = "service-bus-connection-string";

        [SetUp]
        public void SetUp()
        {
            _services = new ServiceCollection();
        }

        private IConfiguration CreateConfiguration(
            string tenantId = TENANT_ID,
            string clientId = CLIENT_ID,
            string clientSecret = CLIENT_SECRET,
            string infraClientId = INFRA_CLIENT_ID,
            string infraClientSecret = INFRA_CLIENT_SECRET,
            string connectionString = CONNECTION_STRING)
        {
            var configBuilder = new ConfigurationBuilder();
            var inMemorySettings = new Dictionary<string, string>
            {
                { ConfigureServices.TENANT_ID_KEY, tenantId },
                { ConfigureServices.PORTAL_CLIENT_ID_KEY, clientId },
                { ConfigureServices.PORTAL_CLIENT_SECRET_KEY, clientSecret },
                { ConfigureServices.DEVOPS_CLIENT_ID_KEY, infraClientId },
                { ConfigureServices.DEVOPS_CLIENT_SECRET_KEY, infraClientSecret },
                { ConfigureServices.DATAHUB_SERVICE_BUS_CONNECTION_STRING_KEY, connectionString }
            };

            configBuilder.AddInMemoryCollection(inMemorySettings);
            return configBuilder.Build();
        }

        [Test]
        public void AddDatahubConfigurationFromFunctionFormat_ShouldThrowArgumentNullException_WhenTenantIdIsNull()
        {
            // Arrange
            var configuration = CreateConfiguration(tenantId: null);

            // Act
            Action act = () => _services.AddDatahubConfigurationFromFunctionFormat(configuration);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*TENANT_ID*");
        }

        [Test]
        public void AddDatahubConfigurationFromFunctionFormat_ShouldThrowArgumentNullException_WhenClientIdIsNull()
        {
            // Arrange
            var configuration = CreateConfiguration(clientId: null);

            // Act
            Action act = () => _services.AddDatahubConfigurationFromFunctionFormat(configuration);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*FUNC_SP_CLIENT_ID*");
        }

        [Test]
        public void AddDatahubConfigurationFromFunctionFormat_ShouldThrowArgumentNullException_WhenClientSecretIsNull()
        {
            // Arrange
            var configuration = CreateConfiguration(clientSecret: null);

            // Act
            Action act = () => _services.AddDatahubConfigurationFromFunctionFormat(configuration);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*FUNC_SP_CLIENT_SECRET*");
        }

        [Test]
        public void AddDatahubConfigurationFromFunctionFormat_ShouldThrowArgumentNullException_WhenInfraClientIdIsNull()
        {
            // Arrange
            var configuration = CreateConfiguration(infraClientId: null);

            // Act
            Action act = () => _services.AddDatahubConfigurationFromFunctionFormat(configuration);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*AzureDevOpsConfiguration:ClientId*");
        }

        [Test]
        public void AddDatahubConfigurationFromFunctionFormat_ShouldThrowArgumentNullException_WhenInfraClientSecretIsNull()
        {
            // Arrange
            var configuration = CreateConfiguration(infraClientSecret: null);

            // Act
            Action act = () => _services.AddDatahubConfigurationFromFunctionFormat(configuration);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*AzureDevOpsConfiguration:ClientSecret*");
        }

        [Test]
        public void AddDatahubConfigurationFromFunctionFormat_ShouldAddConfigurationToServices_WhenAllValuesAreProvided()
        {
            // Arrange
            var configuration = CreateConfiguration();

            // Act
            _services.AddDatahubConfigurationFromFunctionFormat(configuration);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert - The method processes the configuration without throwing exceptions
            // and sets up MassTransit for Azure Functions
            serviceProvider.Should().NotBeNull();
        }
    }
}
