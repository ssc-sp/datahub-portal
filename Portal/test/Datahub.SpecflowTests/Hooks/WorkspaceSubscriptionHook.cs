using BoDi;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services;
using Datahub.Core.Services.CatalogSearch;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Reqnroll;

namespace Datahub.SpecflowTests.Hooks;

[Binding]
public class WorkspaceSubscriptionHook
{
    [BeforeScenario("RequiringResourceMessaging")]
    public async Task BeforeScenarioRequiringResourceMessaging(IObjectContainer objectContainer,
        ScenarioContext scenarioContext)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("appsettings.test.json", optional: true)
            .Build();
        
        var datahubPortalConfiguration = new DatahubPortalConfiguration();
        configuration.Bind(datahubPortalConfiguration);
        
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(databaseName: "DatahubProjectDB")
            .Options;

        var dbContextFactory = new SpecFlowDbContextFactory(options);
        
        var resourceMessagingService = new ResourceMessagingService(datahubPortalConfiguration, dbContextFactory);
        var resourceMessagingSubstitute = Substitute.For<IResourceMessagingService>();
        resourceMessagingSubstitute.GetWorkspaceDefinition(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo =>
                resourceMessagingService.GetWorkspaceDefinition(callInfo.Arg<string>(), callInfo.Arg<string>()));
        
        
        var currentUser = new PortalUser
        {
            GraphGuid = Testing.CURRENT_USER_GUID.ToString(),
            Email = Testing.CURRENT_USER_EMAIL
        };
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.PortalUsers.Add(currentUser);
        await dbContext.SaveChangesAsync();
        
        var userInformationService = Substitute.For<IUserInformationService>();
        userInformationService.GetCurrentPortalUserAsync().Returns(currentUser);
        
        var projectCreationService = new ProjectCreationService(
            datahubPortalConfiguration,
            dbContextFactory,
            Substitute.For<ILogger<ProjectCreationService>>(),
            Substitute.For<IServiceAuthManager>(),
            userInformationService,
            resourceMessagingSubstitute,
            Substitute.For<IDatahubAuditingService>(),
            Substitute.For<IDatahubCatalogSearch>());
        
        
        var datahubAzureSubscriptionService = new DatahubAzureSubscriptionService(dbContextFactory, datahubPortalConfiguration);
        
        objectContainer.RegisterInstanceAs<IResourceMessagingService>(resourceMessagingService);
        objectContainer.RegisterInstanceAs<IDbContextFactory<DatahubProjectDBContext>>(dbContextFactory);
        objectContainer.RegisterInstanceAs<IProjectCreationService>(projectCreationService);
        objectContainer.RegisterInstanceAs<IDatahubAzureSubscriptionService>(datahubAzureSubscriptionService);
    }
}