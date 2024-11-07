using Datahub.Application.Configuration;
using Datahub.Core.Model.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Reqnroll;
using Reqnroll.BoDi;

namespace Datahub.SpecflowTests.Hooks;

[Binding]
public class DbContextHook
{
    [BeforeScenario("DbContext")]
    public void BeforeScenarioDbContext(IObjectContainer objectContainer, ScenarioContext scenarioContext)
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

        var dbContextFactory = new SpecFlowDbContextFactory(options);
        
        // register dependencies
        objectContainer.RegisterInstanceAs(datahubPortalConfiguration);
        objectContainer.RegisterInstanceAs<IDbContextFactory<DatahubProjectDBContext>>(dbContextFactory);
    }
        
}