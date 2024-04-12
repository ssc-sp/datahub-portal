using System.Text.Json;
using Datahub.Application.Services.Budget;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Datahub.SpecflowTests.Steps
{
    [Binding]
    public sealed class WorkspaceCostsSteps(ScenarioContext _scenarioContext, IWorkspaceCostManagementService _sut, IDbContextFactory<DatahubProjectDBContext> _dbContextFactory)
    {
        [Given(@"a workspace with a subscription cost of at most (.*)")]
        public void GivenAWorkspaceWithASubscriptionCostOfAtMost(int subCosts)
        {
            _scenarioContext["subscriptionCost"] = subCosts;
        }

        [When(@"the subscription cost is mock queried")]
        public async Task WhenTheSubscriptionCostIsMockQueried()
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-1);
            var endDate = DateTime.UtcNow.Date;
            var actualAmount = await _sut.QuerySubscriptionCosts(null, startDate, endDate, mock: true);
            if (actualAmount is null)
            {
                _scenarioContext["actualAmount"] = new List<DailyServiceCost> { };
            }
            else
            {
                _scenarioContext["actualAmount"] = actualAmount;
            }
        }

        [Then(@"the result should not exceed the expected value")]
        public void ThenTheResultShouldBeAtMost()
        {
            var actualAmount = _scenarioContext["actualAmount"] as List<DailyServiceCost>;
            var subscriptionCost = _scenarioContext["subscriptionCost"] as int? ?? 0;
            var sum = actualAmount.Sum(c => c.Amount);
            sum.Should().BeGreaterThan(0);
            sum.Should().BeLessThanOrEqualTo(subscriptionCost);
        }

        [When(@"the subscription cost is mock queried with an invalid date range")]
        public async Task WhenTheSubscriptionCostIsMockQueriedWithAnInvalidDateRange()
        {
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2022, 1, 1);
            try
            {
                await _sut.QuerySubscriptionCosts(null, startDate, endDate);
            }
            catch
            {
                _scenarioContext["exception1Thrown"] = true;
            }

            startDate = new DateTime(2021, 1, 1);
            endDate = new DateTime(2024, 1, 1);
            try
            {
                await _sut.QuerySubscriptionCosts(null, startDate, endDate);
            }
            catch
            {
                _scenarioContext["exception2Thrown"] = true;
            }
        }

        [Then(@"an error should be returned")]
        public void ThenAnErrorShouldBeReturned()
        {
            var exception1Thrown = _scenarioContext["exception1Thrown"] as bool? ?? false;
            var exception2Thrown = _scenarioContext["exception2Thrown"] as bool? ?? false;
            exception1Thrown.Should().BeTrue();
            exception2Thrown.Should().BeTrue();
        }

        [Given(@"a list of mock costs")]
        public void GivenAListOfMockCosts()
        {
            var mockCosts = _scenarioContext["mockCosts"];
            var type = mockCosts is List<DailyServiceCost>;
            type.Should().BeTrue();
        }

        [When(@"the costs are grouped by source")]
        public void WhenTheCostsAreGroupedBySource()
        {
            var mockCosts = _scenarioContext["mockCosts"] as List<DailyServiceCost>;
            var groupedCosts = _sut.GroupBySource(mockCosts);
            _scenarioContext["groupedCosts"] = groupedCosts;
        }

        [Then(@"the result should have the expected count and total of source costs")]
        public void ThenTheResultShouldHaveTheExpectedCountOfSourceCosts()
        {
            var groupedCosts = _scenarioContext["groupedCosts"] as List<DailyServiceCost>;
            var dbSource = _scenarioContext["dbSource"] as string;
            var storageSource = _scenarioContext["storageSource"] as string;

            var dbCosts = groupedCosts.FirstOrDefault(c => c.Source == dbSource);
            var storageCosts = groupedCosts.FirstOrDefault(c => c.Source == storageSource);

            groupedCosts.Should().NotBeNull();
            groupedCosts.Count.Should().Be(2);
            dbCosts.Should().NotBeNull();
            dbCosts.Amount.Should().Be(4);
            storageCosts.Should().NotBeNull();
            storageCosts.Amount.Should().Be(2);
        }

        [When(@"the costs are grouped by date")]
        public void WhenTheCostsAreGroupedByDate()
        {
            var mockCosts = _scenarioContext["mockCosts"] as List<DailyServiceCost>;
            var groupedCosts = _sut.GroupByDate(mockCosts);
            _scenarioContext["groupedCosts"] = groupedCosts;
        }

        [Then(@"the result should have the expected count and total of daily costs")]
        public void ThenTheResultShouldHaveTheExpectedCountOfDailyCosts()
        {
            var groupedCosts = _scenarioContext["groupedCosts"] as List<DailyServiceCost>;
            var date1 = _scenarioContext["date1"] as DateTime? ?? DateTime.MinValue;
            var date2 = _scenarioContext["date2"] as DateTime? ?? DateTime.MinValue;

            var date1Costs = groupedCosts.FirstOrDefault(c => c.Date == date1);
            var date2Costs = groupedCosts.FirstOrDefault(c => c.Date == date2);

            groupedCosts.Should().NotBeNull();
            groupedCosts.Count.Should().Be(2);
            date1Costs.Should().NotBeNull();
            date1Costs.Amount.Should().Be(3);
            date2Costs.Should().NotBeNull();
            date2Costs.Amount.Should().Be(3);
        }

        [When(@"the costs are filtered by the current fiscal year")]
        public void WhenTheCostsAreFilteredByTheCurrentFiscalYear()
        {
            var mockCosts = _scenarioContext["mockCosts"] as List<DailyServiceCost>;
            var filteredCosts = _sut.FilterCurrentFiscalYear(mockCosts);
            _scenarioContext["filteredCosts"] = filteredCosts;
        }

        [Then(@"the result should have the expected count and total of fiscal year costs")]
        public void ThenTheResultShouldHaveTheExpectedCountOfFiscalYearCosts()
        {
            var filteredCosts = _scenarioContext["filteredCosts"] as List<DailyServiceCost>;
            filteredCosts.Should().NotBeNull();
            filteredCosts.Count.Should().Be(2);
        }

        [When(@"the costs are filtered by the last fiscal year")]
        public void WhenTheCostsAreFilteredByTheLastFiscalYear()
        {
            var mockCosts = _scenarioContext["mockCosts"] as List<DailyServiceCost>;
            var filteredCosts = _sut.FilterLastFiscalYear(mockCosts);
            _scenarioContext["filteredCosts"] = filteredCosts;
        }

        [Then(@"the result should have the expected count and total of last fiscal year costs")]
        public void ThenTheResultShouldHaveTheExpectedCountOfLastFiscalYearCosts()
        {
            var filteredCosts = _scenarioContext["filteredCosts"] as List<DailyServiceCost>;
            filteredCosts.Should().NotBeNull();
            filteredCosts.Count.Should().Be(1);
        }

        [When(@"the costs are filtered by a specific date range")]
        public void WhenTheCostsAreFilteredByASpecificDateRange()
        {
            var mockCosts = _scenarioContext["mockCosts"] as List<DailyServiceCost>;
            var date1 = _scenarioContext["date1"] as DateTime? ?? DateTime.MinValue;
            var date2 = _scenarioContext["date2"] as DateTime? ?? DateTime.MinValue;
            var startDate = date1.AddDays(-10);
            var endDate = date1.AddDays(10); 
            var filteredCosts1 = _sut.FilterDateRange(mockCosts, startDate, endDate);
            _scenarioContext["filteredCosts1"] = filteredCosts1;

            var singleDate = date1;
            var filteredCosts2 = _sut.FilterDateRange(mockCosts, singleDate);
            _scenarioContext["filteredCosts2"] = filteredCosts2;
            
            var invalidStartDate = date1.AddDays(10);
            var invalidEndDate = date1.AddDays(20);
            var filteredCosts3 = _sut.FilterDateRange(mockCosts, invalidStartDate, invalidEndDate);
            _scenarioContext["filteredCosts3"] = filteredCosts3;
        }

        [Then(@"the result should have the expected count and total of costs in the date range")]
        public void ThenTheResultShouldHaveTheExpectedCountOfCostsInTheDateRange()
        {
            var filteredCosts1 = _scenarioContext["filteredCosts1"] as List<DailyServiceCost>;
            filteredCosts1.Should().NotBeNull();
            filteredCosts1.Count.Should().Be(2);
            
            var filteredCosts2 = _scenarioContext["filteredCosts2"] as List<DailyServiceCost>;
            filteredCosts2.Should().NotBeNull();
            filteredCosts2.Count.Should().Be(2);
            
            var filteredCosts3 = _scenarioContext["filteredCosts3"] as List<DailyServiceCost>;
            filteredCosts3.Should().NotBeNull();
            filteredCosts3.Count.Should().Be(0);
        }

        [When(@"the costs are filtered by a specific workspace")]
        public async Task WhenTheCostsAreFilteredByASpecificWorkspace()
        {
            var mockCosts = _scenarioContext["mockCosts"] as List<DailyServiceCost>;
            var workspaceAcronym = "test";
            var rgNames = new List<string>
            {
                _scenarioContext["resourceGroup1"] as string
            };
            var filteredCosts = await _sut.FilterWorkspaceCosts(mockCosts, workspaceAcronym, rgNames);
            _scenarioContext["filteredCosts"] = filteredCosts;
        }

        [Then(@"the result should have the expected count and total of costs in the workspace")]
        public void ThenTheResultShouldHaveTheExpectedCountOfCostsInTheWorkspace()
        {
            var filteredCosts = _scenarioContext["filteredCosts"] as List<DailyServiceCost>;
            filteredCosts.Should().NotBeNull();
            filteredCosts.Count.Should().Be(2);
        }

        [Given(@"a workspace, with populated costs and credits")]
        public async Task GivenAWorkspaceWithPopulatedCostsAndCredits()
        {
            var ctx = await _dbContextFactory.CreateDbContextAsync();
            var dbSource = _scenarioContext["dbSource"] as string;
            var storageSource = _scenarioContext["storageSource"] as string;
            
            var credits = new Project_Credits
            {
                ProjectId = 1,
                Current = 10.0,
                LastUpdate = DateTime.UtcNow.Date.AddDays(-1),
                BudgetCurrentSpent = 10.2,
                CurrentPerDay = "",
                CurrentPerService = "",
                YesterdayPerService = "",
                YesterdayCredits = 1.0,
            };
            
            var project = new Datahub_Project
            {
                Project_Name = "Test Project",
                Project_Acronym_CD = "TEST",
                Project_ID = 1
            };

            var costs = new List<Datahub_Project_Costs>
            {
                new()
                {
                    ServiceName = dbSource,
                    Date = DateTime.UtcNow.Date.AddDays(-3),
                    Project_ID = 1,
                    CloudProvider = "azure",
                    CadCost = 1.5
                },
                new()
                {
                    ServiceName = storageSource,
                    Date = DateTime.UtcNow.Date.AddDays(-3),
                    Project_ID = 1,
                    CloudProvider = "azure",
                    CadCost = 2.5
                },
                new()
                {
                    ServiceName = dbSource,
                    Date = DateTime.UtcNow.Date.AddDays(-2),
                    Project_ID = 1,
                    CloudProvider = "azure",
                    CadCost = 3.5
                },
            };

            await ctx.Projects.AddAsync(project);
            await ctx.Project_Credits.AddAsync(credits);
            await ctx.Project_Costs.AddRangeAsync(costs);
            await ctx.SaveChangesAsync();
        }

        [When(@"the costs are updated")]
        public async Task WhenTheCostsAreUpdated()
        {
            var updateMockCosts = _scenarioContext["updateMockCosts"] as List<DailyServiceCost>;
            var workspaceAcronym = "TEST";
            var rgNames = new List<string>
            {
                _scenarioContext["resourceGroup1"] as string
            };
            var (rollover, lastFiscalYearCosts) = await _sut.UpdateWorkspaceCostAsync(updateMockCosts, workspaceAcronym, rgNames);
            _scenarioContext["rollover"] = rollover;
            _scenarioContext["lastFiscalYearCosts"] = lastFiscalYearCosts;
        }

        [Then(@"the costs table and credits table should be updated accordingly and correctly")]
        public async Task ThenTheCostsTableAndCreditsTableShouldBeUpdatedAccordinglyAndCorrectly()
        {
            var rollover = _scenarioContext["rollover"] as bool? ?? false;
            var lastFiscalYearCosts = _scenarioContext["lastFiscalYearCosts"] as decimal? ?? 0;
            var dbSource = _scenarioContext["dbSource"] as string;
            var storageSource = _scenarioContext["storageSource"] as string;
            
            rollover.Should().BeFalse();
            lastFiscalYearCosts.Should().Be(0);
            
            using var ctx = await _dbContextFactory.CreateDbContextAsync();
            var project = await ctx.Projects.FirstOrDefaultAsync(p => p.Project_Acronym_CD == "TEST");
            var credits = await ctx.Project_Credits.FirstOrDefaultAsync(c => c.ProjectId == project.Project_ID);
            var costs = await ctx.Project_Costs.Where(c => c.Project_ID == project.Project_ID).ToListAsync();
            var creditsCurrentPerDay = JsonSerializer.Deserialize<List<DailyServiceCost>>(credits.CurrentPerDay);
            var creditsCurrentPerService = JsonSerializer.Deserialize<List<DailyServiceCost>>(credits.CurrentPerService);
            var creditsYesterdayPerService = JsonSerializer.Deserialize<List<DailyServiceCost>>(credits.YesterdayPerService);
            
            credits.LastUpdate.Should().BeAfter(DateTime.UtcNow.Date.AddDays(-1));
            credits.Current.Should().Be(13.5);
            credits.YesterdayCredits.Should().Be(3.0);
            creditsCurrentPerDay.Count.Should().Be(4);
            creditsCurrentPerDay.Sum(c => c.Amount).Should().Be((decimal)13.5);
            creditsCurrentPerDay.All(c => c.Source == "Day").Should().BeTrue();
            creditsCurrentPerService.Count.Should().Be(2);
            creditsCurrentPerService.Sum(c => c.Amount).Should().Be((decimal)13.5);
            creditsCurrentPerService.All(c => c.Source == dbSource || c.Source == storageSource).Should().BeTrue();
            creditsYesterdayPerService.Count.Should().Be(1);
            creditsYesterdayPerService.Sum(c => c.Amount).Should().Be((decimal)3.0);
            creditsYesterdayPerService.All(c => c.Source == dbSource).Should().BeTrue();
        }
    }
}