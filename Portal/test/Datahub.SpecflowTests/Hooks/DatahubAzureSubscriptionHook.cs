using Datahub.Application.Configuration;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.Subscriptions;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Subscriptions;
using Datahub.Infrastructure.Services.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.Extensions;
using Reqnroll;
using Reqnroll.BoDi;

namespace Datahub.SpecflowTests.Hooks;

[Binding]
public class DatahubAzureSubscriptionHook
{
    [BeforeScenario("AzureDatahubSubscription", Order = 2)]
    public void BeforeScenarioAzureDatahubSubscription(IObjectContainer objectContainer,
        ScenarioContext scenarioContext)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<Hooks>()
            .AddJsonFile("appsettings.test.json", optional: true)
            .Build();
        
        var datahubPortalConfiguration = new DatahubPortalConfiguration();
        configuration.Bind(datahubPortalConfiguration);
        
        // Reuse the database name from IWebHostEnvironment hook if it exists
        var databaseName = scenarioContext.ContainsKey("DatabaseName") 
            ? scenarioContext["DatabaseName"].ToString()!
            : $"TestDb_{scenarioContext.ScenarioInfo.Title.Replace(" ", "_")}_{Guid.NewGuid()}";
        
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;
        
        var dbContextFactory = new SpecFlowDbContextFactory(options);
        
        // var datahubAzureSubscriptionService = new DatahubAzureSubscriptionService(dbContextFactory, datahubPortalConfiguration);

        var fetchedSubscription = Task.FromResult(new DatahubAzureSubscription
        {
            TenantId = Testing.WorkspaceTenantGuid,
            SubscriptionId = Testing.WorkspaceSubscriptionGuid,
            SubscriptionName = Testing.SubscriptionName
        });        
        
        var datahubAzureSubscriptionService = Substitute.ForPartsOf<DatahubAzureSubscriptionService>(dbContextFactory, datahubPortalConfiguration);
        
        datahubAzureSubscriptionService
            .Configure()
            .FetchSubscriptionResource(Arg.Is<string>(s => s != "invalid-subscription-id"))!
            .Returns(fetchedSubscription);
        
        datahubAzureSubscriptionService
            .Configure()
            .FetchSubscriptionResource(Arg.Is<string>("invalid-subscription-id"))!
            .ThrowsAsync(new InvalidOperationException("Subscription not found. Please check the subscription id and access permissions."));

        var tokenCredential = Substitute.For<ISystemTokenCredentialService>();

        var unstubbedDatahubAzureSubscriptionService = new DatahubAzureSubscriptionService(dbContextFactory, tokenCredential, datahubPortalConfiguration);
        
        scenarioContext["unstubbedDatahubAzureSubscriptionService"] = unstubbedDatahubAzureSubscriptionService;
        
        objectContainer.RegisterInstanceAs(datahubPortalConfiguration);
        objectContainer.RegisterInstanceAs<IDbContextFactory<DatahubProjectDBContext>>(dbContextFactory);
        objectContainer.RegisterInstanceAs<IDatahubAzureSubscriptionService>(datahubAzureSubscriptionService);
    }
}
