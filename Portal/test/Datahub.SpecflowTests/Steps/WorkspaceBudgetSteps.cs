using Datahub.Application.Services.Budget;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Infrastructure.Services.Cost;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps
{
    [Binding]
    public class WorkspaceBudgetSteps(ScenarioContext scenarioContext, WorkspaceBudgetManagementService sut, IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
    {
        [Given(@"a workspace with a budget identifier of ""(.*)""")]
        public void GivenAWorkspaceWithABudgetIdentifierOf(string budgetId)
        {
            scenarioContext["budgetId"] = budgetId;
        }

        [Given(@"the budget amount is \$(.*)")]
        public async Task GivenTheBudgetAmountIs(decimal budgetAmount)
        {
            var budgetId = scenarioContext["budgetId"] as string;
            await sut.SetBudgetAmountAsync(budgetId, budgetAmount);
            scenarioContext["expectedBudgetAmount"] = budgetAmount;
        }

        [When(@"the budget amount is queried for the workspace")]
        public async Task WhenTheBudgetAmountIsQueriedForTheWorkspace()
        {
            var budgetId = scenarioContext["budgetId"] as string;
            var budgetAmount = await sut.GetBudgetAmountAsync(budgetId);
            scenarioContext["actualBudgetAmount"] = budgetAmount;
        }

        [Then(@"the result should be the expected amount")]
        public void ThenTheResultShouldBe()
        {
            var expectedAmount = scenarioContext["expectedBudgetAmount"] as decimal? ?? 0;
            var actualAmount = scenarioContext["actualBudgetAmount"] as decimal? ?? 1;
            actualAmount.Should().Be(expectedAmount);
        }

        [When(@"the budget amount is set to \$(.*) for the workspace")]
        public async Task WhenTheBudgetAmountIsSetToForTheWorkspace(decimal newBudgetAmount)
        {
            var budgetId = scenarioContext["budgetId"] as string;
            await sut.SetBudgetAmountAsync(budgetId, newBudgetAmount);
            scenarioContext["expectedBudgetAmount"] = newBudgetAmount;
            var actualAmount = await sut.GetBudgetAmountAsync(budgetId);
            scenarioContext["actualBudgetAmount"] = actualAmount;
        }

        [Given(@"the budget spent is less than \$(.*)")]
        public void GivenTheBudgetSpentIsLessThan(decimal spentAmount)
        {
            scenarioContext["expectedSpentAmount"] = spentAmount;
        }

        [When(@"the budget spent is queried for the workspace")]
        public async Task WhenTheBudgetSpentIsQueriedForTheWorkspace()
        {
            scenarioContext["actualSpentAmount"] = await sut.GetBudgetSpentAsync(scenarioContext["budgetId"] as string);
        }

        [Then(@"the result should be less than the expected amount")]
        public void ThenTheResultShouldBeLessThan()
        {
            var expectedAmount = scenarioContext["expectedSpentAmount"] as decimal? ?? 0;
            var actualAmount = scenarioContext["actualSpentAmount"] as decimal? ?? 1;
            actualAmount.Should().BeLessThan(expectedAmount);
        }

        [Given(@"an existing project credit record")]
        public async Task GivenAnExistingProjectCreditRecord()
        {
            
            var ctx = await dbContextFactory.CreateDbContextAsync();
            var dbSource = scenarioContext["dbSource"] as string;
            var storageSource = scenarioContext["storageSource"] as string;
            
            var credits = new Project_Credits
            {
                ProjectId = 1,
                Current = 10.0,
                LastUpdate = DateTime.UtcNow.Date.AddDays(-1),
                BudgetCurrentSpent = 10.0,
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
            
            await ctx.Projects.AddAsync(project);
            await ctx.Project_Credits.AddAsync(credits);
            await ctx.SaveChangesAsync();
        }

        [When(@"the budget is updated for that workspace")]
        public async Task WhenTheBudgetIsUpdatedForThatWorkspace()
        {
            var budgetId = scenarioContext["budgetId"] as string;
            await sut.UpdateWorkspaceBudgetSpentAsync("TEST", budgetId);
        }

        [Then(@"project credit record should be updated")]
        public async Task ThenProjectCreditRecordShouldBeUpdated()
        {
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var projectCredit = await ctx.Project_Credits.FirstOrDefaultAsync();
            projectCredit.BudgetCurrentSpent.Should().NotBe(10.0);
        }
    }
}