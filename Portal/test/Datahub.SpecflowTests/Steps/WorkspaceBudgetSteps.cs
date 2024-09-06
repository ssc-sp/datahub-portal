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
    public class WorkspaceBudgetSteps(
        ScenarioContext scenarioContext,
        IWorkspaceBudgetManagementService sut,
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
    {
        [Given(@"a workspace with a budget")]
        public void GivenAWorkspaceWithABudget()
        {
            var budgetId =
                Testing.BudgetId;
            scenarioContext.Set(Testing.WorkspaceAcronym, "workspaceAcronym");
            scenarioContext.Set(new List<string> { budgetId }, "budgetIds");
        }

        [Given(@"the budget amount is \$(.*)")]
        public async Task GivenTheBudgetAmountIs(decimal budgetAmount)
        {
            var budgetIds = scenarioContext.Get<List<string>>("budgetIds");
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            await sut.SetWorkspaceBudgetAmountAsync(acronym, budgetAmount, budgetIds: budgetIds);
            scenarioContext.Set(budgetAmount, "expectedBudgetAmount");
        }

        [When(@"the budget amount is queried for the workspace")]
        public async Task WhenTheBudgetAmountIsQueriedForTheWorkspace()
        {
            var budgetIds = scenarioContext.Get<List<string>>("budgetIds");
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            var budgetAmount = await sut.GetWorkspaceBudgetAmountAsync(acronym, budgetIds: budgetIds);
            scenarioContext.Set(budgetAmount, "actualBudgetAmount");
        }

        [Then(@"the result should be the expected amount")]
        public void ThenTheResultShouldBe()
        {
            scenarioContext.TryGetValue<decimal>("expectedBudgetAmount", out var expectedAmount);
            scenarioContext.TryGetValue<decimal>("actualBudgetAmount", out var actualAmount);
            actualAmount.Should().Be(expectedAmount);
        }

        [When(@"the budget amount is set to \$(.*) for the workspace")]
        public async Task WhenTheBudgetAmountIsSetToForTheWorkspace(decimal newBudgetAmount)
        {
            var budgetIds = scenarioContext.Get<List<string>>("budgetIds");
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            await sut.SetWorkspaceBudgetAmountAsync(acronym, newBudgetAmount, budgetIds: budgetIds);
            scenarioContext.Set(newBudgetAmount, "expectedBudgetAmount");
            var actualAmount = await sut.GetWorkspaceBudgetAmountAsync(acronym, budgetIds: budgetIds);
            scenarioContext.Set(actualAmount, "actualBudgetAmount");
        }

        [Given(@"the budget spent is less than \$(.*)")]
        public void GivenTheBudgetSpentIsLessThan(decimal spentAmount)
        {
            scenarioContext.Set(spentAmount, "expectedSpentAmount");
        }

        [When(@"the budget spent is queried for the workspace")]
        public async Task WhenTheBudgetSpentIsQueriedForTheWorkspace()
        {
            var budgetIds = scenarioContext.Get<List<string>>("budgetIds");
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            var actualSpentAmount = await sut.GetWorkspaceBudgetSpentAsync(acronym, budgetIds: budgetIds);
            scenarioContext.Set(actualSpentAmount, "actualSpentAmount");
        }

        [Then(@"the result should be less than the expected amount")]
        public void ThenTheResultShouldBeLessThan()
        {
            scenarioContext.TryGetValue<decimal>("expectedBudgetAmount", out var expectedAmount);
            scenarioContext.TryGetValue<decimal>("actualBudgetAmount", out var actualAmount);
            actualAmount.Should().BeLessThanOrEqualTo(expectedAmount);
        }

        [Given(@"an existing project credit record")]
        public void GivenAnExistingProjectCreditRecord()
        {
            // The database is already seeded with a project credit record
        }

        [When(@"the budget is updated for that workspace")]
        public async Task WhenTheBudgetIsUpdatedForThatWorkspace()
        {
            var budgetIds = scenarioContext.Get<List<string>>("budgetIds");
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            await sut.UpdateWorkspaceBudgetSpentAsync(acronym, budgetIds);
        }

        [Then(@"project credit record should be updated")]
        public async Task ThenProjectCreditRecordShouldBeUpdated()
        {
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = await ctx.Projects.FirstAsync(p => p.Project_Acronym_CD == acronym);
            var projectCredit = await ctx.Project_Credits.FirstAsync(pc => pc.ProjectId == project.Project_ID);
            projectCredit.BudgetCurrentSpent.Should().NotBe(10.0);
        }

        [Given(@"a non-existent workspace")]
        public void GivenANonExistentWorkspace()
        {
            scenarioContext.Set(Testing.InvalidWorkspaceAcronym, "workspaceAcronym");
        }

        [When(@"the budget amount is queried")]
        public async Task WhenTheBudgetAmountIsQueried()
        {
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            try
            {
                await sut.GetWorkspaceBudgetAmountAsync(acronym);
                scenarioContext.Set(true, "success");
            }
            catch
            {
                scenarioContext.Set(false, "success");
            }
        }

        [Then(@"the result should be an error")]
        public void ThenTheResultShouldBeAnError()
        {
            var success = scenarioContext.Get<bool>("success");
            success.Should().BeFalse();
        }

        [When(@"the budget amount is set")]
        public async Task WhenTheBudgetAmountIsSet()
        {
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            try
            {
                await sut.SetWorkspaceBudgetAmountAsync(acronym, 200);
                scenarioContext.Set(true, "success");
            }
            catch
            {
                scenarioContext.Set(false, "success");
            }
        }

        [When(@"the budget spent is queried")]
        public async Task WhenTheBudgetSpentIsQueried()
        {
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            try
            {
                await sut.GetWorkspaceBudgetSpentAsync(acronym);
                scenarioContext.Set(true, "success");
            }
            catch
            {
                scenarioContext.Set(false, "success");
            }
        }

        [When(@"the budget is updated")]
        public async Task WhenTheBudgetIsUpdated()
        {
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            try
            {
                await sut.UpdateWorkspaceBudgetSpentAsync(acronym);
                scenarioContext.Set(true, "success");
            }
            catch
            {
                scenarioContext.Set(false, "success");
            }
        }
    }
}