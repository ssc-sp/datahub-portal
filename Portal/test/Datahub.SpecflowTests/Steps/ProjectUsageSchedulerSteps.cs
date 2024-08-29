using Azure.Storage.Blobs;
using Datahub.Application.Configuration;
using Datahub.Application.Services.Cost;
using Datahub.Functions;
using FluentAssertions;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps
{
    [Binding]
    public class ProjectUsageSchedulerSteps(
        ScenarioContext scenarioContext,
        ProjectUsageScheduler sut,
        ProjectUsageUpdater updater,
        DatahubPortalConfiguration configuration)
    {
        [Given(@"workspaces that need to be updated")]
        public void GivenWorkspacesThatNeedToBeUpdated()
        {
            scenarioContext.Set(new List<string> { Testing.WorkspaceAcronym, Testing.WorkspaceAcronym2 }, "acronyms");
            scenarioContext.Set(1, "costExpected");
            scenarioContext.Set(1, "storageExpected");
        }

        [When(@"the scheduler runs")]
        public async Task WhenTheSchedulerRuns()
        {
            var acronyms = scenarioContext.Get<List<string>>("acronyms");
            try
            {
                var (costQueued, storageQueued) = await sut.RunScheduler(acronyms);
                scenarioContext.Set(costQueued, "costQueued");
                scenarioContext.Set(storageQueued, "storageQueued");
                scenarioContext.Set(true, "success");
            }
            catch
            {
                scenarioContext.Set(false, "success");
            }
        }

        [Then(@"the correct number of messages get scheduled")]
        public void ThenTheCorrectNumberOfMessagesGetScheduled()
        {
            var costQueued = scenarioContext.Get<int>("costQueued");
            var storageQueued = scenarioContext.Get<int>("storageQueued");
            var costExpected = scenarioContext.Get<int>("costExpected");
            var storageExpected = scenarioContext.Get<int>("storageExpected");
            var success = scenarioContext.Get<bool>("success");

            success.Should().BeTrue();
            costQueued.Should().Be(costExpected);
            storageQueued.Should().Be(storageExpected);
        }

        [Given(@"no workspaces that need to be updated")]
        public void GivenNoWorkspacesThatNeedToBeUpdated()
        {
            scenarioContext.Set(new List<string> { Testing.WorkspaceAcronym2 }, "acronyms");
            scenarioContext.Set(0, "costExpected");
            scenarioContext.Set(0, "storageExpected");
        }

        [Given(@"some costs")]
        public void GivenSomeCosts()
        {
            var costs = new List<DailyServiceCost>
            {
                new()
                {
                    Amount = 10,
                    Date = Testing.Dates.First(),
                    Source = Testing.ServiceNames.First(),
                    ResourceGroupName = Testing.ExistingTestRg
                },
                new()
                {
                    Amount = 20,
                    Date = Testing.Dates.First(),
                    Source = Testing.ServiceNames.Last(),
                    ResourceGroupName = Testing.ExistingTestRg
                }
            };
            scenarioContext.Set(costs, "costs");
        }

        [When(@"the costs are uploaded to blob storage")]
        public async Task WhenTheCostsAreUploadedToBlobStorage()
        {
            sut.mock = false;
            var costs = scenarioContext.Get<List<DailyServiceCost>>("costs");
            var fileName = await sut.UploadToBlob("test", DateTime.UtcNow.ToString("yyyy-mmmm-dd"), new Guid(), costs);
            scenarioContext.Set(fileName, "fileName");
        }

        [Then(@"the costs should be properly uploaded to blob storage")]
        public async Task ThenTheCostsShouldBeProperlyUploadedToBlobStorage()
        {
            var expectedCosts = scenarioContext.Get<List<DailyServiceCost>>("costs");
            var fileName = scenarioContext.Get<string>("fileName");
            var actualCosts = await updater.FromBlob(fileName);
            actualCosts.Should().BeEquivalentTo(expectedCosts);
            await DeleteBlob(configuration.Media.StorageConnectionString, fileName);
        }

        public async Task DeleteBlob(string connectionString, string fileName)
        {
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient("costs");
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}