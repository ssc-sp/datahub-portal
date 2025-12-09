using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.Metadata;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.Subscriptions;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Users;
using Datahub.Core.Services.CatalogSearch;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Subscriptions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Reqnroll;
using Reqnroll.BoDi;

namespace Datahub.SpecflowTests.Hooks;

[Binding]
public class WorkspaceSubscriptionHook
{
    [BeforeScenario("WorkspaceSubscriptionsLimited")]
    public void BeforeScenarioWorkspaceSubscriptionsLimited(IObjectContainer objectContainer,
        ScenarioContext scenarioContext)
    {
        var datahubPortalConfiguration = LoadConfiguration(objectContainer);
        datahubPortalConfiguration.Hosting.WorkspaceCountPerAzureSubscription = 1;
    }

    private DatahubPortalConfiguration LoadConfiguration(IObjectContainer objectContainer)
    {
        if (objectContainer.IsRegistered<DatahubPortalConfiguration>())
            return objectContainer.Resolve<DatahubPortalConfiguration>();

        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<Hooks>()
            .AddJsonFile("appsettings.test.json", optional: true)
            .Build();

        var datahubPortalConfiguration = new DatahubPortalConfiguration();
        configuration.Bind(datahubPortalConfiguration);

        objectContainer.RegisterInstanceAs(datahubPortalConfiguration);
        return datahubPortalConfiguration;
    }

    [BeforeScenario("RequiringResourceMessaging")]
    public async Task BeforeScenarioRequiringResourceMessaging(IObjectContainer objectContainer,
        ScenarioContext scenarioContext)
    {
        var datahubPortalConfiguration = LoadConfiguration(objectContainer);

        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var dbContextFactory = new SpecFlowDbContextFactory(options);
        var mockISendEndpointProvider = Substitute.For<ISendEndpointProvider>();
        var workspaceVersionService = Substitute.For<IWorkspaceVersionService>(); 

        var resourceMessagingService = new ResourceMessagingService(dbContextFactory, mockISendEndpointProvider, workspaceVersionService);
        var resourceMessagingSubstitute = Substitute.For<IResourceMessagingService>();

        resourceMessagingSubstitute.GetWorkspaceDefinition(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo =>
                resourceMessagingService.GetWorkspaceDefinition(callInfo.Arg<string>(), callInfo.Arg<string>()));

        var currentUser = new PortalUser
        {
            GraphGuid = Testing.CurrentUserGuid.ToString(),
            Email = Testing.CurrentUserEmail
        };
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.PortalUsers.Add(currentUser);
        await dbContext.SaveChangesAsync();

        var userInformationService = Substitute.For<IUserInformationService>();
        userInformationService.GetCurrentPortalUserAsync().Returns(currentUser);

        var datahubAzureSubscriptionService =
            new DatahubAzureSubscriptionService(dbContextFactory, datahubPortalConfiguration);

        var projectCreationService = new WorkspaceCreationService(
            datahubPortalConfiguration,
            dbContextFactory,
            Substitute.For<ILogger<WorkspaceCreationService>>(),
            Substitute.For<IServiceAuthManager>(),
            userInformationService,
            workspaceVersionService,
            Substitute.For<IDatahubAuditingService>(),
            datahubAzureSubscriptionService,
            Substitute.For<IDatahubCatalogSearch>(),
            Substitute.For<IMetadataBrokerService>());


        objectContainer.RegisterInstanceAs<IResourceMessagingService>(resourceMessagingService);
        objectContainer.RegisterInstanceAs<IDbContextFactory<DatahubProjectDBContext>>(dbContextFactory);
        objectContainer.RegisterInstanceAs<IWorkspaceCreationService>(projectCreationService);
        objectContainer.RegisterInstanceAs<IDatahubAzureSubscriptionService>(datahubAzureSubscriptionService);
    }
}