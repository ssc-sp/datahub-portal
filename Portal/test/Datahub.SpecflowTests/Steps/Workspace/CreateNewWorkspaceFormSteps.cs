using GcdsWrapper.Blazor;
using System.Globalization;
using Bunit;
using Bunit.TestDoubles;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.Metadata;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.Subscriptions;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Onboarding;
using Datahub.Core.Model.Users;
using Datahub.Core.Services.CatalogSearch;
using Datahub.Infrastructure.Offline;
using Datahub.Infrastructure.Services;
using Datahub.Portal.Pages.Workspace;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using Reqnroll;
using System.Threading.Tasks;

namespace Datahub.SpecflowTests.Steps.Workspace
{
    [Binding]
    public class CreateNewWorkspaceFormSteps(
        ScenarioContext scenarioContext,
        IWebHostEnvironment hostingEnvironment
        ): BunitTestSteps
    {
        private const string RelativePathToSrc = "../../../../../src";

        private const string CREATE_WORKSPACE_PAGE_CTX_KEY = "createWorkspaceForm";
        private const string WORKSPACE_ACRONYM_CTX_KEY = "workspaceAcronym";
        private const string CBR_ID_CTX_KEY = "cbrId";
        private const string WORKSPACE_BUDGET_CTX_KEY = "workspaceBudget";

        private IUserInformationService userInfoService;
        private DatahubPortalConfiguration portalConfiguration;

        // Per-scenario users to avoid shared static state when tests run in parallel
        private PortalUser cbrOwnerUser;
        private PortalUser otherWorkspaceLeadUser;

        private async Task SetupServices()
        {
            Services.AddSingleton(hostingEnvironment);

            portalConfiguration = new DatahubPortalConfiguration()
            {
                CultureSettings =
                {
                    ResourcesPath = $"{RelativePathToSrc}/Datahub.Portal/i18n",
                    AdditionalResourcePaths = []
                },
                AzureAd =
                {
                    ClientId = Guid.NewGuid().ToString(),
                    TenantId = Guid.NewGuid().ToString(),
                    InfraClientId = Guid.NewGuid().ToString(),
                    InfraClientSecret = Guid.NewGuid().ToString()
                }
            };
            var hostEnv = Substitute.For<IWebHostEnvironment>();
            hostEnv.EnvironmentName.Returns("Development");
            Services.AddScoped(_ => hostEnv);

            Services.AddSingleton(portalConfiguration);

            Services.AddMudServices();
            Services.AddSingleton(Substitute.For<ICultureService>());
            Services.AddDatahubLocalization(portalConfiguration);

            JSInterop.SetupMudBlazor();
            JSInterop.Mode = JSRuntimeMode.Loose;
        }

        private static IWorkspaceCreationService CreateMockedWorkspaceCreationService(
            DatahubPortalConfiguration datahubPortalConfiguration,
            IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
            IUserInformationService userInformationService)
        {
            var logger = Substitute.For<ILogger<WorkspaceCreationService>>();
            var serviceAuthManager = Substitute.For<IServiceAuthManager>();
            var workspaceVersionService = Substitute.For<IWorkspaceVersionService>();
            var auditingService = Substitute.For<IDatahubAuditingService>();
            var azureSubService = Substitute.For<IDatahubAzureSubscriptionService>();
            var catalogSearch = Substitute.For<IDatahubCatalogSearch>();
            var metadataService = Substitute.For<IMetadataBrokerService>();
            var resourceMessagingService = Substitute.For<IResourceMessagingService>();
            azureSubService.NextSubscriptionAsync()
                .Returns(new Core.Model.Subscriptions.DatahubAzureSubscription() { Id = 1, SubscriptionId = "test-subscription-id", TenantId = "test-tenant-id", SubscriptionName = "Test Subscription" });

            var mockedWorkspaceCreationService = Substitute.ForPartsOf<WorkspaceCreationService>(
                        datahubPortalConfiguration,
                        dbContextFactory,
                        logger,
                        serviceAuthManager,
                        userInformationService,
                        workspaceVersionService,
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

        private IRenderedComponent<CreateWorkspacePage> GetCreateWorkspacePageFromContext()
        {
            var createWorkspacePage = scenarioContext[CREATE_WORKSPACE_PAGE_CTX_KEY] as IRenderedComponent<CreateWorkspacePage>;
            createWorkspacePage.Should().NotBeNull();
            return createWorkspacePage!;
        }

        private static IRenderedComponent<CreateWorkspaceForm>? FindCreateWorkspaceForm(IRenderedComponent<CreateWorkspacePage> page)
            => page.FindComponents<CreateWorkspaceForm>().FirstOrDefault();

        private static IRenderedComponent<MudText>? FindNonCbrErrorMessage(IRenderedComponent<CreateWorkspacePage> page)
            => page.FindComponents<MudText>().FirstOrDefault(r => r.Instance.Tag is string tag && tag == CreateWorkspacePage.NON_CBR_OWNER_MESSAGE_TAG);

        private static IRenderedComponent<GcdsButton> FindAction(IRenderedComponent<CreateWorkspacePage> page)
            => page.FindComponents<GcdsButton>().First(r => r.Instance.Id == "workspace-next" || r.Instance.Id == CreateWorkspaceForm.CREATE_WORKSPACE_BUTTON_TAG);

        private static IRenderedComponent<GcdsInput> FindInput(IRenderedComponent<CreateWorkspaceForm> form, string id)
            => form.FindComponents<GcdsInput>().Single(r => r.Instance.Id == id);

        //[Given("authorization as a {string} for the workspace creation page")]
        //public void GivenAuthorizationAsAUserTypeForTheWorkspaceCreationPage(string userType)
        //{
        //    // Create per-scenario users
        //    cbrOwnerUser = CommonCbrTestUtils.CreateCbrOwnerUser();
        //    otherWorkspaceLeadUser = CommonCbrTestUtils.CreateOtherWorkspaceLead();

        //    var isCbrOwner = userType.Equals("CBR Owner", StringComparison.OrdinalIgnoreCase);
        //    var currentUser = isCbrOwner ? cbrOwnerUser : otherWorkspaceLeadUser;

        //    userInfoService = Substitute.For<IUserInformationService>();
        //    userInfoService.GetCurrentPortalUserAsync().Returns(currentUser);
        //    Services.AddSingleton(userInfoService);

        //    CommonCbrTestUtils.AddLoggedInUserAuthorization(this, Testing.WorkspaceAcronym, isCbrOwner, false);
        //}

        [Given("authorization as a CBR Owner for the workspace creation page")]
        public void GivenAuthorizationAsACbrOwnerForTheWorkspaceCreationPage()
        {
            // Create per-scenario users
            cbrOwnerUser = CommonCbrTestUtils.CreateCbrOwnerUser();
            otherWorkspaceLeadUser = CommonCbrTestUtils.CreateOtherWorkspaceLead();

            userInfoService = Substitute.For<IUserInformationService>();
            userInfoService.GetCurrentPortalUserAsync().Returns(cbrOwnerUser);
            Services.AddSingleton(userInfoService);

            CommonCbrTestUtils.AddLoggedInUserAuthorization(this, Testing.WorkspaceAcronym, true, false);
        }

        [Given("authorization as a non-CBR owner for the workspace creation page")]
        public void GivenAuthorizationAsANonCbrOwnerForTheWorkspaceCreationPage()
        {
            // Create per-scenario users
            cbrOwnerUser = CommonCbrTestUtils.CreateCbrOwnerUser();
            otherWorkspaceLeadUser = CommonCbrTestUtils.CreateOtherWorkspaceLead();

            userInfoService = Substitute.For<IUserInformationService>();
            userInfoService.GetCurrentPortalUserAsync().Returns(otherWorkspaceLeadUser);
            Services.AddSingleton(userInfoService);

            CommonCbrTestUtils.AddLoggedInUserAuthorization(this, Testing.WorkspaceAcronym, false, false);
        }

        [Given("a workspace creation page")]
        public async Task GivenAWorkspaceCreationPage()
        {
            await SetupServices();

            // Ensure users are initialized in case this step is used directly
            cbrOwnerUser ??= CommonCbrTestUtils.CreateCbrOwnerUser();
            otherWorkspaceLeadUser ??= CommonCbrTestUtils.CreateOtherWorkspaceLead();

            var dbContextFactory = await CommonCbrTestUtils.GenerateCbrTestDatabase(cbrOwnerUser, otherWorkspaceLeadUser);
            Services.AddSingleton<IDbContextFactory<DatahubProjectDBContext>>(dbContextFactory);

            var workspaceCreationService = CreateMockedWorkspaceCreationService(portalConfiguration, dbContextFactory, userInfoService);
            Services.AddSingleton(workspaceCreationService);

            var authenticatedNewWorkspacePage = Render<CascadingAuthenticationState>(parameters =>
            {
                parameters.AddChildContent<MudPopoverProvider>();
                parameters.AddChildContent<CreateWorkspacePage>();
            });

            var workspaceCreationPage = authenticatedNewWorkspacePage.FindComponent<CreateWorkspacePage>();

            scenarioContext[CREATE_WORKSPACE_PAGE_CTX_KEY] = workspaceCreationPage;
        }

        private async Task CheckWorkspaceCreationFormValidity(bool valid)
        {
            var page = GetCreateWorkspacePageFromContext();
            var step = page.FindComponent<GcdsStepper>().Instance.CurrentStep;
            await page.InvokeAsync(FindAction(page).Instance.OnClick.InvokeAsync);
            page.FindComponent<GcdsStepper>().Instance.CurrentStep.Should().Be(valid ? step + 1 : step);
        }
        [Then("the workspace creation form should be invalid")]
        public Task ThenTheWorkspaceCreationFormShouldBeInvalid() => CheckWorkspaceCreationFormValidity(false);
        [Then("the workspace creation form should be valid")]
        public Task ThenTheWorkspaceCreationFormShouldBeValid() => CheckWorkspaceCreationFormValidity(true);
        private void CheckCreateWorkspaceButtonEnabledStatus(bool enabled)
        {
            var page = GetCreateWorkspacePageFromContext();
            var buttons = page.FindComponents<GcdsButton>().Where(b => b.Instance.Id == CreateWorkspaceForm.CREATE_WORKSPACE_BUTTON_TAG).ToList();
            if (enabled) buttons.Single().Instance.Disabled.Should().BeFalse();
            else buttons.Should().BeEmpty();
        }
        [Then("the create workspace button should be enabled")]
        public void ThenTheCreateWorkspaceButtonShouldBeEnabled() => CheckCreateWorkspaceButtonEnabledStatus(true);
        [Then("the create workspace button should be disabled")]
        public void ThenTheCreateWorkspaceButtonShouldBeDisabled() => CheckCreateWorkspaceButtonEnabledStatus(false);
        [When("the user enters a workspace title in the creation form")]
        public async Task WhenTheUserEntersAWorkspaceTitleInTheCreationForm()
        {
            var page = GetCreateWorkspacePageFromContext();
            var form = FindCreateWorkspaceForm(page)!;
            await page.InvokeAsync(() => FindInput(form, CreateWorkspaceForm.WORKSPACE_TITLE_INPUT_TAG).Instance.ValueChanged.InvokeAsync("Test Workspace"));
            await page.InvokeAsync(form.FindComponents<GcdsButton>().First().Instance.OnClick.InvokeAsync);
            scenarioContext[WORKSPACE_ACRONYM_CTX_KEY] = FindInput(form, CreateWorkspaceForm.WORKSPACE_ACRONYM_INPUT_TAG).Instance.Value!;
            await page.InvokeAsync(FindAction(page).Instance.OnClick.InvokeAsync);
        }
        [When("the user selects a CBR from the dropdown in the workspace creation form")]
        public async Task WhenTheUserSelectsACbrFromTheDropdownInTheWorkspaceCreationForm()
        {
            var page = GetCreateWorkspacePageFromContext();
            var select = page.FindComponent<GcdsSelect>();
            var id = select.Find("option").GetAttribute("value")!;
            scenarioContext[CBR_ID_CTX_KEY] = int.Parse(id, CultureInfo.InvariantCulture);
            await page.InvokeAsync(() => select.Instance.ValueChanged.InvokeAsync(id));
        }
        [When("the user enters a budget of {decimal} in the workspace creation form")]
        public async Task WhenTheUserEntersABudgetInTheWorkspaceCreationForm(decimal budget)
        {
            var page = GetCreateWorkspacePageFromContext();
            scenarioContext[WORKSPACE_BUDGET_CTX_KEY] = budget;
            await page.InvokeAsync(() => FindInput(FindCreateWorkspaceForm(page)!, CreateWorkspaceForm.BUDGET_INPUT_TAG).Instance.ValueChanged.InvokeAsync(budget.ToString(CultureInfo.CurrentCulture)));
        }

        private void CheckIfFormIsShown(bool shown)
        {
            var workspaceCreationPage = GetCreateWorkspacePageFromContext();
            var form = FindCreateWorkspaceForm(workspaceCreationPage);
            if (shown)
            {
                form.Should().NotBeNull();
            }
            else
            {
                form.Should().BeNull();
            }
        }

        [Then("the workspace creation form should be shown")]
        public void ThenTheWorkspaceCreationFormShouldBeShown()
        {
            CheckIfFormIsShown(true);
        }

        [Then("the workspace creation form should not be shown")]
        public void ThenTheWorkspaceCreationFormShouldNotBeShown()
        {
            CheckIfFormIsShown(false);
        }

        private void CheckIfNonCbrErrorIsShown(bool shown)
        {
            var page = GetCreateWorkspacePageFromContext();
            var errorMsg = FindNonCbrErrorMessage(page);
            if (shown)
            {
                errorMsg.Should().NotBeNull();
            } 
            else
            {
                errorMsg.Should().BeNull();
            }
        }

        [Then("the error message restricting workspace creation to CBR owners should be shown")]
        public void ThenTheErrorMessageRestrictingWorkspaceCreationToCbrOwnersShouldBeShown()
        {
            CheckIfNonCbrErrorIsShown(true);
        }

        [Then("the error message restricting workspace creation to CBR owners should not be shown")]
        public void ThenTheErrorMessageRestrictingWorkspaceCreationToCbrOwnersShouldNotBeShown()
        {
            CheckIfNonCbrErrorIsShown(false);
        }

        [When("the user clicks the create workspace button")]
        public async Task WhenTheUserClicksTheCreateWorkspaceButton()
        {
            var page = GetCreateWorkspacePageFromContext();
            var form = FindCreateWorkspaceForm(page);
            form.Should().NotBeNull();

            if (page.FindComponent<GcdsStepper>().Instance.CurrentStep == 2)
                await page.InvokeAsync(FindAction(page).Instance.OnClick.InvokeAsync);
            var button = FindAction(page);
            button.Instance.Disabled.Should().BeFalse();
            await page.InvokeAsync(button.Instance.OnClick.InvokeAsync);
        }

        [When("the user enters a title of {int} characters and acronym {string}")]
        public async Task EnterIdentity(int length, string acronym)
        {
            var page = GetCreateWorkspacePageFromContext();
            var form = FindCreateWorkspaceForm(page)!;
            await page.InvokeAsync(() => FindInput(form, CreateWorkspaceForm.WORKSPACE_TITLE_INPUT_TAG).Instance.ValueChanged.InvokeAsync(new string('x', length)));
            await page.InvokeAsync(() => FindInput(form, CreateWorkspaceForm.WORKSPACE_ACRONYM_INPUT_TAG).Instance.ValueChanged.InvokeAsync(acronym));
        }
        [When("the user goes back to workspace identity")]
        public async Task BackToIdentity()
        {
            var page = GetCreateWorkspacePageFromContext();
            var back = page.FindComponents<GcdsButton>().Single(b => b.Find("gcds-button").TextContent.Trim() == "Back");
            await page.InvokeAsync(back.Instance.OnClick.InvokeAsync);
        }
        [Then("the workspace acronym should be {string}")]
        public void CheckAcronym(string acronym)
            => FindInput(FindCreateWorkspaceForm(GetCreateWorkspacePageFromContext())!, CreateWorkspaceForm.WORKSPACE_ACRONYM_INPUT_TAG).Instance.Value.Should().Be(acronym);

        [When("the user continues to feature interests")]
        public async Task ContinueToFeatures()
        {
            var page = GetCreateWorkspacePageFromContext();
            await page.InvokeAsync(FindAction(page).Instance.OnClick.InvokeAsync);
            page.FindComponent<GcdsStepper>().Instance.CurrentStep.Should().Be(3);
        }
        [When("the user selects all feature interests")]
        public async Task SelectAllFeatures()
        {
            var page = GetCreateWorkspacePageFromContext();
            await page.InvokeAsync(() => page.FindComponent<GcdsCheckboxes>().Instance.ValueChanged.InvokeAsync(new[] { "Other", "Collaboration", "Storage", "Analytics" }));
        }
        [When("the user enters {int} characters for other feature interests")]
        public async Task EnterOther(int count)
        {
            var page = GetCreateWorkspacePageFromContext();
            await page.InvokeAsync(() => page.FindComponent<GcdsTextarea>().Instance.ValueChanged.InvokeAsync(new string('x', count)));
        }
        [Then("feature interest overflow should be {word}")]
        public void CheckOverflow(string expected)
        {
            var page = GetCreateWorkspacePageFromContext();
            FindAction(page).Instance.Disabled.Should().Be(expected == "shown");
            (page.FindComponent<GcdsCheckboxes>().Instance.ErrorMessage is not null).Should().Be(expected == "shown");
        }
        [When("the user deselects Other")]
        public async Task DeselectOther()
        {
            var page = GetCreateWorkspacePageFromContext();
            await page.InvokeAsync(() => page.FindComponent<GcdsCheckboxes>().Instance.ValueChanged.InvokeAsync(new[] { "Storage", "Analytics", "Collaboration" }));
            page.FindComponents<GcdsTextarea>().Should().BeEmpty();
        }
        [Then("the other feature text should contain {int} characters")]
        public void CheckOther(int count) => GetCreateWorkspacePageFromContext().FindComponent<GcdsTextarea>().Instance.Value.Should().HaveLength(count);
        [Then("the saved feature interests should have {int} characters")]
        public async Task CheckSavedInterests(int count)
        {
            await using var db = await Services.GetRequiredService<IDbContextFactory<DatahubProjectDBContext>>().CreateDbContextAsync();
            var details = await db.ProjectCreationDetails.SingleAsync();
            details.InterestedFeatures.Should().HaveLength(count);
            if (count > 0) details.InterestedFeatures.Should().StartWith("Storage, Analytics, Collaboration, Other: ");
        }
        [When("the user enters a malformed workspace budget")]
        public async Task MalformedBudget()
        {
            var page = GetCreateWorkspacePageFromContext();
            await page.InvokeAsync(() => FindInput(FindCreateWorkspaceForm(page)!, CreateWorkspaceForm.BUDGET_INPUT_TAG).Instance.ValueChanged.InvokeAsync("invalid"));
        }

        [Then("the workspace should be created with the correct parent CBR ID and budget")]
        public async Task ThenTheWorkspaceShouldBeCreatedWithTheCorrectParentCbrIdAndBudget()
        {
            var dbContextFactory = Services.GetService<IDbContextFactory<DatahubProjectDBContext>>();

            var workspaceAcronym = scenarioContext[WORKSPACE_ACRONYM_CTX_KEY] as string;
            var cbrId = scenarioContext[CBR_ID_CTX_KEY] as int?;
            var workspaceBudget = scenarioContext[WORKSPACE_BUDGET_CTX_KEY] as decimal?;

            await using var ctx = await dbContextFactory!.CreateDbContextAsync();
            var createdWorkspace = await ctx.Projects.FirstOrDefaultAsync(w => w.Project_Acronym_CD == workspaceAcronym);

            createdWorkspace.Should().NotBeNull();
            cbrId.Should().NotBeNull();
            createdWorkspace!.ParentGCHostingBudgetId.Should().Be(cbrId);
            workspaceBudget.Should().NotBeNull();
            createdWorkspace!.Project_Budget.Should().Be(workspaceBudget);
        }

        [Then("the navigation manager should be redirected to the created workspace")]
        public void ThenTheNavigationManagerShouldBeRedirectedToTheCreatedWorkspace()
        {
            var navManager = Services.GetService<NavigationManager>();
            var bunitNavObject = navManager as Bunit.TestDoubles.BunitNavigationManager;
            bunitNavObject.Should().NotBeNull();

            var workspaceAcronym = scenarioContext[WORKSPACE_ACRONYM_CTX_KEY] as string;

            var workspaceUrl = $"/w/{workspaceAcronym}";
            bunitNavObject!.History.Count.Should().Be(1);
            bunitNavObject!.History.First().Uri.Should().Be(workspaceUrl);
        }
    }
}
