using Azure.Messaging.ServiceBus;
using Datahub.Application.Services.Cost;
using Datahub.Core.Model.Context;
using Datahub.Functions;
using Datahub.Functions.Extensions;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.SpecflowTests.Hooks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps
{
    [Binding]
    public class ProjectUsageUpdaterSteps(
        ScenarioContext scenarioContext,
        ProjectUsageUpdater sut,
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
    {
        [Given(@"a project usage update message for a workspace that doesn't to be rolled over")]
        public void GivenAProjectUsageUpdateMessage()
        {
            var message = new ProjectUsageUpdateMessage(Testing.WorkspaceAcronym, "mock-costs.json", "mock-totals.json",
                false);
            scenarioContext.Set(message, "message");
            scenarioContext.Set(Testing.WorkspaceAcronym, "workspaceAcronym");
        }

        [Given(@"the last update date is in the current fiscal year")]
        public async Task GivenTheLastUpdateDateIsInTheCurrentFiscalYear()
        {
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = await ctx.Projects.FirstAsync(p => p.Project_Acronym_CD == acronym);
            var projectCredits = await ctx.Project_Credits.FirstAsync(p => p.ProjectId == project.Project_ID);
            projectCredits.LastUpdate = DateTime.UtcNow.AddDays(-1);
            ctx.Project_Credits.Update(projectCredits);
            await ctx.SaveChangesAsync();
        }

        [When(@"the project usage is updated")]
        public async Task WhenTheProjectUsageIsUpdated()
        {
            var message = scenarioContext.Get<ProjectUsageUpdateMessage>("message");
            var ct = new CancellationToken();
            try
            {
                var rollOver = await sut.UpdateUsage(message, ct);
                scenarioContext.Set(rollOver, "rollOver");
                scenarioContext.Set(true, "success");
            }
            catch
            {
                scenarioContext.Set(false, "success");
            }
        }

        [Then(@"the rollover should not be triggered")]
        public void ThenTheRolloverShouldNotBeTriggered()
        {
            var rollOver = scenarioContext.Get<bool>("rollOver");
            rollOver.Should().BeFalse();
        }

        [Given(@"the last update date is in the previous fiscal year")]
        public async Task GivenTheLastUpdateDateIsInThePreviousFiscalYear()
        {
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = await ctx.Projects.FirstAsync(p => p.Project_Acronym_CD == acronym);
            var projectCredits = await ctx.Project_Credits.FirstAsync(p => p.ProjectId == project.Project_ID);
            projectCredits.LastUpdate = DateTime.UtcNow.AddYears(-1);
            ctx.Project_Credits.Update(projectCredits);
            await ctx.SaveChangesAsync();
        }

        [Then(@"the rollover should be triggered")]
        public void ThenTheRolloverShouldBeTriggered()
        {
            var rollOver = scenarioContext.Get<bool>("rollOver");
            rollOver.Should().BeTrue();
        }

        [Given(@"a project usage update message for a workspace that needs to be rolled over")]
        public void GivenAProjectUsageUpdateMessageForAWorkspaceThatNeedsToBeRolledOver()
        {
            var message = new ProjectUsageUpdateMessage(Testing.WorkspaceAcronym2, "mock-costs.json",
                "mock-totals.json",
                false);
            scenarioContext.Set(message, "message");
            scenarioContext.Set(Testing.WorkspaceAcronym, "workspaceAcronym");
        }

        [Given(@"an existing file in blob storage")]
        public void GivenAnExistingFileInBlobStorage()
        {
            scenarioContext.Set("mock-costs", "fileName");
        }

        [When(@"the file is downloaded and parsed")]
        public async Task WhenTheFileIsDownloadedAndParsed()
        {
            var fileName = scenarioContext.Get<string>("fileName");
            try
            {
                var totals = await sut.FromBlob(fileName);
                scenarioContext.Set(totals, "totals");
                scenarioContext.Set(true, "success");
            }
            catch
            {
                scenarioContext.Set(false, "success");
            }
        }

        [Then(@"the values should be as expected")]
        public void ThenTheValuesShouldBeAsExpected()
        {
            var totals = scenarioContext.Get<List<DailyServiceCost>>("totals");
            totals.Should().NotBeNull();
            totals.Should().HaveCount(2);
            totals.All(c =>
                c.Date == null && c.Amount != 0 && c.Source == string.Empty &&
                !string.IsNullOrEmpty(c.ResourceGroupName));
        }

        [Given(@"a non-existing file in blob storage")]
        public void GivenANonExistingFileInBlobStorage()
        {
            scenarioContext.Set("invalidfile", "fileName");
        }

        [Then(@"there should be an error")]
        public void ThenThereShouldBeAnError()
        {
            var success = scenarioContext.Get<bool>("success");
            success.Should().BeFalse();
        }
    }
}