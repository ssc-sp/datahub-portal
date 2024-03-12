using BoDi;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services.Projects;
using Datahub.Infrastructure.Offline;
using Datahub.Infrastructure.Services;
using Datahub.ProjectTools.Services;
using Datahub.Shared.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;

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

		var datahubPortalConfiguration = new DatahubPortalConfiguration();
		configuration.Bind(datahubPortalConfiguration);

		// setup in memory provider ef core context
		var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
			.UseInMemoryDatabase(databaseName: "DatahubProjectDB")
			.Options;

		var dbContextFactory = new SpecFlowDbContextFactory(options);

		var datahubAuditingService = new OfflineDatahubTelemetryAuditingService();
		var actualResourceMessageService = new ResourceMessagingService(datahubPortalConfiguration, dbContextFactory);

		var substituteResourceMessageService = Substitute.For<IResourceMessagingService>();
		substituteResourceMessageService.SendToTerraformQueue(Arg.Any<WorkspaceDefinition>())
			.Returns(Task.CompletedTask);

		substituteResourceMessageService.GetWorkspaceDefinition(Arg.Any<string>(), Arg.Any<string?>())
			.Returns(callInfo =>
				actualResourceMessageService.GetWorkspaceDefinition((string)callInfo[0]));

		var requestManagementService = new RequestManagementService(
			Substitute.For<ILogger<RequestManagementService>>(),
			dbContextFactory,
			datahubAuditingService,
			substituteResourceMessageService);

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
}