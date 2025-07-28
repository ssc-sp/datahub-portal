using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.Metadata;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.Subscriptions;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Services.CatalogSearch;
using Datahub.Infrastructure.Services;
using Datahub.Metadata.DTO;
using Datahub.Metadata.Model;
using Datahub.Metadata.Utils;
using Datahub.Portal.Controllers;
using Datahub.Shared.Configuration;
using DocumentFormat.OpenXml.Wordprocessing;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private readonly IMetadataBrokerService _metadataService;

        private const string REQUEST_BODY_CONTEXT_KEY = "requestBody";
        private const string CREATED_WORKSPACE_ACRONYM_CONTEXT_KEY = "workspaceAcronym";

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
            _metadataService = CreateMockMetadataService();
            _projectCreationService = CreateMockedWorkspaceCreationService(_datahubPortalConfiguration, dbContextFactory, _userInformationService, _metadataService);
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

        private static FieldDefinitions GenerateWorkspaceFieldDefs()
        {
            var i = 1;
            var fieldDefList = new List<FieldDefinition>()
            {
                new() {FieldDefinitionId = i++, Field_Name_TXT=FieldNames.name_en},
                new() {FieldDefinitionId = i++, Field_Name_TXT=FieldNames.name_fr},
                new() {FieldDefinitionId = i++, Field_Name_TXT=FieldNames.description_en},
                new() {FieldDefinitionId = i++, Field_Name_TXT=FieldNames.description_fr},
                new() {FieldDefinitionId = i++, Field_Name_TXT=FieldNames.keywords_en},
                new() {FieldDefinitionId = i++, Field_Name_TXT=FieldNames.keywords_fr},
                new() {FieldDefinitionId = i++, Field_Name_TXT=FieldNames.creator},
                new() {FieldDefinitionId = i++, Field_Name_TXT=FieldNames.contact_email},
                new() {FieldDefinitionId = i++, Field_Name_TXT=FieldNames.organization_name, Choices=[new() {Value_TXT="exa",Label_English_TXT= "Department of Example" }] },
                new() {FieldDefinitionId = i++, Field_Name_TXT=FieldNames.security_classification, Choices=[new() { Value_TXT="0", Label_English_TXT="Unclassified"}]},
                new() {FieldDefinitionId = i++, Field_Name_TXT=FieldNames.subject, Choices=[new() { Value_TXT="example", Label_English_TXT= "Example Subject" }]},
                new() {FieldDefinitionId = i++, Field_Name_TXT=FieldNames.access_restrictions, Choices=[new() { Value_TXT="0", Label_English_TXT="Unrestricted"}]}
            };

            var defs = new FieldDefinitions();
            defs.Add(fieldDefList);

            return defs;
        }

        private static IMetadataBrokerService CreateMockMetadataService()
        {
            var metadataCache = new Dictionary<string, FieldValueContainer>();
            var metadataService = Substitute.For<IMetadataBrokerService>();

            var defs = GenerateWorkspaceFieldDefs();
            metadataService.GetFieldDefinitions().Returns(defs);

            metadataService.SaveMetadata(Arg.Do<FieldValueContainer>(f =>
            {
                if (f is not null)
                {
                    metadataCache.Add(f.ObjectId, f);
                }
            }), Arg.Any<bool>());

            metadataService.GetObjectMetadataValues(Arg.Any<string>())
                .Returns(x => metadataCache.TryGetValue((string)x[0], out var result) ?
                    result :
                    new FieldValueContainer(0, (string)x[0], defs, new List<ObjectFieldValue>()));

            return metadataService;
        }

        private static IWorkspaceCreationService CreateMockedWorkspaceCreationService(
            DatahubPortalConfiguration datahubPortalConfiguration,
            IDbContextFactory<DatahubProjectDBContext> dbContextFactory, 
            IUserInformationService userInformationService,
            IMetadataBrokerService metadataService)
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
                        catalogSearch,
                        metadataService);

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
            _scenarioContext[REQUEST_BODY_CONTEXT_KEY] = requestBody;
        }
        //
        [Then("the response should have a {int} status code and {string} json")]
        public async Task ThenTheResponseShouldHaveAStatusCode(int response_code, string response_json)
        {
            // Arrange
            var context = new DefaultHttpContext();
            var requestString = (string)_scenarioContext[REQUEST_BODY_CONTEXT_KEY];
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
            Assert.True(response_code == (result as ObjectResult)?.StatusCode, (result as ObjectResult)?.Value?.ToString());

            if (result is OkObjectResult objectResult)
            {
                var valueObj = (Dictionary<string, string>)objectResult.Value!;
                valueObj.Should().NotBeNull();
                valueObj.Should().HaveCount(2);
                var acronym = valueObj["Acronym"];
                // Deserialize both the actual and expected JSON into objects
                var expectedObject = JsonConvert.DeserializeObject<JObject>(response_json);

                // Assert that the deserialized objects are equivalent
                Assert.Equal(expectedObject["Acronym"], acronym);
                Assert.Equal(expectedObject["ResourceGroup"], valueObj["ResourceGroup"]);                

                acronym.Should().NotBeNullOrEmpty();
                _scenarioContext[CREATED_WORKSPACE_ACRONYM_CONTEXT_KEY] = acronym;
            }

            //var okResult = Assert.IsType<OkObjectResult>(result);
        }

        [Then("the created project metadata should not be filled in")]
        public void ThenTheCreatedProjectMetadataShouldNotBeFilledIn()
        {
            _scenarioContext.Should().NotContainKey(CREATED_WORKSPACE_ACRONYM_CONTEXT_KEY);
        }

        [Then("the created project metadata should be filled in")]
        public async Task ThenTheCreatedProjectMetadataShouldBeFilledIn()
        {
            var requestString = (string)_scenarioContext[REQUEST_BODY_CONTEXT_KEY];
            var acronym = (string)_scenarioContext[CREATED_WORKSPACE_ACRONYM_CONTEXT_KEY];
            var hostingServiceInfo = JsonConvert.DeserializeObject<HostingServiceInfo>(requestString);
            hostingServiceInfo.Should().NotBeNull();

            var metadata = await _metadataService.GetObjectMetadataValues(acronym);
            metadata.Should().NotBeNull();
            metadata[FieldNames.name_en].Value_TXT.Should().Be(hostingServiceInfo!.WorkspaceName);
            metadata[FieldNames.name_fr].Value_TXT.Should().Be(hostingServiceInfo!.WorkspaceName);
            metadata[FieldNames.description_en].Value_TXT.Should().Be(hostingServiceInfo!.WorkspaceDescription);
            metadata[FieldNames.description_fr].Value_TXT.Should().Be(hostingServiceInfo!.WorkspaceDescription);
            metadata[FieldNames.keywords_en].Value_TXT.Should().Be(hostingServiceInfo!.Keywords);
            metadata[FieldNames.keywords_fr].Value_TXT.Should().Be(hostingServiceInfo!.Keywords);
            metadata[FieldNames.contact_email].Value_TXT.Should().Be(hostingServiceInfo!.LeadEmail);
            metadata[FieldNames.creator].Value_TXT.Should().Contain(hostingServiceInfo!.LeadFirstName);
            metadata[FieldNames.creator].Value_TXT.Should().Contain(hostingServiceInfo!.LeadLastName);

            TestLookupField(metadata, FieldNames.organization_name, hostingServiceInfo!.DepartmentName);
            TestLookupField(metadata, FieldNames.security_classification, hostingServiceInfo!.SecurityClassification);
            TestLookupField(metadata, FieldNames.subject, hostingServiceInfo!.Subject);
        }

        private static void TestLookupField(FieldValueContainer metadata, string fieldName, string inputValue)
        {
            var fieldDef = metadata.Definitions.Get(fieldName);
            fieldDef.Should().NotBeNull();
            fieldDef.Choices.Should().NotBeNullOrEmpty();

            if (fieldDef.Choices.Select(c => c.Label_English_TXT).Contains(inputValue))
            {
                // input value has a corresponding mapping => should match that mapping
                var fieldValue = metadata[fieldName];
                fieldValue.Should().NotBeNull();
                fieldValue.Value_TXT.Should().NotBeNullOrEmpty();
                fieldDef.GetChoiceTextValue(fieldValue.Value_TXT, true).Should().Be(inputValue);
            }
            else
            {
                // input value does not have a correponding mapping => should be left unpopulated
                var fieldValue = metadata[fieldName];
                fieldValue.Should().BeNull();
            }
        }
    }
}
