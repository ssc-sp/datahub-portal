using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Subscriptions;
using Datahub.Core.Model.Users;
using Datahub.Core.Services.Projects;
using Datahub.Infrastructure.Services;
using Datahub.Shared.Configuration;
using Datahub.Shared.Entities;
using Datahub.SpecflowTests.Utils;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
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
            await GenerateWorkspaceHelper.GenerateWorkspace(
                dbContextFactory,
                Testing.WorkspaceAcronym, generateResourceGroup: false);

            objectContainer.RegisterInstanceAs<IDbContextFactory<DatahubProjectDBContext>>(dbContextFactory);
            objectContainer.RegisterInstanceAs(datahubPortalConfiguration);
        }

        [AfterScenario("toolbox")]
        public async Task AfterScenarioRequiringQueue(IObjectContainer objectContainer)
        {
        }

    }
}