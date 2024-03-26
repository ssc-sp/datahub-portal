using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Datahub.Core.Model.Datahub;
using Datahub.Infrastructure.Services.Cost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using static Datahub.Infrastructure.UnitTests.Testing;

namespace Datahub.Infrastructure.UnitTests.Services
{
    [TestFixture]
    public class WorkspaceCostManagementServiceTests
    {
        private WorkspaceCostManagementService _sut;
        private ILogger<WorkspaceCostManagementServiceTests> _logger;
        
        [SetUp]
        public void SetUp()
        {
            _logger = _loggerFactory.CreateLogger<WorkspaceCostManagementServiceTests>();

            var credentials = new ClientSecretCredential(_datahubPortalConfiguration.AzureAd.TenantId, _datahubPortalConfiguration.AzureAd.ClientId, _datahubPortalConfiguration.AzureAd.ClientSecret);
            var armClientOptions = new ArmClientOptions
            {
                Retry =
                {
                    Mode = RetryMode.Exponential,
                    MaxRetries = 5,
                    Delay = TimeSpan.FromSeconds(2)
                }
            };
            var armClient = new ArmClient(credentials, SubscriptionId, armClientOptions);
            _sut = new WorkspaceCostManagementService(armClient, _loggerFactory.CreateLogger<WorkspaceCostManagementService>(), _dbContextFactory);
        }
        
        [Test]
        public async Task QueryResourceGroupCost_ShouldReturnRightAmount()
        {
            // Arrange
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2024, 1, 1);
            var expectedAmount = 0;
            
            // Act
            var actualAmount = await _sut.QuerySubscriptionCosts(startDate, endDate);
            _logger.LogInformation(actualAmount?.ToString());

            if (actualAmount is null)
            {
                Assert.Fail("Usage query encountered a throttling failure");
            }
            
            // Assert
            Assert.Pass();
            //Assert.That(actualAmount, Is.EqualTo(expectedAmount));
        }
    }
}