using Bunit;
using Bunit.TestDoubles;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Data;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Services.Projects;
using Datahub.Core.Services.UserManagement;
using Datahub.Infrastructure.Offline;
using Datahub.Portal.Pages.Workspace.Settings;
using Datahub.Shared.Entities;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps
{
    [Binding]
    public class WorkspaceUsersSteps(
        ScenarioContext scenarioContext,
        IWebHostEnvironment hostingEnvironment
    ) : TestContext
    {

        private const string RelativePathToSrc = "../../../../../src";

        [Given("the user is on the workspace users page")]
        public async Task GivenTheUserIsOnTheWorkspaceUsersPage()
        {
            Services.AddSingleton(hostingEnvironment);
            var portalConfiguration = new DatahubPortalConfiguration()
            {
                CultureSettings =
            {
                ResourcesPath = $"{RelativePathToSrc}/Datahub.Portal/i18n",
                AdditionalResourcePaths = []
            }
            };

            Services.AddMudServices();
            Services.AddDatahubLocalization(portalConfiguration);

            Services.AddStub<ICultureService>();
            Services.AddStub<IDatahubAuditingService>();
            Services.AddStub<IUserInformationService>();

            var mockRequestManagementService = Substitute.For<IRequestManagementService>();
            Services.AddSingleton(mockRequestManagementService);

            var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var dbContextFactory = new SpecFlowDbContextFactory(options);
            Services.AddSingleton<IDbContextFactory<DatahubProjectDBContext>>(dbContextFactory);

            var workspace = new Datahub_Project()
            {
                Project_Acronym_CD = Testing.WorkspaceAcronym,
            };

            await using var context = await dbContextFactory.CreateDbContextAsync();
            context.Projects.Add(workspace);
            await context.SaveChangesAsync();

            var mockAuthorizationPolicyProvider = Substitute.For<IAuthorizationPolicyProvider>();
            mockAuthorizationPolicyProvider.GetDefaultPolicyAsync()
                .Returns(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

            var authContext = this.AddTestAuthorization();
            authContext.SetAuthorized("TEST USER");
            authContext.SetRoles(RoleConstants.DATAHUB_ROLE_ADMIN);

            Services.AddSingleton(mockAuthorizationPolicyProvider);

            JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);

            var workspaceSettingsPage = RenderComponent<WorkspaceSettingsPage>(parameterCollection =>
                parameterCollection.Add(p => p.WorkspaceAcronym, Testing.WorkspaceAcronym));

            scenarioContext["workspaceSettingsPage"] = workspaceSettingsPage;
        }

        [When(@"the user adds a new user with email ""(.*)""")]
        public void WhenTheUserAddsANewUserWithEmail(string email)
        {
            // Implement the step logic here
        }

        [When(@"the user removes the user with email ""(.*)""")]
        public void WhenTheUserRemovesTheUserWithEmail(string email)
        {
            // Implement the step logic here
        }

        [When(@"the user sets the Data Stuart role for the user with email ""(.*)""")]
        public void WhenTheUserSetsTheDataStuartRoleForTheUserWithEmail(string email)
        {
            // Implement the step logic here
        }

        [When("the user clicks the \"Save\" button")]
        public void WhenTheUserClicksTheSaveButton()
        {
            // Implement the step logic here
        }

        [Then("the workspace users page should be displayed")]
        public void ThenTheWorkspaceUsersPageShouldBeDisplayed()
        {
            // scenarioContext.Pending();
        }

        [Then(@"the user with email ""(.*)"" should have the Data Stuart role")]
        public void ThenTheUserWithEmailShouldHaveTheDataStuartRole(string email)
        {
            // Implement the step logic here
        }

        [Then("the new user should be added to the workspace")]
        public void ThenTheNewUserShouldBeAddedToTheWorkspace()
        {
            //scenarioContext.Pending();
        }

        [Then("the user should be removed from the workspace")]
        public void ThenTheUserShouldBeRemovedFromTheWorkspace()
        {
            //scenarioContext.Pending();
        }

        [When("the user removes the Data Stuart role from the user with email {string}")]
        public void WhenTheUserRemovesTheDataStuartRoleFromTheUserWithEmail(string email)
        {
            // scenarioContext.Pending();
        }

        [Then("the user with email {string} should not have the Data Stuart role")]
        public void ThenTheUserWithEmailShouldNotHaveTheDataStuartRole(string email)
        {
            // scenarioContext.Pending();
        }


    }
}