using System.Text;
using System.Text.Json;
using Azure.Core.Amqp;
using Azure.Messaging.ServiceBus;
using Datahub.Application.Services.Budget;
using Datahub.Application.Services.Storage;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Functions;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using FluentAssertions;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps
{
    [Binding]
    public class ProjectUsageUpdaterSteps(
        ScenarioContext scenarioContext,
        ILoggerFactory loggerFactory,
        ISendEndpointProvider sendEndpointProvider,
        QueuePongService pongService,
        IWorkspaceCostManagementService costMgmt,
        IWorkspaceBudgetManagementService budgetMgmt,
        IWorkspaceStorageManagementService storageMgmt,
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
    {
        [Given(@"a project usage update message")]
        public void GivenAProjectUsageUpdateMessage()
        {
            var mockCosts = new List<DailyServiceCost>();
            var message = new ProjectUsageUpdateMessage("TEST", mockCosts, false);
            scenarioContext["message"] = message;
            scenarioContext["mockCosts"] = mockCosts;
        }

        [Given(@"an associated project credits record")]
        public async Task GivenAnAssociatedProjectCreditsRecord()
        {
            await using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = new Datahub_Project
            {
                Project_Name = "Test project",
                Project_Acronym_CD = "TEST",
            };
            var credits = new Project_Credits
            {
                Project = project,
                Current = 10.0,
                LastUpdate = DateTime.UtcNow.Date.AddDays(-1),
                BudgetCurrentSpent = 10.2,
                CurrentPerDay = "",
                CurrentPerService = "",
                YesterdayPerService = "",
                YesterdayCredits = 1.0,
            };
            ctx.Projects.Add(project);
            ctx.Project_Credits.Add(credits);
            await ctx.SaveChangesAsync();
        }

        [When(@"the project usage is updated")]
        public async Task WhenTheProjectUsageIsUpdated()
        {
            var message = scenarioContext["message"] as ProjectUsageUpdateMessage;
            var costRollover = scenarioContext["costRollover"] as bool? ?? false;
            var costSpent = scenarioContext["costSpent"] as decimal? ?? 0;
            var budgetSpent = scenarioContext["budgetSpent"] as decimal? ?? 0;
            var strMessage = JsonSerializer.Serialize(message);

            var serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(
                new AmqpAnnotatedMessage(new AmqpMessageBody(new List<ReadOnlyMemory<byte>>
                {
                    Encoding.UTF8.GetBytes(strMessage)
                })), new BinaryData(Encoding.UTF8.GetBytes(strMessage)));

            budgetMgmt.UpdateWorkspaceBudgetSpentAsync(Arg.Any<string>()).Returns(budgetSpent);
            budgetMgmt.GetWorkspaceBudgetAmountAsync(Arg.Any<string>()).Returns((decimal)100.0);
            budgetMgmt.When(c => c.SetWorkspaceBudgetAmountAsync(Arg.Any<string>(), Arg.Any<decimal>(), true)).Do(
                c =>
                {
                    using var ctx = dbContextFactory.CreateDbContext();
                    var credits = ctx.Project_Credits.FirstOrDefault();
                    if (credits != null)
                    {
                        credits.LastRollover = DateTime.UtcNow;
                        ctx.Project_Credits.Update(credits);
                    }

                    ctx.SaveChanges();
                });
            costMgmt.UpdateWorkspaceCostAsync(Arg.Any<List<DailyServiceCost>>(), Arg.Any<string>())
                .Returns((costRollover, (decimal)10.0));
            var projectUsageUpdater = new ProjectUsageUpdater(loggerFactory, pongService, costMgmt, budgetMgmt,
                sendEndpointProvider, storageMgmt);

            var rolledOver = await projectUsageUpdater.Run(serviceBusReceivedMessage, CancellationToken.None);
            scenarioContext["rolledOver"] = rolledOver;
        }

        [Given(@"the last update date is in the current fiscal year")]
        public async Task WhenTheLastUpdateDateIsInTheCurrentFiscalYear()
        {
            scenarioContext["costSpent"] = (decimal)10.0;
            scenarioContext["costRollover"] = false;
            scenarioContext["budgetSpent"] = (decimal)10.0;
        }

        [Then(@"the rollover should not be triggered")]
        public void ThenTheRolloverShouldNotBeTriggered()
        {
            var rolledOver = scenarioContext["rolledOver"] as bool?;
            rolledOver.Should().Be(false);
        }

        [Given(@"the last update date is in the previous fiscal year")]
        public async Task WhenTheLastUpdateDateIsInThePreviousFiscalYear()
        {
            scenarioContext["costSpent"] = (decimal)10.0;
            scenarioContext["costRollover"] = true;
            scenarioContext["budgetSpent"] = (decimal)10.0;
        }

        [Then(@"the rollover should be triggered")]
        public void ThenTheRolloverShouldBeTriggered()
        {
            var rolledOver = scenarioContext["rolledOver"] as bool?;
            rolledOver.Should().Be(true);
        }

        [Then(@"the project credits should be updated accordingly")]
        public async Task ThenTheProjectCreditsShouldBeUpdatedAccordingly()
        {
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var credits = await ctx.Project_Credits.FirstOrDefaultAsync();
            credits.LastRollover.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(60));
        }

        [Given(@"the difference between budget spent and cost captured is too large")]
        public void GivenTheDifferenceBetweenBudgetSpentAndCostCapturedIsTooLarge()
        {
            scenarioContext["costSpent"] = (decimal)10.0;
            scenarioContext["costRollover"] = true;
            scenarioContext["budgetSpent"] = (decimal)20.0;
        }
    }
}