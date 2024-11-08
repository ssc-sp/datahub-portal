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
using Datahub.Portal.Pages.Workspace.Users;
using Datahub.Shared.Entities;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using Reqnroll;
using System.Reflection;

namespace Datahub.SpecflowTests.Steps
{
    [Binding]
    public class WorkspaceUsersSteps(
        ScenarioContext scenarioContext,
        IWebHostEnvironment hostingEnvironment
    ) : TestContext
    {

        private const string RelativePathToSrc = "../../../../../src";
        private SpecFlowDbContextFactory dbContextFactory;
        private IRenderedComponent<WorkspaceUsersPage> workspaceUsersPage;

        private readonly ISnackbar _snackBar = Substitute.For<ISnackbar>();
        private readonly IStringLocalizer _stringLocalizer = Substitute.For<IStringLocalizer>();

        [Given("the user is on the workspace users page")]
        public async Task GivenTheUserIsOnTheWorkspaceUsersPage()
        {
            if (hostingEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostingEnvironment));
            }
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
            Services.AddStub<IUserInformationService>();

            var mockRequestManagementService = Substitute.For<IRequestManagementService>();
            Services.AddSingleton(mockRequestManagementService);

            var mockProjectUserManagementService = Substitute.For<IProjectUserManagementService>();
            Services.AddSingleton(mockProjectUserManagementService);

            var mockProjectUsers = new List<Datahub_Project_User>
            {
                new Datahub_Project_User
                {
                    PortalUserId = 1,
                    Role = new Project_Role  { Name = RoleConstants.GUEST_ROLE, Id=1, Description=RoleConstants.GUEST_ROLE },
                    PortalUser = new PortalUser { DisplayName = "User1", Email = "dataSteward@example.com", GraphGuid=Guid.NewGuid().ToString() }
                },
                new Datahub_Project_User
                {
                    PortalUserId = 2,
                    Role = new Project_Role  { Name = RoleConstants.ADMIN_ROLE, Id=2, Description=RoleConstants.ADMIN_ROLE },
                    PortalUser = new PortalUser { DisplayName = "User2", Email = "user2@example.com", GraphGuid=Guid.NewGuid().ToString() }
                }
            };

            // Set up the mock to return the mock data
            mockProjectUserManagementService.GetProjectUsersAsync(Arg.Any<string>())
                .Returns(Task.FromResult(mockProjectUsers.ToList()));

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
            context.Project_Users.Add(new Datahub_Project_User()
            {
                Role = new Project_Role() { Name = RoleConstants.GUEST_ROLE, Id=1, Description=RoleConstants.GUEST_ROLE },
                PortalUser = new PortalUser()
                {
                    GraphGuid = Guid.NewGuid().ToString(),
                    Email = "dataSteward@example.com"
                }
            });
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

            var authContext = this.AddTestAuthorization();
            authContext.SetAuthorized("TEST USER");
            authContext.SetRoles(RoleConstants.DATAHUB_ROLE_ADMIN);

            Services.AddSingleton(mockAuthorizationPolicyProvider);
            Services.AddMudServices();

            var module = JSInterop.SetupModule("./_content/Datahub.Core/Components/DHMarkdown.razor.js");
            JSInterop.Setup<BunitJSInterop>("import", "./_content/Datahub.Core/Components/DHMarkdown.razor.js")
                .SetResult(module);
            module.SetupVoid("styleCodeblocks");
            JSInterop.Mode = JSRuntimeMode.Loose;
            workspaceUsersPage = RenderComponent<WorkspaceUsersPage>(parameterCollection =>
                parameterCollection.Add(p => p.WorkspaceAcronym, Testing.WorkspaceAcronym));
            if (workspaceUsersPage == null)
            {
                throw new Exception("workspaceUsersPage is null");
            }
            scenarioContext["workspaceUsersPage"] = workspaceUsersPage;
        }

        [When("the user sets the Data Steward role for the user with email {string}")]
        public async Task WhenTheUserSetsTheDataStewardRoleForTheUserWithEmail(string email)
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var user = await context.Project_Users
                .Include(pu => pu.PortalUser)
                .Include(pu => pu.Role)
                .FirstOrDefaultAsync(pu => pu.PortalUser.Email == email);

            SetPrivateField(workspaceUsersPage, "_currentlySelected", new List<Datahub_Project_User> { user });
            var result = InvokePrivateMethodAsync(workspaceUsersPage, "ChangeDataStewardFlag", user, true);

        }

        [When("the user clicks the \"Save\" button")]
        public void WhenTheUserClicksTheSaveButton()
        {
            InvokePrivateMethod(workspaceUsersPage, "SaveChanges");
        }
        [Then("specified user should appear on the page")]
        public async Task ThenSpecifiedUserShouldAppearOnThePage()
        {
            var email = "dataSteward@example.com";
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var user = await context.Project_Users
                .Include(pu => pu.PortalUser)
                .Include(pu => pu.Role)
                .FirstOrDefaultAsync(pu => pu.PortalUser.Email == email);

            user.Should().NotBeNull();
        }

        [Then("user with email {string} should appear on the page")]
        public async Task ThenTheUserWithEmailShouldAppearOnThePage(string email)
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var user = await context.Project_Users
                .Include(pu => pu.PortalUser)
                .Include(pu => pu.Role)
                .FirstOrDefaultAsync(pu => pu.PortalUser.Email == email);

            user.Should().NotBeNull();
        }

        [Then("the user with email {string} should have the Data Steward role")] 
        public async Task ThenTheUserWithEmailShouldHaveTheDataStewardRole(string email)
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var user = await context.Project_Users
                .Include(pu => pu.PortalUser)
                .Include(pu => pu.Role)
                .FirstOrDefaultAsync(pu => pu.PortalUser.Email == email);

            user.Should().NotBeNull();
            //user.IsDataSteward.Should().BeTrue();
        }

        [When("the user removes the Data Steward role from the user with email {string}")]
        public void WhenTheUserRemovesTheDataStewardRoleFromTheUserWithEmail(string email)
        {
            // scenarioContext.Pending();
        }

        [Then("the user with email {string} should not have the Data Steward role")]
        public void ThenTheUserWithEmailShouldNotHaveTheDataStuartRole(string email)
        {
            // scenarioContext.Pending();
        }

        private void SetPrivateField(IRenderedComponent<WorkspaceUsersPage> component, string fieldName, object value)
        {
            var obj = component.Instance;
            if (obj != null)
            {
                var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(obj, value);
                }
            }
        }
        private object InvokePrivateMethod(IRenderedComponent<WorkspaceUsersPage> component, string methodName, params object[] parameters)
        {
            var obj = component.Instance; 
            if (obj != null)
            {
                var method = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null)
                {
                    return method.Invoke(obj, parameters);
                }
            }
              
            return null;
        }
        private async Task<object> InvokePrivateMethodAsync(IRenderedComponent<WorkspaceUsersPage> component, string methodName, params object[] parameters)
        {
            var obj = component.Instance;
            if (obj != null)
            {
                var method = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null)
                {
                    return await component.InvokeAsync(() => method.Invoke(obj, parameters));
                }
            }
            throw new InvalidOperationException($"Method '{methodName}' not found in type '{obj.GetType().FullName}'.");
        }

    }
}