using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Datahub.Core.Services.Projects;
using Datahub.Infrastructure.Offline;
using Datahub.Infrastructure.Services;
using Datahub.Shared.Entities;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ClearExtensions;
using Reqnroll;
using Reqnroll.BoDi;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace Datahub.SpecflowTests.Hooks;

[Binding]
public class Hooks
{
    [BeforeScenario("queue")]
    public void BeforeScenarioRequiringQueue(IObjectContainer objectContainer, ScenarioContext scenarioContext)
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

        var datahubAuditingService = new OfflineDatahubTelemetryAuditingService();
        var mockSendEndpointProvider = Substitute.For<ISendEndpointProvider>();
        var workspaceVersionService = Substitute.For<IWorkspaceVersionService>();
        var actualResourceMessageService = new ResourceMessagingService(dbContextFactory, mockSendEndpointProvider, workspaceVersionService);

        var substituteResourceMessageService = Substitute.For<IResourceMessagingService>();
        substituteResourceMessageService.ClearSubstitute();
        substituteResourceMessageService.SendToTerraformQueue(Arg.Any<WorkspaceDefinition>())
            .Returns(Task.CompletedTask);

        substituteResourceMessageService.GetWorkspaceDefinition(Arg.Any<string>(), Arg.Any<string?>())
            .Returns(callInfo =>
                actualResourceMessageService.GetWorkspaceDefinition((string)callInfo[0]));

        var requestManagementService = new RequestManagementService(
            Substitute.For<ILogger<RequestManagementService>>(),
            dbContextFactory,
            datahubAuditingService,
            substituteResourceMessageService,
            workspaceVersionService);

        // register dependencies
        objectContainer.RegisterInstanceAs(datahubPortalConfiguration);
        objectContainer.RegisterInstanceAs<IDbContextFactory<DatahubProjectDBContext>>(dbContextFactory);
        objectContainer.RegisterInstanceAs(substituteResourceMessageService);
        objectContainer.RegisterInstanceAs<IRequestManagementService>(requestManagementService);
    }


    [BeforeScenario("IWebHostEnvironment")]
    public async Task BeforeScenarioRequiringOffline(IObjectContainer objectContainer, ScenarioContext scenarioContext)
    {
        // Use a concrete test implementation of IWebHostEnvironment instead of a substitute
        var testEnvironment = new TestWebHostEnvironment
        {
            EnvironmentName = "Hosting:SpecflowUnitTestingEnvironment",
            ApplicationName = "Datahub.SpecflowTests",
            ContentRootPath = System.IO.Directory.GetCurrentDirectory(),
            WebRootPath = System.IO.Directory.GetCurrentDirectory(),
            ContentRootFileProvider = new PhysicalFileProvider(System.IO.Directory.GetCurrentDirectory()),
            WebRootFileProvider = new PhysicalFileProvider(System.IO.Directory.GetCurrentDirectory())
        };

        objectContainer.RegisterInstanceAs<IWebHostEnvironment>(testEnvironment);
        
        // Register IDbContextFactory for scenarios that might need it during teardown
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContextFactory = new SpecFlowDbContextFactory(options);
        objectContainer.RegisterInstanceAs<IDbContextFactory<DatahubProjectDBContext>>(dbContextFactory);
    }

    // Simple concrete IWebHostEnvironment implementation for tests
    private class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = string.Empty;
        public string ApplicationName { get; set; } = string.Empty;
        public string WebRootPath { get; set; } = string.Empty;
        public IFileProvider? WebRootFileProvider { get; set; }
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider? ContentRootFileProvider { get; set; }
    }
}