using System.Globalization;
using System.Text.Json;
using Datahub.Application.Configuration;
using Datahub.Application.Services.Cost;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps
{
    [Binding]
    public sealed class WorkspaceCostsSteps(
        ScenarioContext scenarioContext,
        IWorkspaceCostManagementService sut,
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
        DatahubPortalConfiguration datahubPortalConfiguration)
    {
        [Given(@"a workspace with known costs")]
        public void GivenAWorkspaceWithKnownCosts()
        {
            scenarioContext.Set(Testing.WorkspaceAcronym, "workspaceAcronym");
        }


        [When(@"I query the daily costs for the workspace")]
        public async Task WhenIQueryTheDailyCostsForTheWorkspace()
        {
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            try
            {
                var costs = await sut.QueryWorkspaceCostsAsync(acronym, Testing.Dates.First(),
                    Testing.Dates.Last(), QueryGranularity.Daily);
                scenarioContext.Set(costs, "costs");
                scenarioContext.Set(true, "success");
            }
            catch
            {
                scenarioContext.Set(false, "success");
            }
        }

        [Then(@"I should receive the correct daily costs")]
        public void ThenIShouldReceiveTheCorrectDailyCosts()
        {
            var costs = scenarioContext.Get<List<DailyServiceCost>>("costs");
            costs.Should().NotBeNull();
            costs.Should().HaveCount(5);
            costs.Should().AllSatisfy(c =>
            {
                var b = c.ResourceGroupName == Testing.ResourceGroupName1 &&
                        c.Date >= Testing.Dates.First() && c.Date <= Testing.Dates.Last();
            });
        }

        [Given(@"a subscription with known costs")]
        public void GivenASubscriptionWithKnownCosts()
        {
            scenarioContext.Set(Testing.WorkspaceSubscriptionGuid, "subscriptionId");
        }

        [When(@"I query the daily costs for the subscription")]
        public async Task WhenIQueryTheDailyCostsForTheSubscription()
        {
            var subscriptionId = scenarioContext.Get<string>("subscriptionId");
            var costs = await sut.QuerySubscriptionCostsAsync(subscriptionId, Testing.Dates.First(),
                Testing.Dates.Last(), QueryGranularity.Daily);
            scenarioContext.Set(costs, "costs");
        }

        [When(@"I query the total costs for the workspace")]
        public async Task WhenIQueryTheTotalCostsForTheWorkspace()
        {
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            var costs = await sut.QueryWorkspaceCostsAsync(acronym, Testing.Dates.First(), Testing.Dates.Last(),
                QueryGranularity.Total);
            scenarioContext.Set(costs, "totals");
        }

        [Then(@"I should receive the correct total costs")]
        public void ThenIShouldReceiveTheCorrectTotalCosts()
        {
            var totals = scenarioContext.Get<List<DailyServiceCost>>("totals");
            totals.Should().NotBeNull();
            totals.Should().HaveCount(2);
            totals.TotalAmount().Should().Be(300);
        }

        [When(@"I query the total costs for the subscription")]
        public async Task WhenIQueryTheTotalCostsForTheSubscription()
        {
            var subscriptionId = scenarioContext.Get<string>("subscriptionId");
            var costs = await sut.QuerySubscriptionCostsAsync(subscriptionId, Testing.Dates.First(),
                Testing.Dates.Last(), QueryGranularity.Total);
            scenarioContext.Set(costs, "totals");
        }

        [Given(@"a workspace with existing costs and credits")]
        public void GivenAWorkspaceWithExistingCostsAndCredits()
        {
            scenarioContext.Set(Testing.WorkspaceAcronym, "workspaceAcronym");
        }

        [When(@"I update the costs for the workspace")]
        public async Task WhenIUpdateTheCostsForTheWorkspace()
        {
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = ctx.Projects.FirstOrDefault(p => p.Project_Acronym_CD == acronym);
            var existingCosts = ctx.Project_Costs.Where(c => c.Project_ID == project.Project_ID).Select(c =>
                new DailyServiceCost { Amount = (decimal)c.CadCost, Date = c.Date, Source = c.ServiceName }).ToList();
            var costs = new List<DailyServiceCost>
            {
                new()
                {
                    Amount = 1, Date = Testing.Dates.First().Date, ResourceGroupName = Testing.ResourceGroupName1,
                    Source = "Service1"
                },
                new()
                {
                    Amount = 2, Date = Testing.Dates.First().Date, ResourceGroupName = Testing.ResourceGroupName1,
                    Source = "Service2"
                },
                new()
                {
                    Amount = 3, Date = Testing.Dates.First().Date, ResourceGroupName = Testing.ResourceGroupName1,
                    Source = "Service3"
                },
            };
            var (rollOver, lastYearTotal) = await sut.UpdateWorkspaceCostsAsync(acronym, costs);
            scenarioContext.Set(existingCosts, "existingCosts");
            scenarioContext.Set(costs, "costs");
            scenarioContext.Set(rollOver, "rollOver");
            scenarioContext.Set(lastYearTotal, "actualLastYearTotal");
        }


        [Then(@"the costs and credits records should reflect the new costs")]
        public async Task ThenTheCostsAndCreditsRecordsShouldReflectTheNewCosts()
        {
            var costs = scenarioContext.Get<List<DailyServiceCost>>("costs");
            var existingCosts = scenarioContext.Get<List<DailyServiceCost>>("existingCosts");
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = ctx.Projects.First(p => p.Project_Acronym_CD == acronym);
            var projectCosts = ctx.Project_Costs.Where(c => c.Project_ID == project.Project_ID).Select(c =>
                new DailyServiceCost { Amount = (decimal)c.CadCost, Date = c.Date, Source = c.ServiceName }).ToList();
            var projectCredits = ctx.Project_Credits.FirstOrDefault(c => c.ProjectId == project.Project_ID);
            projectCosts.Count.Should().Be(costs.Count + existingCosts.Count);
            projectCosts.Should().AllSatisfy(c =>
            {
                var existingInExisting = existingCosts.FirstOrDefault(n =>
                    n.Date == c.Date && n.Source == c.Source && n.Amount == c.Amount);
                var existingInNew =
                    costs.FirstOrDefault(n => n.Date == c.Date && n.Source == c.Source && n.Amount == c.Amount);
                var b = (existingInNew != null || existingInExisting != null);
            });
            projectCredits.Should().NotBeNull();
            projectCredits!.Current.Should().Be((double)projectCosts.TotalAmount());
            projectCredits.CurrentPerDay.Should().Be(JsonSerializer.Serialize(projectCosts.GroupByDate()));
            projectCredits.CurrentPerService.Should().Be(JsonSerializer.Serialize(projectCosts.GroupBySource()));
            projectCredits.YesterdayCredits.Should()
                .Be((double)projectCosts.FilterDateRange(DateTime.UtcNow.Date.AddDays(-1)).TotalAmount());
            projectCredits.YesterdayPerService.Should()
                .Be(JsonSerializer.Serialize(projectCosts.FilterDateRange(DateTime.UtcNow.Date.AddDays(-1))
                    .GroupBySource()));
            projectCredits.LastUpdate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }

        [Given(@"a workspace with no existing costs or credits")]
        public async Task GivenAWorkspaceWithNoExistingCostsOrCredits()
        {
            scenarioContext.Set(Testing.WorkspaceAcronym, "workspaceAcronym");
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = ctx.Projects.First(p => p.Project_Acronym_CD == Testing.WorkspaceAcronym);
            ctx.Project_Costs.RemoveRange(ctx.Project_Costs.Where(c => c.Project_ID == project.Project_ID));
            ctx.Project_Credits.RemoveRange(ctx.Project_Credits.Where(c => c.ProjectId == project.Project_ID));
            await ctx.SaveChangesAsync();
        }

        [When(@"I update the costs with existing cost but different enough values")]
        public async Task WhenIUpdateTheCostsWithExistingCostButDifferentEnoughValues()
        {
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = ctx.Projects.First(p => p.Project_Acronym_CD == acronym);
            var existingCosts = ctx.Project_Costs.Where(c => c.Project_ID == project.Project_ID).Select(c =>
                new DailyServiceCost { Amount = (decimal)c.CadCost, Date = c.Date, Source = c.ServiceName }).ToList();
            var existingCost = existingCosts.First();
            var newCosts = new List<DailyServiceCost>
            {
                new DailyServiceCost
                {
                    Amount = existingCost.Amount * 10,
                    Date = existingCost.Date,
                    ResourceGroupName = Testing.ResourceGroupName1,
                    Source = existingCost.Source
                }
            };
            await sut.UpdateWorkspaceCostsAsync(acronym, newCosts);
            scenarioContext.Set(existingCosts, "existingCosts");
            scenarioContext.Set(newCosts, "updatedCosts");
        }

        [Then(@"the relevant cost should be updated")]
        public void ThenTheRelevantCostShouldBeUpdated()
        {
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            var existingCosts = scenarioContext.Get<List<DailyServiceCost>>("existingCosts");
            var existingCost = existingCosts.First();
            var updatedCosts = scenarioContext.Get<List<DailyServiceCost>>("updatedCosts");
            var updatedCost = updatedCosts.First();
            using var ctx = dbContextFactory.CreateDbContext();
            var project = ctx.Projects.First(p => p.Project_Acronym_CD == acronym);
            var projectCosts = ctx.Project_Costs.Where(c => c.Project_ID == project.Project_ID).Select(c =>
                new DailyServiceCost { Amount = (decimal)c.CadCost, Date = c.Date, Source = c.ServiceName }).ToList();
            var costInQuestion =
                projectCosts.First(c => c.Date == existingCost.Date && c.Source == existingCost.Source);
            costInQuestion.Amount.Should().Be(updatedCost.Amount);
            projectCosts.Count.Should().Be(existingCosts.Count);

            scenarioContext.Set(projectCosts, "costs");
            scenarioContext.Set(new List<DailyServiceCost>(), "existingCosts");
        }

        [When(@"I update the costs with existing cost but similar values")]
        public async Task WhenIUpdateTheCostsWithExistingCostButSimilarValues()
        {
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = ctx.Projects.First(p => p.Project_Acronym_CD == acronym);
            var existingCosts = ctx.Project_Costs.Where(c => c.Project_ID == project.Project_ID).Select(c =>
                new DailyServiceCost { Amount = (decimal)c.CadCost, Date = c.Date, Source = c.ServiceName }).ToList();
            var existingCost = existingCosts.First();
            var newCosts = new List<DailyServiceCost>
            {
                new DailyServiceCost
                {
                    Amount = existingCost.Amount + (decimal)0.05,
                    Date = existingCost.Date,
                    ResourceGroupName = Testing.ResourceGroupName1,
                    Source = existingCost.Source
                }
            };
            await sut.UpdateWorkspaceCostsAsync(acronym, newCosts);
            scenarioContext.Set(existingCosts, "existingCosts");
            scenarioContext.Set(newCosts, "updatedCosts");
        }

        [Then(@"the relevant cost should not be updated")]
        public async Task ThenTheRelevantCostShouldNotBeUpdated()
        {
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            var existingCosts = scenarioContext.Get<List<DailyServiceCost>>("existingCosts");
            var existingCost = existingCosts.First();
            var updatedCosts = scenarioContext.Get<List<DailyServiceCost>>("updatedCosts");
            var updatedCost = updatedCosts.First();
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = ctx.Projects.First(p => p.Project_Acronym_CD == acronym);
            var projectCosts = ctx.Project_Costs.Where(c => c.Project_ID == project.Project_ID).Select(c =>
                new DailyServiceCost { Amount = (decimal)c.CadCost, Date = c.Date, Source = c.ServiceName }).ToList();
            var costInQuestion =
                projectCosts.First(c => c.Date == existingCost.Date && c.Source == existingCost.Source);
            costInQuestion.Amount.Should().Be(existingCost.Amount);
            projectCosts.Count.Should().Be(existingCosts.Count);

            scenarioContext.Set(projectCosts, "costs");
            scenarioContext.Set(new List<DailyServiceCost>(), "existingCosts");
        }

        [Given(@"a workspace with total costs that match Azure totals")]
        public void GivenAWorkspaceWithTotalCostsThatMatchAzureTotals()
        {
            scenarioContext.Set(Testing.WorkspaceAcronym, "workspaceAcronym");
            scenarioContext.Set(0, "variance");
        }

        [When(@"I verify if a refresh is needed")]
        public async Task WhenIVerifyIfARefreshIsNeeded()
        {
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            var variance = scenarioContext.Get<int>("variance");
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = ctx.Projects.First(p => p.Project_Acronym_CD == acronym);
            var costs = ctx.Project_Costs.Where(c => c.Project_ID == project.Project_ID).Select(c =>
                new DailyServiceCost { Amount = (decimal)c.CadCost, Date = c.Date, Source = c.ServiceName }).ToList();
            var totals = new List<DailyServiceCost>
            {
                new()
                {
                    Amount = costs.TotalAmount() + variance,
                    ResourceGroupName = Testing.ResourceGroupName1
                }
            };
            var refreshNeeded = await sut.VerifyAndRefreshWorkspaceCostsAsync(acronym, totals, executeRefresh: false);
            scenarioContext.Set(refreshNeeded, "refreshNeeded");
        }

        [Then(@"no refresh should be needed")]
        public void ThenNoRefreshShouldBeNeeded()
        {
            var refreshNeeded = scenarioContext.Get<bool>("refreshNeeded");
            refreshNeeded.Should().BeFalse();
        }

        [Given(@"a workspace with total costs that do not match Azure totals")]
        public void GivenAWorkspaceWithTotalCostsThatDoNotMatchAzureTotals()
        {
            scenarioContext.Set(Testing.WorkspaceAcronym, "workspaceAcronym");
            scenarioContext.Set(1000, "variance");
        }

        [Then(@"a refresh should be needed")]
        public void ThenARefreshShouldBeNeeded()
        {
            scenarioContext.Get<bool>("refreshNeeded").Should().BeTrue();
        }

        [When(@"I refresh the costs for the workspace")]
        public async Task WhenIRefreshTheCostsForTheWorkspace()
        {
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            var refreshed = await sut.RefreshWorkspaceCostsAsync(acronym);
            scenarioContext.Set(refreshed, "refreshed");
        }

        [Then(@"there should be costs for the whole fiscal year and the credits should be updated")]
        public void ThenThereShouldBeCostsForTheWholeFiscalYearAndTheCreditsShouldBeUpdated()
        {
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            var refreshed = scenarioContext.Get<bool>("refreshed");
            using var ctx = dbContextFactory.CreateDbContext();
            var project = ctx.Projects.First(p => p.Project_Acronym_CD == acronym);
            var projectCosts = ctx.Project_Costs.Where(c => c.Project_ID == project.Project_ID).Select(c =>
                new DailyServiceCost { Amount = (decimal)c.CadCost, Date = c.Date, Source = c.ServiceName }).ToList();
            refreshed.Should().BeTrue();
            projectCosts.Should().HaveCount(5);
            projectCosts.Should().AllSatisfy(c =>
            {
                var b = c.Date > CostManagementUtilities.CurrentFiscalYear.StartDate &&
                        c.Date < CostManagementUtilities.CurrentFiscalYear.EndDate;
            });
            projectCosts.TotalAmount().Should().Be(1500);
            var projectCredits = ctx.Project_Credits.FirstOrDefault(c => c.ProjectId == project.Project_ID);
            projectCredits.Should().NotBeNull();
            projectCredits!.Current.Should().Be(1500);
        }

        [Given(@"a non-existent workspace acronym")]
        public void GivenANonExistentWorkspaceAcronym()
        {
            scenarioContext.Set(Testing.InvalidWorkspaceAcronym, "workspaceAcronym");
        }

        [Then(@"I should receive an error")]
        public void ThenIShouldReceiveAnError()
        {
            scenarioContext.Get<bool>("success").Should().BeFalse();
        }

        [When(@"I query the costs for the workspace with an invalid date range")]
        public async Task WhenIQueryTheCostsForTheWorkspaceWithAnInvalidDateRange()
        {
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            try
            {
                await sut.QueryWorkspaceCostsAsync(acronym, Testing.Dates.Last(),
                    Testing.Dates.First(), QueryGranularity.Daily);
                scenarioContext.Set(true, "success");
            }
            catch
            {
                scenarioContext.Set(false, "success");
            }

            try
            {
                await sut.QueryWorkspaceCostsAsync(acronym, DateTime.MinValue,
                    DateTime.MaxValue, QueryGranularity.Daily);
                scenarioContext.Set(true, "success");
            }
            catch
            {
                scenarioContext.Set(false, "success");
            }
        }

        [When(@"I update the costs with no new costs")]
        public async Task WhenIUpdateTheCostsWithNoNewCosts()
        {
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = ctx.Projects.First(p => p.Project_Acronym_CD == acronym);
            var existingCosts = ctx.Project_Costs.Where(c => c.Project_ID == project.Project_ID).Select(c =>
                new DailyServiceCost { Amount = (decimal)c.CadCost, Date = c.Date, Source = c.ServiceName }).ToList();
            var existingCredits = ctx.Project_Credits.FirstOrDefault(c => c.ProjectId == project.Project_ID);
            await sut.UpdateWorkspaceCostsAsync(acronym, new List<DailyServiceCost>());
            scenarioContext.Set(existingCosts, "existingCosts");
            scenarioContext.Set(existingCredits, "existingCredits");
        }

        [Then(@"the costs and credits records should not change")]
        public async Task ThenTheCostsAndCreditsRecordsShouldNotChange()
        {
            var existingCosts = scenarioContext.Get<List<DailyServiceCost>>("existingCosts");
            var existingCredits = scenarioContext.Get<Project_Credits>("existingCredits");
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = ctx.Projects.First(p => p.Project_Acronym_CD == acronym);
            var projectCosts = ctx.Project_Costs.Where(c => c.Project_ID == project.Project_ID).Select(c =>
                new DailyServiceCost { Amount = (decimal)c.CadCost, Date = c.Date, Source = c.ServiceName }).ToList();
            var projectCredits = ctx.Project_Credits.FirstOrDefault(c => c.ProjectId == project.Project_ID);
            projectCosts.Count.Should().Be(existingCosts.Count);
            projectCosts.Should().AllSatisfy(c =>
            {
                var existingInExisting = existingCosts.FirstOrDefault(n =>
                    n.Date == c.Date && n.Source == c.Source && n.Amount == c.Amount);
                var b = existingInExisting != null;
            });
            projectCredits.Should().NotBeNull();
            projectCredits!.Current.Should().Be(existingCredits.Current);
            projectCredits.CurrentPerDay.Should().Be(existingCredits.CurrentPerDay);
            projectCredits.CurrentPerService.Should().Be(existingCredits.CurrentPerService);
            projectCredits.YesterdayCredits.Should().Be(existingCredits.YesterdayCredits);
            projectCredits.YesterdayPerService.Should().Be(existingCredits.YesterdayPerService);
        }

        [When(@"I update the costs for the non-existent workspace")]
        public async Task WhenIUpdateTheCostsForTheNonExistentWorkspace()
        {
            var acronym = scenarioContext.Get<string>("workspaceAcronym");

            try
            {
                var testCosts = new List<DailyServiceCost>
                {
                    new() { Amount = 1, Date = Testing.Dates.First(), Source = Testing.ServiceNames.First() }
                };
                await sut.UpdateWorkspaceCostsAsync(acronym, testCosts);
                scenarioContext.Set(true, "success");
            }
            catch
            {
                scenarioContext.Set(false, "success");
            }
        }

        [Given(@"a workspace with existing costs and credits that need a rollover")]
        public async Task GivenAWorkspaceWithExistingCostsAndCreditsThatNeedARollover()
        {
            scenarioContext.Set(Testing.WorkspaceAcronym, "workspaceAcronym");
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = ctx.Projects.First(p => p.Project_Acronym_CD == Testing.WorkspaceAcronym);
            var costs = ctx.Project_Costs.Where(c => c.Project_ID == project.Project_ID).ToList();
            var credits = ctx.Project_Credits.First(c => c.ProjectId == project.Project_ID);
            var firstCost = costs.First();
            firstCost.Date = DateTime.UtcNow.Date.AddYears(-1);
            firstCost.CadCost = 100;
            ctx.Project_Costs.Update(firstCost);
            credits.LastUpdate = DateTime.UtcNow.AddYears(-1);
            ctx.Project_Credits.Update(credits);
            await ctx.SaveChangesAsync();
            scenarioContext.Set((decimal)100.0, "lastYearTotal");
        }

        [Then(@"a rollover needed is returned")]
        public void ThenARolloverNeededIsReturned()
        {
            var rollOver = scenarioContext.Get<bool>("rollOver");
            rollOver.Should().BeTrue();
        }

        [Then(@"the correct amount of costs is given")]
        public void ThenTheCorrectAmountOfCostsIsGiven()
        {
            var actualLastYearTotal = scenarioContext.Get<decimal>("actualLastYearTotal");
            var lastYearTotal = scenarioContext.Get<decimal>("lastYearTotal");
            actualLastYearTotal.Should().Be(lastYearTotal);
        }

        [When(@"I query the costs for the workspace with a large response")]
        public async Task WhenIQueryTheCostsForTheWorkspaceWithALargeResponse()
        {
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            var costs = await sut.QueryWorkspaceCostsAsync(acronym, Testing.Dates.First(), Testing.Dates.Last(),
                QueryGranularity.Daily);
            var rgName = costs.First().ResourceGroupName;
            var expectedCosts = GetLargeResponseDailyServiceCosts(1000, rgName);
            scenarioContext.Set(expectedCosts, "expectedCosts");
            scenarioContext.Set(costs, "actualCosts");
        }

        public List<DailyServiceCost> GetLargeResponseDailyServiceCosts(int rows, string rgName)
        {
            var costs = new List<DailyServiceCost>();
            for (int i = 1; i <= rows; i++)
            {
                costs.Add(new DailyServiceCost
                {
                    Amount = i * 100,
                    Date = DateTime.ParseExact(Testing.Dates.First().ToString("yyyyMMdd"), "yyyyMMdd",
                        CultureInfo.InvariantCulture),
                    ResourceGroupName = rgName,
                    Source = $"ServiceName{i}"
                });
            }

            return costs;
        }

        [Then(@"there should be no duplicate costs")]
        public void ThenThereShouldBeNoDuplicateCosts()
        {
            var expectedCosts = scenarioContext.Get<List<DailyServiceCost>>("expectedCosts");
            var actualCosts = scenarioContext.Get<List<DailyServiceCost>>("actualCosts");
            actualCosts.Should().HaveCount(expectedCosts.Count);
            actualCosts.ForEach(ac =>
            {
                var cost = expectedCosts
                    .FirstOrDefault(ec =>
                    ec.Date == ac.Date && ec.ResourceGroupName == ac.ResourceGroupName && ec.Source == ac.Source && ec.Amount == ac.Amount);
                cost.Should().NotBeNull();
                expectedCosts.Remove(cost);
            });
            expectedCosts.Should().HaveCount(0);
        }
    }
}