using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Datahub.Application.Services.Budget;
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
        private List<DailyServiceCost> _mockCosts;
        
        private readonly DateTime Date1 = new(2024, 1, 1);
        private readonly DateTime Date2 = new(2023, 1, 1);
        private const string StorageSource = "Storage account";
        private const string DbSource = "Database";
        private const string ResourceGroup1 = "test-rg";
        private const string ResourceGroup2 = "test-rg-2";

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
            _mockCosts =
            [

                new DailyServiceCost
                {
                    Amount = 1,
                    Date = Date1,
                    Source = StorageSource,
                    ResourceGroupName = ResourceGroup1
                },


                new DailyServiceCost
                {
                    Amount = 2,
                    Date = Date1,
                    Source = DbSource,
                    ResourceGroupName = ResourceGroup1
                },


                new DailyServiceCost
                {
                    Amount = 3,
                    Date = Date2,
                    Source = DbSource,
                    ResourceGroupName = ResourceGroup2
                }

            ];
        }
        
        [Test]
        public async Task QuerySubscriptionGroupCost_ShouldReturnRightAmount()
        {
            // Arrange
            var startDate = DateTime.UtcNow.Date.AddDays(-1);
            var endDate = DateTime.UtcNow.Date;
            var expectedUpperBound = 100;
            
            // Act
            // This query is mocked by specifying only a couple workspaces instead of all workspaces
            // Hence the low upper bound
            var actualAmount = await _sut.QuerySubscriptionCosts(null, startDate, endDate, mock: true);

            if (actualAmount is null)
            {
                Assert.Fail("Usage query encountered a throttling failure");
            }
            
            var sum = actualAmount!.Sum(c => c.Amount);
            
            // Assert
            Assert.That(sum > 0 && sum < expectedUpperBound);
            Assert.That(actualAmount.All(c => c.Date >= startDate && c.Date <= endDate));
            Assert.Pass($"Actual amount: {sum}");
        }
        
        [Test]
        public async Task QuerySubscriptionCosts_ShouldThrow_GivenInvalidDateRange()
        {
            // Arrange
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2022, 1, 1);
            
            // Act
            try
            {
                await _sut.QuerySubscriptionCosts(null, startDate, endDate);
                Assert.Fail("Expected exception to be thrown");
            }
            catch 
            {
                // ignored
            }

            startDate = new DateTime(2021, 1, 1);
            endDate = new DateTime(2024, 1, 1);
            
            try
            {
                await _sut.QuerySubscriptionCosts(null, startDate, endDate);
                Assert.Fail("Expected exception to be thrown");
            }
            catch 
            {
                // ignored
            }

            // Assert
            Assert.Pass("Exceptions were thrown as expected");
        }
        
        [Test]
        public void GroupBySource_ShouldReturnCorrectCosts()
        {
            // Arrange
            var expectedCostCount = 2;
            
            // Act
            var actualCosts = _sut.GroupBySource(_mockCosts);
            var dbCosts = actualCosts.FirstOrDefault(c => c.Source == DbSource); 
            var storageCosts = actualCosts.FirstOrDefault(c => c.Source == StorageSource);
            
            // Assert
            Assert.That(actualCosts.Count, Is.EqualTo(expectedCostCount));
            Assert.That(dbCosts.Amount, Is.EqualTo(5));
            Assert.That(storageCosts.Amount, Is.EqualTo(1));
        }

        [Test]
        public void GroupByDate_ShouldReturnCorrectCosts()
        {
            // Arrange
            var expectedCostCount = 2;
            
            // Act
            var actualCosts = _sut.GroupByDate(_mockCosts);
            var date1Costs = actualCosts.FirstOrDefault(c => c.Date == Date1);
            var date2Costs = actualCosts.FirstOrDefault(c => c.Date == Date2);
            
            // Assert
            Assert.That(actualCosts.Count, Is.EqualTo(expectedCostCount));
            Assert.That(date1Costs.Amount, Is.EqualTo(3));
            Assert.That(date2Costs.Amount, Is.EqualTo(3));
        }
        
        [Test]
        public void FilterCurrentFiscalYear_ShouldReturnCorrectCosts()
        {
            // Arrange
            var expectedCostCount = 2;
            
            // Act
            var actualCosts = _sut.FilterCurrentFiscalYear(_mockCosts);
            
            // Assert
            Assert.That(actualCosts.Count, Is.EqualTo(expectedCostCount));
        }
        
        [Test]
        public void FilterLastFiscalYear_ShouldReturnCorrectCosts()
        {
            // Arrange
            var expectedCostCount = 1;
            
            // Act
            var actualCosts = _sut.FilterLastFiscalYear(_mockCosts);
            
            // Assert
            Assert.That(actualCosts.Count, Is.EqualTo(expectedCostCount));
        }
        
        [Test]
        public void FilterDateRange_ShouldReturnCorrectCosts()
        {
            // CORRECT DATES
            // Arrange
            var startDate = Date1.AddDays(-10);
            var endDate = Date1.AddDays(10);
            var expectedCostCount = 2;
            
            // Act
            var actualCosts = _sut.FilterDateRange(_mockCosts, startDate, endDate);
            
            // Assert
            Assert.That(actualCosts.Count, Is.EqualTo(expectedCostCount));

            // SINGLE DATE
            // Arrange
            var date = Date1;
            var expectedCostCount2 = 2;
            
            // Act
            var actualCosts2 = _sut.FilterDateRange(_mockCosts, date);
            
            // Assert
            Assert.That(actualCosts2.Count, Is.EqualTo(expectedCostCount2));
            
            // INCORRECT DATES
            // Arrange
            startDate = Date1.AddDays(10);
            endDate = Date1.AddDays(20);
            
            // Act
            var actualCosts3 = _sut.FilterDateRange(_mockCosts, startDate, endDate);
            
            // Assert
            Assert.That(actualCosts3.Count, Is.EqualTo(0));
            
            // LIMITS OF DATE RANGE
            // Arrange
            startDate = Date1;
            endDate = Date1.AddDays(5); 
            var expectedCostCount4 = 2;
            
            // Act
            var actualCosts4 = _sut.FilterDateRange(_mockCosts, startDate, endDate);
            
            // Assert
            Assert.That(actualCosts4.Count, Is.EqualTo(expectedCostCount4));
            
            // Arrange
            startDate = Date1.AddDays(-5);
            endDate = Date1;
            var expectedCostCount5 = 2;
            
            // Act
            var actualCosts5 = _sut.FilterDateRange(_mockCosts, startDate, endDate);
            
            // Assert
            Assert.That(actualCosts5.Count, Is.EqualTo(expectedCostCount5));
        }
        
        [Test]
        public async Task FilterWorkspaceCosts_ShouldReturnCorrectCosts()
        {
            // Arrange
            var expectedCostCount = 2;
            var workspaceAcronym = "test";
            var testRgNames = new List<string> {ResourceGroup1};
            
            // Act
            var actualCosts = await _sut.FilterWorkspaceCosts(_mockCosts, workspaceAcronym, testRgNames);
            
            // Assert
            Assert.That(actualCosts.Count, Is.EqualTo(expectedCostCount));
            Assert.That(actualCosts.All(c => c.ResourceGroupName == ResourceGroup1));
        }
    }
}