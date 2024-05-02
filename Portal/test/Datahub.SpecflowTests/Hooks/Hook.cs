using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using BoDi;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.Budget;
using Datahub.Application.Services.Storage;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services.Projects;
using Datahub.Infrastructure.Offline;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Cost;
using Datahub.Infrastructure.Services.Storage;
using Datahub.Shared.Entities;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Reqnroll;

namespace Datahub.SpecflowTests.Hooks;

[Binding]
public class Hooks
{
    [BeforeScenario("queue")]
    public void BeforeScenarioRequiringQueue(IObjectContainer objectContainer, ScenarioContext scenarioContext)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json", optional: true)
            .Build();

        var capturedWorkspaces = new List<WorkspaceDefinition>();

        var datahubPortalConfiguration = new DatahubPortalConfiguration();
        configuration.Bind(datahubPortalConfiguration);

        // setup in memory provider ef core context
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var dbContextFactory = new SpecFlowDbContextFactory(options);

        var datahubAuditingService = new OfflineDatahubTelemetryAuditingService();
        var actualResourceMessageService = new ResourceMessagingService(datahubPortalConfiguration, dbContextFactory);

        var substituteResourceMessageService = Substitute.For<IResourceMessagingService>();
        substituteResourceMessageService.SendToTerraformQueue(Arg.Do<WorkspaceDefinition>(workspace =>
                capturedWorkspaces.Add(workspace)))
            .Returns(Task.CompletedTask);

        substituteResourceMessageService.GetWorkspaceDefinition(Arg.Any<string>(), Arg.Any<string?>())
            .Returns(callInfo =>
                actualResourceMessageService.GetWorkspaceDefinition((string)callInfo[0]));

        var requestManagementService = new RequestManagementService(
            Substitute.For<ILogger<RequestManagementService>>(),
            dbContextFactory,
            datahubAuditingService,
            substituteResourceMessageService);
        
        scenarioContext["CapturedWorkspaces"] = capturedWorkspaces;

