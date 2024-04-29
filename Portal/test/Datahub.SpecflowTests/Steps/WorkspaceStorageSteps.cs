using Datahub.Application.Services.Storage;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Datahub.SpecflowTests.Steps
{
    [Binding]
    public class WorkspaceStorageSteps(ScenarioContext scenarioContext, IDbContextFactory<DatahubProjectDBContext> dbContextFactory, IWorkspaceStorageManagementService sut)
    {
        [Given(@"a workspace with a storage account id of ""(.*)""")]
        public void GivenAWorkspaceWithAStorageAccountIdOf(string storageAccountId)
        {
            scenarioContext["storageAccountId"] = storageAccountId;
        }

        [Given(@"the storage account has a capacity of above (.*) bytes and below (.*) bytes")]
        public void GivenTheStorageAccountHasACapacityOfBytes(double lowerCapacity, double upperCapacity)
        {
            scenarioContext["lowerCapacity"] = lowerCapacity;
            scenarioContext["upperCapacity"] = upperCapacity;
        }

        [When(@"the storage capacity is requested")]
        public async Task WhenTheStorageCapacityIsRequested()
        {
            var actualCapacity = await sut.GetStorageCapacity("TEST", new List<string> { scenarioContext["storageAccountId"] as string });
            scenarioContext["actualCapacity"] = actualCapacity;
        }

        [Then(@"the result should be as expected")]
        public void ThenTheResultShouldBeAsExpected()
        {
            var lowerCapacity = scenarioContext["lowerCapacity"] as double? ?? 0;
            var upperCapacity = scenarioContext["upperCapacity"] as double? ?? 1;
            var actualCapacity = scenarioContext["actualCapacity"] as double? ?? 2;
            actualCapacity.Should().BeInRange(lowerCapacity, upperCapacity);
        }

        [When(@"the workspace's storage capacity is updated")]
        public async Task WhenTheWorkspacesStorageCapacityIsUpdated()
        {
            var actualCapacity = await sut.UpdateStorageCapacity("TEST", new List<string> { scenarioContext["storageAccountId"] as string });
            scenarioContext["actualCapacity"] = actualCapacity;
        }

        [Then(@"the database should contain the updated capacity")]
        public async Task ThenTheDatabaseShouldContainTheUpdatedCapacity()
        {
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var storageAvg = ctx.Project_Storage_Avgs.Where(p => p.ProjectId == 1).ToList();
            storageAvg.Any(s => s.Date == DateTime.UtcNow.Date).Should().BeTrue();
            storageAvg.First(s => s.Date == DateTime.UtcNow.Date).AverageCapacity.Should().Be(scenarioContext["actualCapacity"] as double? ?? 0);
        }

        [Given(@"a project exists in the database for the workspace")]
        public async Task GivenAStorageRecordExistsInTheDatabaseForTheWorkspace()
        {
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            
            var project = new Datahub_Project
            {
                Project_Name = "Test Project",
                Project_Acronym_CD = "TEST",
                Project_ID = 1
            };
            
            await ctx.Projects.AddAsync(project);
            await ctx.SaveChangesAsync();
        }
    }
}