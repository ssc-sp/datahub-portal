using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Datahub.Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Reqnroll;
using Reqnroll.BoDi;

namespace Datahub.SpecflowTests.Hooks;

[Binding]
public class WorkspaceDefinitionHook
{
    [BeforeScenario("workspace-definition")]
    public void BeforeScenarioRequiringWorkspaceDefinition(IObjectContainer objectContainer,
        ScenarioContext scenarioContext)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Hooks>()
            .AddJsonFile("appsettings.test.json", optional: true)
            .Build();

        var datahubPortalConfiguration = new DatahubPortalConfiguration();
        configuration.Bind(datahubPortalConfiguration);

        // setup in memory provider ef core context
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        var mockSendEndpointProvider = Substitute.For<ISendEndpointProvider>();
        var dbContextFactory = new SpecFlowDbContextFactory(options);

        var resourceMessagingService = new ResourceMessagingService(dbContextFactory, mockSendEndpointProvider);
        
        objectContainer.RegisterInstanceAs(datahubPortalConfiguration);
        objectContainer.RegisterInstanceAs<IResourceMessagingService>(resourceMessagingService);
        objectContainer.RegisterInstanceAs<IDbContextFactory<DatahubProjectDBContext>>(dbContextFactory);
    }
}