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
        private IConfiguration _configuration;
        private IServiceCollection _services;

        [SetUp]
        public void SetUp()
        {
            var jsonConfig = @"
            {
                ""AzureAd"": {
                    ""TenantId"": ""tenant-id"",
                    ""ClientId"": ""client-id"",
                    ""ClientSecret"": ""client-secret"",
                    ""InfraClientId"": ""infra-client-id"",
                    ""InfraClientSecret"": ""infra-client-secret""
                },
                ""DatahubServiceBus"": {
                    ""ConnectionString"": ""service-bus-connection-string""
                }
            }";

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonStream(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonConfig)));
            _configuration = configurationBuilder.Build();

            _services = new ServiceCollection();
        }

        [Test]
        public void AddDatahubConfigurationFromFunctionFormat_ShouldThrowArgumentNullException_WhenTenantIdIsNull()
        {
            // Arrange
            var jsonConfig = @"
            {
                ""AzureAd"": {
                    ""TenantId"": null,
                    ""ClientId"": ""client-id"",
                    ""ClientSecret"": ""client-secret"",
                    ""InfraClientId"": ""infra-client-id"",
                    ""InfraClientSecret"": ""infra-client-secret""
                },
                ""DatahubServiceBus"": {
                    ""ConnectionString"": ""service-bus-connection-string""
                }
            }";

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonStream(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonConfig)));
            _configuration = configurationBuilder.Build();

            // Act
            Action act = () => _services.AddDatahubConfigurationFromFunctionFormat(_configuration);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*TENANT_ID*");
        }

        [Test]
        public void AddDatahubConfigurationFromFunctionFormat_ShouldThrowArgumentNullException_WhenClientIdIsNull()
        {
            // Arrange
            var jsonConfig = @"
            {
                ""AzureAd"": {
                    ""TenantId"": ""tenant-id"",
                    ""ClientId"": null,
                    ""ClientSecret"": ""client-secret"",
                    ""InfraClientId"": ""infra-client-id"",
                    ""InfraClientSecret"": ""infra-client-secret""
                },
                ""DatahubServiceBus"": {
                    ""ConnectionString"": ""service-bus-connection-string""
                }
            }";

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonStream(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonConfig)));
            _configuration = configurationBuilder.Build();

            // Act
            Action act = () => _services.AddDatahubConfigurationFromFunctionFormat(_configuration);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*FUNC_SP_CLIENT_ID*");
        }

        [Test]
        public void AddDatahubConfigurationFromFunctionFormat_ShouldThrowArgumentNullException_WhenClientSecretIsNull()
        {
            // Arrange
            var jsonConfig = @"
            {
                ""AzureAd"": {
                    ""TenantId"": ""tenant-id"",
                    ""ClientId"": ""client-id"",
                    ""ClientSecret"": null,
                    ""InfraClientId"": ""infra-client-id"",
                    ""InfraClientSecret"": ""infra-client-secret""
                },
                ""DatahubServiceBus"": {
                    ""ConnectionString"": ""service-bus-connection-string""
                }
            }";

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonStream(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonConfig)));
            _configuration = configurationBuilder.Build();

            // Act
            Action act = () => _services.AddDatahubConfigurationFromFunctionFormat(_configuration);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*FUNC_SP_CLIENT_SECRET*");
        }

        [Test]
        public void AddDatahubConfigurationFromFunctionFormat_ShouldThrowArgumentNullException_WhenInfraClientIdIsNull()
        {
            // Arrange
            var jsonConfig = @"
            {
                ""AzureAd"": {
                    ""TenantId"": ""tenant-id"",
                    ""ClientId"": ""client-id"",
                    ""ClientSecret"": ""client-secret"",
                    ""InfraClientId"": null,
                    ""InfraClientSecret"": ""infra-client-secret""
                },
                ""DatahubServiceBus"": {
                    ""ConnectionString"": ""service-bus-connection-string""
                }
            }";

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonStream(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonConfig)));
            _configuration = configurationBuilder.Build();

            // Act
            Action act = () => _services.AddDatahubConfigurationFromFunctionFormat(_configuration);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*AzureDevOpsConfiguration:ClientId*");
        }

        [Test]
        public void AddDatahubConfigurationFromFunctionFormat_ShouldThrowArgumentNullException_WhenInfraClientSecretIsNull()
        {
            // Arrange
            var jsonConfig = @"
            {
                ""AzureAd"": {
                    ""TenantId"": ""tenant-id"",
                    ""ClientId"": ""client-id"",
                    ""ClientSecret"": ""client-secret"",
                    ""InfraClientId"": ""infra-client-id"",
                    ""InfraClientSecret"": null
                },
                ""DatahubServiceBus"": {
                    ""ConnectionString"": ""service-bus-connection-string""
                }
            }";

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonStream(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonConfig)));
            _configuration = configurationBuilder.Build();

            // Act
            Action act = () => _services.AddDatahubConfigurationFromFunctionFormat(_configuration);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*AzureDevOpsConfiguration:ClientSecret*");
        }

        [Test]
        public void AddDatahubConfigurationFromFunctionFormat_ShouldAddConfigurationToServices_WhenAllValuesAreProvided()
        {
            // Arrange
            var jsonConfig = @"
            {
                ""AzureAd"": {
                    ""TenantId"": ""tenant-id"",
                    ""ClientId"": ""client-id"",
                    ""ClientSecret"": ""client-secret"",
                    ""InfraClientId"": ""infra-client-id"",
                    ""InfraClientSecret"": ""infra-client-secret""
                },
                ""DatahubServiceBus"": {
                    ""ConnectionString"": ""service-bus-connection-string""
                }
            }";

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonStream(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonConfig)));
            _configuration = configurationBuilder.Build();

            // Act
            _services.AddDatahubConfigurationFromFunctionFormat(_configuration);
            var serviceProvider = _services.BuildServiceProvider();
            var config = serviceProvider.GetService<DatahubPortalConfiguration>();

            // Assert
            config.Should().NotBeNull();
            config.AzureAd.TenantId.Should().Be("tenant-id");
            config.AzureAd.ClientId.Should().Be("client-id");
            config.AzureAd.ClientSecret.Should().Be("client-secret");
            config.AzureAd.InfraClientId.Should().Be("infra-client-id");
            config.AzureAd.InfraClientSecret.Should().Be("infra-client-secret");
            config.DatahubServiceBus.ConnectionString.Should().Be("service-bus-connection-string");
        }
    }
}
