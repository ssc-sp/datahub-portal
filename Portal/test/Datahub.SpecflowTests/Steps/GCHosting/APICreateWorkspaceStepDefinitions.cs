using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Context;
using Datahub.Portal.Controllers;
using Datahub.Shared.Configuration;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Octokit;
using Reqnroll;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Datahub.SpecflowTests.Steps.GCHosting
{
    [Binding]
    public class APICreateWorkspaceStepDefinitions
    {

        private readonly ILogger<HostingServicesController> _logger;
        private readonly IProjectCreationService _projectCreationService;
        private readonly IUserInformationService _userInformationService;
        private readonly IUserEnrollmentService _userEnrollmentService;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly DatahubPortalConfiguration _datahubPortalConfiguration;
        private readonly ScenarioContext _scenarioContext;
        private readonly HostingServicesController _controller;

        public APICreateWorkspaceStepDefinitions(ScenarioContext scenarioContext)
        {
            var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dbContextFactory = new SpecFlowDbContextFactory(options);
            _logger = Substitute.For<ILogger<HostingServicesController>>();
            _projectCreationService = Substitute.For<IProjectCreationService>();
            _userInformationService = Substitute.For<IUserInformationService>();
            _userEnrollmentService = Substitute.For<IUserEnrollmentService>();
            _sendEndpointProvider = Substitute.For<ISendEndpointProvider>();
            _datahubPortalConfiguration = Substitute.For<DatahubPortalConfiguration>();
            _scenarioContext = scenarioContext;
            _controller = new HostingServicesController(
                dbContextFactory.CreateDbContext(),
                _projectCreationService,
                _userInformationService,
                _userEnrollmentService,
                _logger,
                _sendEndpointProvider,
                _datahubPortalConfiguration
            );
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
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes((string)_scenarioContext["requestBody"]));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            _projectCreationService.GenerateProjectAcronymAsync(Arg.Any<string>())
                .Returns("TEST");

            // Act
            var result = await _controller.PostCreateWorkspace();
            Assert.NotNull(result);
            Assert.Equal(p0, (result as ObjectResult)?.StatusCode);
            //var okResult = Assert.IsType<OkObjectResult>(result);
        }
    }
}
