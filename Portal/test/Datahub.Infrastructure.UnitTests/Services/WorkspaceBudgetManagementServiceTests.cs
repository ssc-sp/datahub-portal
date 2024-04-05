using Azure.Identity;
using Azure.ResourceManager;
using Datahub.Core.Model.Datahub;
using Datahub.Infrastructure.Services.Cost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Datahub.Infrastructure.UnitTests.Testing;
using NSubstitute;

namespace Datahub.Infrastructure.UnitTests.Services
{
    [TestFixture]
    public class WorkspaceBudgetManagementServiceTests
    {
        private WorkspaceBudgetManagementService _sut;
        private ILogger<WorkspaceBudgetManagementServiceTests> _logger;
        private IDbContextFactory<DatahubProjectDBContext> _dbContextFactory = Substitute.For<IDbContextFactory<DatahubProjectDBContext>>();
        private const decimal INITIAL_BUDGET = 1000;
        
        [SetUp]
        public void SetUp()
        {
            _logger = _loggerFactory.CreateLogger<WorkspaceBudgetManagementServiceTests>();
            var credentials = new ClientSecretCredential(_datahubPortalConfiguration.AzureAd.TenantId, _datahubPortalConfiguration.AzureAd.ClientId, _datahubPortalConfiguration.AzureAd.ClientSecret);
            var armClient = new ArmClient(credentials);
            _sut = new WorkspaceBudgetManagementService(armClient, _loggerFactory.CreateLogger<WorkspaceBudgetManagementService>(), _dbContextFactory);
        }

        [Test]
        public async Task GetBudgetAmountAsync_ShouldReturnRightAmount()
        {
            // Arrange
            var budgetId = TestBudgetId;
            var expectedAmount = INITIAL_BUDGET;
            
            _logger.LogInformation("Expected amount: {0}", expectedAmount);
            // Act
            var actualAmount = await _sut.GetBudgetAmountAsync(budgetId);
            _logger.LogInformation("Budget amount: {0}", actualAmount);
            
            // Assert
            Assert.That(actualAmount, Is.EqualTo(expectedAmount));
        }
        
        [Test]
        public async Task SetBudgetAmountAsync_ShouldSetRightAmount()
        {
            // Arrange
            var budgetId = TestBudgetId;
            var expectedAmount = 500;
            
            // Act
            await _sut.SetBudgetAmountAsync(budgetId, expectedAmount);
            Task.Delay(1000).Wait();
            var actualAmount = await _sut.GetBudgetAmountAsync(budgetId);
            
            // Assert
            Assert.That(actualAmount, Is.EqualTo(expectedAmount));
        }
        
        [Test]
        public async Task GetBudgetSpentAsync_ShouldReturnRightAmount()
        {
            // Arrange
            var budgetId = TestBudgetId;
            
            // Act
            var actualAmount = await _sut.GetBudgetSpentAsync(budgetId);
            
            // Assert
            Assert.That(actualAmount, Is.InRange(0, 5));
        }
        
        [Test]
        public async Task GetBudgetAzureResource_ShouldReturnRightResource()
        {
            // Arrange
            var budgetId = TestBudgetId;
            
            // Act
            var actualResource = await _sut.GetBudgetAzureResource(budgetId);
            
            // Assert
            Assert.That(actualResource, Is.Not.Null);
            Assert.That(actualResource.Id.ToString(), Is.EqualTo(budgetId));
        }

        [OneTimeTearDown]
        public async Task TearDown()
        {
            await _sut.SetBudgetAmountAsync(TestBudgetId, INITIAL_BUDGET);
        }
    }
}