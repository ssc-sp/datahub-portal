using BoDi;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Core.Model.Datahub;
using Datahub.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Datahub.SpecflowTests.Hooks;

[Binding]
public class ResourceMessagingHook
{
    [BeforeScenario("ResourceMessagingService")]
    public void BeforeScenarioRequiringResourceMessaging(IObjectContainer objectContainer,
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
        
        objectContainer.RegisterInstanceAs<IResourceMessagingService>(resourceMessagingService);
        objectContainer.RegisterInstanceAs<IDbContextFactory<DatahubProjectDBContext>>(dbContextFactory);
    }
}