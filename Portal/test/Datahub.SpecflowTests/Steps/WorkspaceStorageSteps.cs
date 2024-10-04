using Datahub.Application.Services.Storage;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps
{
    [Binding]
    public class WorkspaceStorageSteps(
        ScenarioContext scenarioContext,
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
        IWorkspaceStorageManagementService sut)
    {
        [Given(@"a workspace with a storage account")]
        public void GivenAWorkspaceWithAStorageAccount()
        {
            scenarioContext.Set(Testing.WorkspaceAcronym, "workspaceAcronym");
        }

        [Given(@"the storage account has a capacity of above (.*) bytes and below (.*) bytes")]
        public void GivenTheStorageAccountHasACapacityOfAboveBytesAndBelowBytes(int p0, int p1)
        {
            scenarioContext.Set(p0, "minCapacity");
            scenarioContext.Set(p1, "maxCapacity");
        }

        [When(@"the storage capacity is requested")]
        public async Task WhenTheStorageCapacityIsRequested()
        {
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            try
            {
                var capacity = await sut.GetStorageCapacity(acronym);
                scenarioContext.Set(capacity, "capacity");
                scenarioContext.Set(true, "success");
            }
            catch
            {
                scenarioContext.Set(false, "success");
            }
        }

        [Then(@"the result should be as expected")]
        public void ThenTheResultShouldBeAsExpected()
        {
            var capacity = scenarioContext.Get<double>("capacity");
            var minCapacity = scenarioContext.Get<int>("minCapacity");
            var maxCapacity = scenarioContext.Get<int>("maxCapacity");

            capacity.Should().BeGreaterOrEqualTo(minCapacity);
            capacity.Should().BeLessOrEqualTo(maxCapacity);
        }

        [When(@"the workspace's storage capacity is updated")]
        public async Task WhenTheWorkspacesStorageCapacityIsUpdated()
        {
            var acronym = scenarioContext.Get<string>("workspaceAcronym");

            try
            {
                await sut.UpdateStorageCapacity(acronym);
                scenarioContext.Set(true, "success");
            }
            catch
            {
                scenarioContext.Set(false, "success");
            }
        }

        [Then(@"the database should contain the updated capacity")]
        public async Task ThenTheDatabaseShouldContainTheUpdatedCapacity()
        {
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            var minCapacity = scenarioContext.Get<int>("minCapacity");
            var maxCapacity = scenarioContext.Get<int>("maxCapacity");
            var hasCount = scenarioContext.TryGetValue("count", out int count);
            var expectedCount = hasCount ? count : 2;
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = await ctx.Projects.FirstAsync(p => p.Project_Acronym_CD == acronym);
            var storage = ctx.Project_Storage_Avgs.Where(s => s.ProjectId == project.Project_ID).ToList();
            storage.Should().NotBeEmpty();
            storage.Should().HaveCount(expectedCount);
            var orderedStorage = storage.OrderBy(s => s.Date).ToList();
            var newestStorage = orderedStorage.Last();
            newestStorage.AverageCapacity.Should().BeGreaterOrEqualTo(minCapacity);
            newestStorage.AverageCapacity.Should().BeLessOrEqualTo(maxCapacity);
        }

        [Given(@"a workspace that does not exist")]
        public void GivenAWorkspaceThatDoesNotExist()
        {
            scenarioContext.Set(Testing.InvalidWorkspaceAcronym, "workspaceAcronym");
        }

        [Then(@"an error should be returned")]
        public void ThenAnErrorShouldBeReturned()
        {
            var success = scenarioContext.Get<bool>("success");
            success.Should().BeFalse();
        }

        [Given(@"a new workspace")]
        public async Task GivenANewWorkspace()
        {
            scenarioContext.Set(Testing.WorkspaceAcronym, "workspaceAcronym");
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = ctx.Projects.First(p => p.Project_Acronym_CD == Testing.WorkspaceAcronym);
            ctx.Project_Storage_Avgs.RemoveRange(
                ctx.Project_Storage_Avgs.Where(s => s.ProjectId == project.Project_ID));
            await ctx.SaveChangesAsync();
            scenarioContext.Set(1, "count");
        }

        [Given(@"a workspace with no storage account")]
        public void GivenAWorkspaceWithNoStorageAccount()
        {
            scenarioContext.Set(Testing.WorkspaceAcronym2, "workspaceAcronym");
            scenarioContext.Set(1, "count");
        }

        [Then(@"the result should be zero")]
        public void ThenTheResultShouldBeZero()
        {
            var capacity = scenarioContext.Get<double>("capacity");
            capacity.Should().Be(0);
        }
    }
}