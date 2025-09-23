using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Subscriptions;
using Datahub.Core.Services.Projects;
using Datahub.Infrastructure.Services;
using Datahub.Shared.Configuration;
using Datahub.Shared.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using NSubstitute.Extensions;
using Reqnroll;
using Reqnroll.BoDi;

namespace Datahub.SpecflowTests.Hooks
{
    [Binding]
    public class WorkspaceToolboxHook
    {
        [BeforeScenario("toolbox")]
        public async Task BeforeScenarioRequiringQueue(IObjectContainer objectContainer,
            ScenarioContext scenarioContext)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<Hooks>()
                .AddJsonFile("appsettings.test.json", optional: true)
                .Build();

            var datahubPortalConfiguration = new DatahubPortalConfiguration();
            datahubPortalConfiguration.ToolboxConfig.DisableSubmissionDelays = true;
            datahubPortalConfiguration.ToolboxConfig.DisableSubmissions = true;
            configuration.Bind(datahubPortalConfiguration);

            // setup in memory provider ef core context
            var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dbContextFactory = new SpecFlowDbContextFactory(options);
            var ctx = await dbContextFactory.CreateDbContextAsync();
            await SeedDb(ctx);


            objectContainer.RegisterInstanceAs<IDbContextFactory<DatahubProjectDBContext>>(dbContextFactory);
            objectContainer.RegisterInstanceAs(datahubPortalConfiguration);
        }

        [AfterScenario("toolbox")]
        public async Task AfterScenarioRequiringQueue(IObjectContainer objectContainer)
        {
        }

        private async Task SeedDb(DatahubProjectDBContext context)
        {
            var sub = new DatahubAzureSubscription
            {
                SubscriptionId = "00000000-0000-0000-0000-000000000000",
                TenantId = "00000000-0000-0000-0000-000000000000",
                Nickname = "Test",
                SubscriptionName = "Test",
            };
            context.AzureSubscriptions.Add(sub);

            var project = new Datahub_Project
            {
                Project_Acronym_CD = Testing.WorkspaceAcronym,
                DatahubAzureSubscriptionId = sub.Id,
                MetadataAdded = true,
                Project_Budget = 100.0M
            };
            context.Projects.Add(project);

            var credits = new Project_Credits
            {
                ProjectId = project.Project_ID,
                Current = 0
            };
            context.Project_Credits.Add(credits);

            var user = new PortalUser
            {
                Email = Testing.CurrentUserEmail,
                GraphGuid = "00000000-0000-0000-0000-000000000000",
                DisplayName = "Test User"
            };
            context.PortalUsers.Add(user);

            var projectUser = new UserRoleLinks
            {
                PortalUserId = user.Id,
                Project_ID = project.Project_ID
            };
            context.UserRolesLinks.Add(projectUser);
            await context.SaveChangesAsync();
        }
    }
}