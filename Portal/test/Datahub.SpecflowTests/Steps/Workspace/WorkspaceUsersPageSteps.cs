using System.Reflection;
using Bunit;
using Bunit.TestDoubles;
using Datahub.Application.Commands;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Data;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Users;
using Datahub.Core.Services.Projects;
using Datahub.Infrastructure.Offline;
using Datahub.Portal.Layout;
using Datahub.Portal.Pages.Workspace.Users;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.FeatureManagement;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using Reqnroll;
using Xunit;

namespace Datahub.SpecflowTests.Steps
{
    [Binding]
    public class WorkspaceUsersSteps(
        ScenarioContext scenarioContext,
        IWebHostEnvironment hostingEnvironment
    ) : BunitTestSteps
    {

        private const string RelativePathToSrc = "../../../../../src";
        private SpecFlowDbContextFactory dbContextFactory;

        private readonly ISnackbar _snackBar = Substitute.For<ISnackbar>();
        private readonly IStringLocalizer _stringLocalizer = Substitute.For<IStringLocalizer>();

        private static readonly string WORKSPACE_USERS_PAGE_CTX_KEY = "workspaceUsersPage";

        private static IEnumerable<UserRoleLinks> SetupProjectUsers()
        {
            var workspaceLeadRole = new Project_Role()
            {
                Id = (int)Project_Role.RoleNames.WorkspaceLead,
                Name = RoleConstants.WORKSPACE_LEAD_ROLE,
                Description = RoleConstants.WORKSPACE_LEAD_ROLE
            };
            var adminRole = new Project_Role()
            {
                Id = (int)Project_Role.RoleNames.Admin,
                Name = RoleConstants.ADMIN_ROLE,
                Description = RoleConstants.ADMIN_ROLE
            };
            var guestRole = new Project_Role()
            {
                Id = (int)Project_Role.RoleNames.Guest,
                Name = RoleConstants.GUEST_ROLE,
                Description = RoleConstants.GUEST_ROLE
            };

            yield return new UserRoleLinks()
            {
                PortalUserId = 1,
                PortalUser = new PortalUser() { Id = 1, EntraUser = new() { GraphGuid = Guid.NewGuid().ToString(), PortalUser = null! }, DisplayName = "Walter Lead", Email = "wlead@example.com" },
                Role = workspaceLeadRole,
                RoleId = workspaceLeadRole.Id,
                IsDataSteward = true
            };

            yield return new UserRoleLinks()
            {
                PortalUserId = 2,
                PortalUser = new PortalUser() { Id = 2, EntraUser = new() { GraphGuid = Guid.NewGuid().ToString(), PortalUser = null! }, DisplayName = "Nathan Admin", Email = "admin@example.com" },
                Role = adminRole,
                RoleId = adminRole.Id
            };

            yield return new UserRoleLinks()
            {
                PortalUserId = 3,
                PortalUser = new PortalUser() { Id = 3, EntraUser = new() { GraphGuid = Guid.NewGuid().ToString(), PortalUser = null! }, DisplayName = "Gary Guest", Email = "guest@example.com" },
                Role = guestRole,
                RoleId = guestRole.Id
            };
        }

        [Given("the user is on the workspace users page")]
        public async Task GivenTheUserIsOnTheWorkspaceUsersPage()
        {
            ArgumentNullException.ThrowIfNull(hostingEnvironment);

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
            Services.AddSingleton(portalConfiguration);

            Services.AddStub<ICultureService>();
            Services.AddStub<IDatahubAuditingService>();
            var featureManager = Substitute.For<IFeatureManagerSnapshot>();
            featureManager.IsEnabledAsync(Arg.Any<string>()).Returns(false);
            Services.AddSingleton(featureManager);

            var userInfoService = Substitute.For<IUserInformationService>();
            Services.AddSingleton(userInfoService);

            var mockRequestManagementService = Substitute.For<IRequestManagementService>();
            Services.AddSingleton(mockRequestManagementService);

            var mockProjectUserManagementService = Substitute.For<IProjectUserManagementService>();
            Services.AddSingleton(mockProjectUserManagementService);

            var mockLockedUserManagementService = Substitute.For<ILockedUserManagementService>();
            mockLockedUserManagementService
                .GetLockedUsersInWorkspaceAsync(Arg.Any<int>())
                .Returns(Task.FromResult(new List<UserLockStatus>()));
            Services.AddSingleton(mockLockedUserManagementService);

            var mockUserSettingsService = Substitute.For<IUserSettingsService>();
            Services.AddSingleton(mockUserSettingsService);

            var mockProjectUsers = SetupProjectUsers();

            // Set up the mock to return the mock data
            mockProjectUserManagementService.GetProjectUsersAsync(Arg.Any<string>())
                .Returns(Task.FromResult(mockProjectUsers.ToList()));

            userInfoService.GetCurrentPortalUserAsync()
                .Returns(mockProjectUsers.First().PortalUser);

            _stringLocalizer[Arg.Any<string>()].Returns(new LocalizedString("test", "test")); 

            var optionsBuilder = new DbContextOptionsBuilder<DatahubProjectDBContext>(); 
            optionsBuilder.EnableSensitiveDataLogging();  

            var options = optionsBuilder
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            dbContextFactory = new SpecFlowDbContextFactory(options);
            Services.AddSingleton<IDbContextFactory<DatahubProjectDBContext>>(dbContextFactory);

            var workspace = new Datahub_Project()
            {
                Project_Acronym_CD = Testing.WorkspaceAcronym,
            };

            await using var context = await dbContextFactory.CreateDbContextAsync();
            context.Projects.Add(workspace);
            context.UserRolesLinks.AddRange(mockProjectUsers);

            foreach (var role in context.Project_Roles)
            {
                if (string.IsNullOrWhiteSpace(role.Description))
                {
                    role.Description = role.Name;
                }
            }
            
            await context.SaveChangesAsync();

            var mockAuthorizationPolicyProvider = Substitute.For<IAuthorizationPolicyProvider>();
            mockAuthorizationPolicyProvider.GetDefaultPolicyAsync()
                .Returns(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

            var authContext = this.AddAuthorization();
            authContext.SetAuthorized("TEST USER");
            authContext.SetRoles(RoleConstants.DATAHUB_ROLE_ADMIN, $"{workspace.Project_Acronym_CD}{RoleConstants.WORKSPACE_LEAD_SUFFIX}");

            Services.AddSingleton(mockAuthorizationPolicyProvider);
            Services.AddMudServices();

            RenderTree.Add<CommonMudProviders>();

            var module = JSInterop.SetupModule("./_content/Datahub.Core/Components/DHMarkdown.razor.js");
            JSInterop.Setup<BunitJSInterop>("import", "./_content/Datahub.Core/Components/DHMarkdown.razor.js")
                .SetResult(module);
            module.SetupVoid("styleCodeblocks");
            JSInterop.Mode = JSRuntimeMode.Loose;
            var workspaceUsersPage = Render<WorkspaceUsersPage>(parameterCollection =>
                parameterCollection.Add(p => p.WorkspaceAcronym, Testing.WorkspaceAcronym));
            
            workspaceUsersPage.Should().NotBeNull();

            scenarioContext[WORKSPACE_USERS_PAGE_CTX_KEY] = workspaceUsersPage;
        }

        private IRenderedComponent<WorkspaceUsersPage> GetWorkspaceUserPageFromContext()
        {
            var userPage = scenarioContext[WORKSPACE_USERS_PAGE_CTX_KEY] as IRenderedComponent<WorkspaceUsersPage>;
            userPage.Should().NotBeNull();
            return userPage!;
        }

        private static IRenderedComponent<MudTr>? FindUserTableRowWithGivenEmail(IRenderedComponent<WorkspaceUsersPage> workspaceUsersPage, string userEmail)
        {
            var usersTable = workspaceUsersPage.FindComponent<MudTable<UserRoleLinks>>();
            var rows = usersTable.FindComponents<MudTr>();
            var userRow = rows.FirstOrDefault(r => r.Instance.Item is UserRoleLinks user && user.PortalUser.Email == userEmail);
            return userRow;
        }

        private static IRenderedComponent<MudCheckBox<bool>> FindDataStewardCheckboxInRow(IRenderedComponent<MudTr> userRow)
        {
            // currently there's only one checkbox, but to be safe we'll make sure to get the right one
            var checkboxes = userRow.FindComponents<MudCheckBox<bool>>();
            var dataStewardCheckbox = checkboxes.FirstOrDefault(c => c.Instance.Tag is string tagStr && tagStr == "DataSteward");
            dataStewardCheckbox.Should().NotBeNull();
            return dataStewardCheckbox!;
        }

        private static IRenderedComponent<ProjectMembersRoleSelect> FindProjectMemberRoleSelector(IRenderedComponent<MudTr> userRow)
        {
            // there should only ever be one role selector component in each user row
            var roleSelector = userRow.FindComponent<ProjectMembersRoleSelect>();
            roleSelector.Should().NotBeNull();
            return roleSelector!;
        }

        private static IRenderedComponent<MudButton>? FindSaveChangesButton(IRenderedComponent<WorkspaceUsersPage> workspaceUsersPage)
        {
            var allButtons = workspaceUsersPage.FindComponents<MudButton>();
            var saveChangesButton = allButtons.FirstOrDefault(b => b.Instance.Tag is string tagStr && tagStr == "SaveChanges");
            return saveChangesButton;
        }

        private async Task ToggleDataStewardRole(string email, bool desiredStatus)
        {
            var usersPage = GetWorkspaceUserPageFromContext();
            var userRow = FindUserTableRowWithGivenEmail(usersPage, email);
            userRow.Should().NotBeNull();

            var dsCheck = FindDataStewardCheckboxInRow(userRow!);
            dsCheck.Instance.Disabled.Should().BeFalse();
            dsCheck.Instance.Value.Should().Be(!desiredStatus);

            await usersPage.InvokeAsync(async () => await dsCheck.Instance.ValueChanged.InvokeAsync(desiredStatus));

            usersPage.Render();
        }

        [When("the user sets the Data Steward role for the user with email {string}")]
        public async Task WhenTheUserSetsTheDataStewardRoleForTheUserWithEmail(string email)
        {
            await ToggleDataStewardRole(email, true);
        }

        [When("the user removes the Data Steward role from the user with email {string}")]
        public async Task WhenTheUserRemovesTheDataStewardRoleFromTheUserWithEmail(string email)
        {
            await ToggleDataStewardRole(email, false);
        }

        [When("the Save Changes button is visible")]
        public void WhenTheSaveChangesButtonIsVisible()
        {
            var usersPage = GetWorkspaceUserPageFromContext();
            var saveChangesButton = FindSaveChangesButton(usersPage);
            saveChangesButton.Should().NotBeNull();
        }

        [When("the user clicks the \"Save\" button")]
        public async Task WhenTheUserClicksTheSaveButton()
        {
            var usersPage = GetWorkspaceUserPageFromContext();
            var saveChangesButton = FindSaveChangesButton(usersPage);
            saveChangesButton.Should().NotBeNull();

            await usersPage.InvokeAsync(async () => await saveChangesButton!.Instance.OnClick.InvokeAsync());

            usersPage.Render();
        }

        [Then("user with email {string} should appear on the page")]
        public void ThenTheUserWithEmailShouldAppearOnThePage(string email)
        {
            var usersPage = GetWorkspaceUserPageFromContext();
            var userRow = FindUserTableRowWithGivenEmail(usersPage, email);
            userRow.Should().NotBeNull();
        }

        [Then("user with email {string} should not appear on the page")]
        public void ThenTheUserWithEmailShouldNotAppearOnThePage(string email)
        {
            var usersPage = GetWorkspaceUserPageFromContext();
            var userRow = FindUserTableRowWithGivenEmail(usersPage, email);
            userRow.Should().BeNull();
        }

        private void UserShouldHaveTheGivenDataStewardStatus(string email, bool status)
        {
            var usersPage = GetWorkspaceUserPageFromContext();
            var userRow = FindUserTableRowWithGivenEmail(usersPage, email);
            userRow.Should().NotBeNull();

            var user = userRow!.Instance.Item as UserRoleLinks;
            user.Should().NotBeNull();
            user!.IsDataSteward.Should().Be(status);
        }

        [Then("the user with email {string} should have the Data Steward role")]
        [Given("the user with email {string} has the Data Steward role")]
        public void ThenTheUserWithEmailShouldHaveTheDataStewardRole(string email)
        {
            UserShouldHaveTheGivenDataStewardStatus(email, true);
        }

        [Then("the user with email {string} should not have the Data Steward role")]
        [Given("the user with email {string} doesn't have the Data Steward role")]
        public void ThenTheUserWithEmailShouldNotHaveTheDataStuartRole(string email)
        {
            UserShouldHaveTheGivenDataStewardStatus(email, false);
        }

        private void DataStewardCheckboxEnabledStatus(string email, bool enabled)
        {
            var usersPage = GetWorkspaceUserPageFromContext();
            var userRow = FindUserTableRowWithGivenEmail(usersPage, email);
            userRow.Should().NotBeNull();

            var dsCheckbox = FindDataStewardCheckboxInRow(userRow!);
            dsCheckbox.Should().NotBeNull();
            dsCheckbox.Instance.Disabled.Should().NotBe(enabled);
        }

        [Then("the Data Steward checkbox should be enabled for user {string}")]
        [Given("the Data Steward checkbox is enabled for user {string}")]
        public void ThenTheDataStewardCheckboxShouldBeEnabledForUser(string email)
        {
            DataStewardCheckboxEnabledStatus(email, true);
        }

        [Then("the Data Steward checkbox should be disabled for user {string}")]
        [Given("the Data Steward checkbox is disabled for user {string}")]
        public void ThenTheDataStewardCheckboxShouldBeDisabledForUser(string email)
        {
            DataStewardCheckboxEnabledStatus(email, false);
        }

        private async Task UpdateGivenUsersRole(string email, int roleId)
        {
            var usersPage = GetWorkspaceUserPageFromContext();
            var userRow = FindUserTableRowWithGivenEmail(usersPage, email);
            userRow.Should().NotBeNull();

            var roleSelector = FindProjectMemberRoleSelector(userRow!);
            await usersPage.InvokeAsync(async () => await roleSelector.Instance.OnRoleChanged.InvokeAsync(roleId));

            usersPage.Render();
        }

        [When("the user updates the role of user with email {string} to Guest")]
        public async Task WhenTheUserUpdatesSomeonesRoleToGuest(string email)
        {
            await UpdateGivenUsersRole(email, (int)Project_Role.RoleNames.Guest);
        }

        [When("the user updates the role of user with email {string} to Collaborator")]
        public async Task WhenTheUserUpdatesSomeonesRoleToCollaborator(string email)
        {
            await UpdateGivenUsersRole(email, (int)Project_Role.RoleNames.Collaborator);
        }

        [Given(@"I have an existing workspace lead")]
        public async Task GivenIHaveAnExistingWorkspaceLead()
        {

            await GivenTheUserIsOnTheWorkspaceUsersPage();
            // Retrieve the already-registered NSubstitute mock from DI
            // (Set up previously in GivenTheUserIsOnTheWorkspaceUsersPage() or wherever you configure your services.)
            var projectUserManagementService = Services.GetService<IProjectUserManagementService>();

            var page = GetWorkspaceUserPageFromContext();
            // Substitute behavior: return a user who is a Workspace Lead
            projectUserManagementService!.GetProjectUsersAsync(Arg.Any<string>())
                .Returns(new List<UserRoleLinks>
                {
            new UserRoleLinks
            {
                RoleId = (int)Project_Role.RoleNames.WorkspaceLead,
                PortalUser = new PortalUser { EntraUser = new() { GraphGuid = Guid.NewGuid().ToString(), PortalUser = null! }, Email = "lead@test.com" }
            }
                });            
        }

        [When(@"I add another lead")]
        public void WhenIAddAnotherLead()
        {
            var page = GetWorkspaceUserPageFromContext();
            
            // Get the instance itself, not the type
            var instance = page.Instance;
            
            // Access the field from the instance
            var fieldUsersToAdd = instance.GetType().GetField("_usersToAdd", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldUsersToAdd.Should().NotBeNull("The _usersToAdd field should exist in the component");
            
            var usersToAdd = (List<ProjectUserAddEntraUserCommand>)fieldUsersToAdd.GetValue(instance);
            usersToAdd.Should().NotBeNull("The _usersToAdd list should be initialized");

            usersToAdd.Add(new ProjectUserAddEntraUserCommand
            {
                Email = "second_lead@test.com",
                RoleId = (int)Project_Role.RoleNames.WorkspaceLead
            });

            // Run validation
            var validateMethod = instance.GetType().GetMethod("ValidateWorkspaceRules", BindingFlags.NonPublic | BindingFlags.Instance);
            validateMethod.Should().NotBeNull("The ValidateWorkspaceRules method should exist");
            validateMethod.Invoke(instance, null);
        }

        [Then(@"a validation error is shown preventing multiple leads")]
        public void ThenAValidationErrorIsShownPreventingMultipleLeads()
        {
            var page = GetWorkspaceUserPageFromContext();
            
            // Get the instance itself, not the type
            var instance = page.Instance;
            
            // Check the private field for error message
            var errorMessageField = instance.GetType().GetField("_validationErrorMessage", BindingFlags.NonPublic | BindingFlags.Instance);
            errorMessageField.Should().NotBeNull("The _validationErrorMessage field should exist in the component");
            
            var validationMessage = (string)errorMessageField.GetValue(instance);
            validationMessage.Should().NotBeNull("There should be a validation error message");
            validationMessage.Should().Contain("You cannot have more than one workspace lead.");
        }
    }
}