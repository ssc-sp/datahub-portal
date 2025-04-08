using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.Subscriptions;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Services.CatalogSearch;
using Datahub.Infrastructure.Services;
using Datahub.Portal.Controllers;
using Datahub.Shared.Configuration;
using DocumentFormat.OpenXml.Wordprocessing;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;
using Octokit;
using Reqnroll;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Datahub.Portal.Controllers.HostingServicesController;

namespace Datahub.SpecflowTests.Steps.GCHosting
{
    [Binding]
    public class APICreateWorkspaceStepDefinitions
    {

        private readonly ILogger<HostingServicesController> _logger;
        private readonly IWorkspaceCreationService _projectCreationService;
        private readonly IUserInformationService _userInformationService;
        private readonly IUserEnrollmentService _userEnrollmentService;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly DatahubPortalConfiguration _datahubPortalConfiguration;
        private readonly ScenarioContext _scenarioContext;
        private readonly DatahubProjectDBContext _dbContext;
        private readonly HostingServicesController _controller;

        public APICreateWorkspaceStepDefinitions(ScenarioContext scenarioContext)
        {
            var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dbContextFactory = new SpecFlowDbContextFactory(options);
            _logger = Substitute.For<ILogger<HostingServicesController>>();
            _userInformationService = Substitute.For<IUserInformationService>();
            _userEnrollmentService = Substitute.For<IUserEnrollmentService>();
            _sendEndpointProvider = Substitute.For<ISendEndpointProvider>();
            _datahubPortalConfiguration = Substitute.For<DatahubPortalConfiguration>();
            _projectCreationService = CreateMockedWorkspaceCreationService(_datahubPortalConfiguration, dbContextFactory, _userInformationService);
            _scenarioContext = scenarioContext;
            _dbContext = dbContextFactory.CreateDbContext();
            _controller = new HostingServicesController(
                _dbContext,
                _projectCreationService,
                _userInformationService,
                _userEnrollmentService,
                _logger,
                _sendEndpointProvider,
                _datahubPortalConfiguration
            );
        }

        private static IWorkspaceCreationService CreateMockedWorkspaceCreationService(
            DatahubPortalConfiguration datahubPortalConfiguration,
            IDbContextFactory<DatahubProjectDBContext> dbContextFactory, 
            IUserInformationService userInformationService)
        {
            var logger = Substitute.For<ILogger<WorkspaceCreationService>>();
            var serviceAuthManager = Substitute.For<IServiceAuthManager>();
            var resourceMessagingService = Substitute.For<IResourceMessagingService>();
            var auditingService = Substitute.For<IDatahubAuditingService>();
            var azureSubService = Substitute.For<IDatahubAzureSubscriptionService>();
            var catalogSearch = Substitute.For<IDatahubCatalogSearch>();

            azureSubService.NextSubscriptionAsync()
                .Returns(new Core.Model.Subscriptions.DatahubAzureSubscription() { Id = 1 });

            var mockedWorkspaceCreationService = Substitute.ForPartsOf<WorkspaceCreationService>(
                        datahubPortalConfiguration,
                        dbContextFactory,
                        logger,
                        serviceAuthManager,
                        userInformationService,
                        resourceMessagingService,
                        auditingService,
                        azureSubService,
                        catalogSearch);

            mockedWorkspaceCreationService.When(c => c.GenerateWorkspaceAcronymAsync(Arg.Any<string>())).DoNotCallBase();

            mockedWorkspaceCreationService.GenerateWorkspaceAcronymAsync(Arg.Any<string>())
                .Returns("TEST");

            return mockedWorkspaceCreationService;
        }

        [Given("a request with (.*)")]
        public async Task GivenARequestWithID(string id)
        {
            var jsonData = Path.Combine("Features/GCHosting/requests", id + ".json");
            if (!File.Exists(jsonData))
            {
                throw new Exception($"File {jsonData} does not exist");
            }
            var requestBody = await File.ReadAllTextAsync(jsonData);
            _scenarioContext["requestBody"] = requestBody;
        }

        [Then("the response should have a {int} status code")]
        public async Task ThenTheResponseShouldHaveAStatusCode(int p0)
        {
            // Arrange
            var context = new DefaultHttpContext();
            var requestString = (string)_scenarioContext["requestBody"];
            var cGuid = Guid.NewGuid().ToString();
            string currentEmail = null!;

            _userEnrollmentService.SendUserDatahubPortalInvite(Arg.Any<string>(), Arg.Any<string>())
                .ReturnsForAnyArgs(e => {
                    currentEmail = (string)e[0];
                    return cGuid;
                });

            _userInformationService.GetCurrentPortalUserAsync()
                .Returns(new PortalUser
                {
                    Email = currentEmail,
                    GraphGuid = cGuid
                });

            _userInformationService.CreatePortalUserAsync(Arg.Any<string>())
                .ReturnsForAnyArgs(async userName => {
                    Assert.Equal(cGuid, userName[0]);
                    _dbContext.PortalUsers.Add(new PortalUser
                    {
                        Email = currentEmail,
                        GraphGuid = cGuid
                    });
                    await _dbContext.SaveChangesAsync();                    
                });

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestString));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            // Act
            var result = await _controller.PostCreateWorkspace();
            Assert.NotNull(result);
            Assert.True(p0 == (result as ObjectResult)?.StatusCode, (result as ObjectResult)?.Value?.ToString());
            //var okResult = Assert.IsType<OkObjectResult>(result);
        }
    }
}
