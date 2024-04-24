using BoDi;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.Subscriptions;
using Datahub.Core.Model.Datahub;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Reqnroll;

namespace Datahub.SpecflowTests.Hooks;

[Binding]
public class DatahubAzureSubscriptionHook
{
    [BeforeScenario("AzureDatahubSubscription")]
    public async Task BeforeScenarioAzureDatahubSubscription(IObjectContainer objectContainer,
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
        
        var datahubAzureSubscriptionService = new DatahubAzureSubscriptionService(dbContextFactory, datahubPortalConfiguration);
        
        objectContainer.RegisterInstanceAs(datahubPortalConfiguration);
        objectContainer.RegisterInstanceAs<IDbContextFactory<DatahubProjectDBContext>>(dbContextFactory);
        objectContainer.RegisterInstanceAs<IDatahubAzureSubscriptionService>(datahubAzureSubscriptionService);
    }
}