        // register dependencies
        objectContainer.RegisterInstanceAs(datahubPortalConfiguration);
        objectContainer.RegisterInstanceAs<IDbContextFactory<DatahubProjectDBContext>>(dbContextFactory);
        objectContainer.RegisterInstanceAs<IResourceMessagingService>(substituteResourceMessageService);
        objectContainer.RegisterInstanceAs<IRequestManagementService>(requestManagementService);
    }

    [BeforeScenario("IWebHostEnvironment")]
    public void BeforeScenarioRequiringOffline(IObjectContainer objectContainer, ScenarioContext scenarioContext)
    {
        var substituteHostingEnvironment = Substitute.For<IWebHostEnvironment>();
        substituteHostingEnvironment.EnvironmentName.Returns("Hosting:SpecflowUnitTestingEnvironment");

        objectContainer.RegisterInstanceAs<IWebHostEnvironment>(substituteHostingEnvironment);
    }

    [BeforeScenario("WorkspaceManagement")]
    public void BeforeScenarioRequiringWorkspaceManagement(IObjectContainer objectContainer,
        ScenarioContext scenarioContext)
    {
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(databaseName: "DatahubProjectDB")
            .Options;

        var dbContextFactory = new SpecFlowDbContextFactory(options);

        objectContainer.RegisterInstanceAs<IDbContextFactory<DatahubProjectDBContext>>(dbContextFactory);
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json", optional: true)
            .Build();
        var datahubPortalConfiguration = new DatahubPortalConfiguration();
        configuration.Bind(datahubPortalConfiguration);
        var credentials = new ClientSecretCredential(datahubPortalConfiguration.AzureAd.TenantId,
            datahubPortalConfiguration.AzureAd.ClientId, datahubPortalConfiguration.AzureAd.ClientSecret);
        var armClientOptions = new ArmClientOptions()
        {
            Retry = { Delay = TimeSpan.FromSeconds(2), MaxRetries = 5, Mode = RetryMode.Exponential }
        };
        var armClient = new ArmClient(credentials, datahubPortalConfiguration.AzureAd.SubscriptionId, armClientOptions);

        var workspaceCostManagementService = new WorkspaceCostManagementService(armClient,
            Substitute.For<ILogger<WorkspaceCostManagementService>>(),
            objectContainer.Resolve<IDbContextFactory<DatahubProjectDBContext>>());
        var workspaceBudgetManagementService = new WorkspaceBudgetManagementService(armClient,
            Substitute.For<ILogger<WorkspaceBudgetManagementService>>(),
            objectContainer.Resolve<IDbContextFactory<DatahubProjectDBContext>>());
        var workspaceStorageManagementService = new WorkspaceStorageManagementService(armClient,
            Substitute.For<ILogger<WorkspaceStorageManagementService>>(),
            objectContainer.Resolve<IDbContextFactory<DatahubProjectDBContext>>());
        
        objectContainer.RegisterInstanceAs<IWorkspaceCostManagementService>(workspaceCostManagementService);
        objectContainer.RegisterInstanceAs<IWorkspaceBudgetManagementService>(workspaceBudgetManagementService);
        objectContainer.RegisterInstanceAs(workspaceBudgetManagementService);
        objectContainer.RegisterInstanceAs<IWorkspaceStorageManagementService>(workspaceStorageManagementService);

        // Mock costs to test querying, filtering and grouping
        DateTime Date1 = new(2024, 5, 1);
        DateTime Date2 = new(2023, 5, 1);
        DateTime Date3 = DateTime.UtcNow.Date;
        DateTime Date4 = DateTime.UtcNow.Date.AddDays(-1);
        string dbSource = "Database";
        string storageSource = "Storage account";
        string rg1 = "test-rg";
        string rg2 = "test-rg-2";
        scenarioContext["date1"] = Date1;
        scenarioContext["date2"] = Date2;
        scenarioContext["date3"] = Date3;
        scenarioContext["date4"] = Date4;
        scenarioContext["dbSource"] = dbSource;
        scenarioContext["storageSource"] = storageSource;
        scenarioContext["resourceGroup1"] = rg1;
        scenarioContext["resourceGroup2"] = rg2;

        // Mock costs to test querying, filtering and grouping
        var mockCosts = CreateMockCosts(Date1, Date2, rg1, rg2, dbSource, storageSource);
        scenarioContext["mockCosts"] = mockCosts;

        // Mock costs to test updating workspace costs
        var updateMockCosts = CreateMockCosts(Date3, Date4, rg1, rg1, dbSource, storageSource);
        scenarioContext["updateMockCosts"] = updateMockCosts;
    }

    [AfterScenario("WorkspaceManagement")]
    public void AfterScenarioRequiringWorkspaceManagement(IObjectContainer objectContainer, ScenarioContext scenarioContext)
    {
        var dbContextFactory = objectContainer.Resolve<IDbContextFactory<DatahubProjectDBContext>>();
        using var context = dbContextFactory.CreateDbContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }

    [BeforeScenario("MockWorkspaceManagement")]
    public void BeforeScenarioRequiringMockWorkspaceManagement(IObjectContainer objectContainer,
        ScenarioContext scenarioContext)
    {
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(databaseName: "DatahubProjectDB")
            .Options;

        var dbContextFactory = new SpecFlowDbContextFactory(options);

        objectContainer.RegisterInstanceAs<IDbContextFactory<DatahubProjectDBContext>>(dbContextFactory);

        var workspaceCostManagementService = Substitute.For<IWorkspaceCostManagementService>();
        var workspaceBudgetManagementService = Substitute.For<IWorkspaceBudgetManagementService>();
        var workspaceStorageManagementService = Substitute.For<IWorkspaceStorageManagementService>();
        var loggerFactory = Substitute.For<ILoggerFactory>();
        var mediator = Substitute.For<IMediator>();
        var pongService = new QueuePongService(mediator);
        
        objectContainer.RegisterInstanceAs(workspaceCostManagementService);
        objectContainer.RegisterInstanceAs(workspaceBudgetManagementService);
        objectContainer.RegisterInstanceAs(workspaceStorageManagementService);
        objectContainer.RegisterInstanceAs(loggerFactory);
        objectContainer.RegisterInstanceAs(pongService);
        objectContainer.RegisterInstanceAs(mediator);
    }
    
    [AfterScenario("MockWorkspaceManagement")]
    public void AfterScenarioRequiringMockWorkspaceManagement(IObjectContainer objectContainer, ScenarioContext scenarioContext)
    {
        var dbContextFactory = objectContainer.Resolve<IDbContextFactory<DatahubProjectDBContext>>();
        using var context = dbContextFactory.CreateDbContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }

    public List<DailyServiceCost> CreateMockCosts(DateTime date1, DateTime date2, string rg1, string rg2, string source1, string source2)
    {
        List<DailyServiceCost> mockCosts =
        [
            new DailyServiceCost
            {
                Amount = 1,
                Date = date1,
                Source = source1,
                ResourceGroupName = rg1
            },


            new DailyServiceCost
            {
                Amount = 2,
                Date = date1,
                Source = source2,
                ResourceGroupName = rg1
            },


            new DailyServiceCost
            {
                Amount = 3,
                Date = date2,
                Source = source1,
                ResourceGroupName = rg2
            }
        ];

        return mockCosts;
    }
}