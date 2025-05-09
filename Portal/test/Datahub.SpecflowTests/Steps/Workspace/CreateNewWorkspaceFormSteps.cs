using Bunit;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.Metadata;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.Subscriptions;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Onboarding;
using Datahub.Core.Services.CatalogSearch;
using Datahub.Infrastructure.Offline;
using Datahub.Infrastructure.Services;
using Datahub.Portal.Pages.Workspace;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps.Workspace
{
    [Binding]
    public class CreateNewWorkspaceFormSteps(
        ScenarioContext scenarioContext,
        IWebHostEnvironment hostingEnvironment
        ):TestContext
    {
        private const string RelativePathToSrc = "../../../../../src";

        private const string CREATE_WORKSPACE_PAGE_CTX_KEY = "createWorkspaceForm";

        private IUserInformationService userInfoService;
        private DatahubPortalConfiguration portalConfiguration;

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

            Services.AddSingleton(portalConfiguration);

            Services.AddMudServices();
            Services.AddDatahubLocalization(portalConfiguration);

            JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
            JSInterop.SetupModule("./_content/Datahub.Portal/Components/SkipLink.razor.js");
            JSInterop.SetupVoid("mudPopover.initialize", _ => true);
            JSInterop.Setup<int>("mudpopoverHelper.countProviders");
            JSInterop.SetupVoid("mudPopover.connect", _ => true);
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
            var metadataService = Substitute.For<IMetadataBrokerService>();

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

        private IRenderedComponent<CreateWorkspacePage> GetCreateWorkspacePageFromContext()
        {
            var createWorkspacePage = scenarioContext[CREATE_WORKSPACE_PAGE_CTX_KEY] as IRenderedComponent<CreateWorkspacePage>;
            createWorkspacePage.Should().NotBeNull();
            return createWorkspacePage!;
        }

        private static IRenderedComponent<MudForm>? FindCreateWorkspaceForm(IRenderedComponent<CreateWorkspacePage> createWorkspacePage)
        {
            var forms = createWorkspacePage.FindComponents<MudForm>();
            var form = forms.FirstOrDefault();
            return form;
        }

        private static IRenderedComponent<MudText>? FindNonCbrErrorMessage(IRenderedComponent<CreateWorkspacePage> createWorkspaceForm)
        {
            var textElements = createWorkspaceForm.FindComponents<MudText>();
            var text = textElements.FirstOrDefault(r => r.Instance.Tag is string tagStr && tagStr == CreateWorkspacePage.NON_CBR_OWNER_MESSAGE_TAG);
            return text;
        }

        private static IRenderedComponent<MudButton> FindCreateWorkspaceButtonInNewWorkspaceForm(IRenderedComponent<CreateWorkspacePage> createWorkspacePage)
        {
            var formButtons = createWorkspacePage.FindComponents<MudButton>();
            var button = formButtons.FirstOrDefault(r => r.Instance.Tag is string tagStr && tagStr == CreateWorkspaceForm.CREATE_WORKSPACE_BUTTON_TAG);
            button.Should().NotBeNull();
            return button!;
        }

        private static IRenderedComponent<MudTextField<string>> FindWorkspaceTitleTextField(IRenderedComponent<MudForm> form)
        {
            var formTextFields = form.FindComponents<MudTextField<string>>();
            var textField = formTextFields.FirstOrDefault(r => r.Instance.Tag is string tagStr && tagStr == CreateWorkspaceForm.WORKSPACE_TITLE_INPUT_TAG);
            textField.Should().NotBeNull();
            return textField!;
        }

        private static IRenderedComponent<MudTextField<string>> FindWorkspaceAcronymTextField(IRenderedComponent<MudForm> form)
        {
            var formTextFields = form.FindComponents<MudTextField<string>>();
            var textField = formTextFields.FirstOrDefault(r => r.Instance.Tag is string tagStr && tagStr == CreateWorkspaceForm.WORKSPACE_ACRONYM_INPUT_TAG);
            textField.Should().NotBeNull();
            return textField!;
        }

        private static IRenderedComponent<MudSelect<GCHostingWorkspaceDetails>> FindCbrDropdownInForm(IRenderedComponent<MudForm> form)
        {
            var formDropdowns = form.FindComponents<MudSelect<GCHostingWorkspaceDetails>>();
            var cbrDropdown = formDropdowns.FirstOrDefault(r => r.Instance.Tag is string tagStr && tagStr == CreateWorkspaceForm.CBR_DROPDOWN_TAG);
            cbrDropdown.Should().NotBeNull();
            return cbrDropdown!;
        }

        private static GCHostingWorkspaceDetails FindFirstNonNullCbr(IRenderedComponent<MudSelect<GCHostingWorkspaceDetails>> cbrDropdown)
        {
            var selectItems = cbrDropdown.FindComponents<MudSelectItem<GCHostingWorkspaceDetails>>();
            var firstNonNullCbr = selectItems.FirstOrDefault(r => r.Instance.Value is not null);
            firstNonNullCbr.Should().NotBeNull();
            return firstNonNullCbr!.Instance.Value!;
        }

        private static IRenderedComponent<MudTextField<decimal>> FindBudgetInput(IRenderedComponent<MudForm> form)
        {
            var textFields = form.FindComponents<MudTextField<decimal>>();
            var budgetInput = textFields.FirstOrDefault(r => r.Instance.Tag is string tagStr && tagStr == CreateWorkspaceForm.BUDGET_INPUT_TAG);
            budgetInput.Should().NotBeNull();
            return budgetInput!;
        }

        [Given("authorization as a CBR Owner for the workspace creation page")]
        public void GivenAuthorizationAsACbrOwnerForTheWorkspaceCreationPage()
        {
            userInfoService = Substitute.For<IUserInformationService>();
            userInfoService.GetCurrentPortalUserAsync().Returns(CommonCbrTestUtils.CbrOwnerUser);
            Services.AddSingleton(userInfoService);

            CommonCbrTestUtils.AddLoggedInUserAuthorization(this, Testing.WorkspaceAcronym, true, false);
        }

        [Given("authorization as a non-CBR owner for the workspace creation page")]
        public void GivenAuthorizationAsANonCbrOwnerForTheWorkspaceCreationPage()
        {
            userInfoService = Substitute.For<IUserInformationService>();
            userInfoService.GetCurrentPortalUserAsync().Returns(CommonCbrTestUtils.OtherWorkspaceLead);
            Services.AddSingleton(userInfoService);

            CommonCbrTestUtils.AddLoggedInUserAuthorization(this, Testing.WorkspaceAcronym, false, false);
        }

        [Given("a workspace creation page")]
        public async Task GivenAWorkspaceCreationPage()
        {
            await SetupServices();

            var dbContextFactory = await CommonCbrTestUtils.GenerateCbrTestDatabase();
            Services.AddSingleton<IDbContextFactory<DatahubProjectDBContext>>(dbContextFactory);

            var workspaceCreationService = CreateMockedWorkspaceCreationService(portalConfiguration, dbContextFactory, userInfoService);
            Services.AddSingleton(workspaceCreationService);

            var authenticatedNewWorkspacePage = RenderComponent<CascadingAuthenticationState>(parameters =>
            {
                parameters.AddChildContent<MudPopoverProvider>();
                parameters.AddChildContent<CreateWorkspacePage>();
            });

            var workspaceCreationPage = authenticatedNewWorkspacePage.FindComponent<CreateWorkspacePage>();

            scenarioContext[CREATE_WORKSPACE_PAGE_CTX_KEY] = workspaceCreationPage;
        }

        private async Task CheckWorkspaceCreationFormValidity(bool valid)
        {
            var workspaceCreationPage = GetCreateWorkspacePageFromContext();
            var form = FindCreateWorkspaceForm(workspaceCreationPage);
            form.Should().NotBeNull();
            await workspaceCreationPage.InvokeAsync(form!.Instance.Validate);
            form!.Instance.IsValid.Should().Be(valid);
        }

        [Then("the workspace creation form should be invalid")]
        public async Task ThenTheWorkspaceCreationFormShouldBeInvalid()
        {
            await CheckWorkspaceCreationFormValidity(false);
        }

        [Then("the workspace creation form should be valid")]
        public async Task ThenTheWorkspaceCreationFormShouldBeValid()
        {
            await CheckWorkspaceCreationFormValidity(true);
        }

        private void CheckCreateWorkspaceButtonEnabledStatus(bool enabled)
        {
            var createWorkspacePage = GetCreateWorkspacePageFromContext();
            var button = FindCreateWorkspaceButtonInNewWorkspaceForm(createWorkspacePage);
            button.Instance.Disabled.Should().NotBe(enabled);
        }

        [Then("the create workspace button should be enabled")]
        public void ThenTheCreateWorkspaceButtonShouldBeEnabled()
        {
            CheckCreateWorkspaceButtonEnabledStatus(true);
        }

        [Then("the create workspace button should be disabled")]
        public void ThenTheCreateWorkspaceButtonShouldBeDisabled()
        {
            CheckCreateWorkspaceButtonEnabledStatus(false);
        }

        [When("the user enters a workspace title in the creation form")]
        public async Task WhenTheUserEntersAWorkspaceTitleInTheCreationForm()
        {
            var workspaceCreationPage = GetCreateWorkspacePageFromContext();
            var form = FindCreateWorkspaceForm(workspaceCreationPage);
            form.Should().NotBeNull();
            var titleTextbox = FindWorkspaceTitleTextField(form!);
            var acronymTextbox = FindWorkspaceAcronymTextField(form!);

            await workspaceCreationPage.InvokeAsync(async () => await titleTextbox.Instance.ValueChanged.InvokeAsync("Test Workspace"));
            // manually invoke the debounce task in order to auto generate and populate acronym
            await workspaceCreationPage.InvokeAsync(titleTextbox.Instance.OnDebounceIntervalElapsed.InvokeAsync);
            workspaceCreationPage.Render();
            
        }

        [When("the user selects a CBR from the dropdown in the workspace creation form")]
        public async Task WhenTheUserSelectsACbrFromTheDropdownInTheWorkspaceCreationForm()
        {
            var workspaceCreationPage = GetCreateWorkspacePageFromContext();
            var form = FindCreateWorkspaceForm(workspaceCreationPage);
            form.Should().NotBeNull();
            var cbrDropdown = FindCbrDropdownInForm(form!);
            var nonNullCbr = FindFirstNonNullCbr(cbrDropdown);

            await workspaceCreationPage.InvokeAsync(async () => await cbrDropdown.Instance.ValueChanged.InvokeAsync(nonNullCbr));
            await workspaceCreationPage.InvokeAsync(form!.Instance.Validate);
            workspaceCreationPage.Render();
        }

        [When("the user enters a budget of {decimal} in the workspace creation form")]
        public async Task WhenTheUserEntersABudgetInTheWorkspaceCreationForm(decimal budget)
        {
            var workspaceCreationPage = GetCreateWorkspacePageFromContext();
            var form = FindCreateWorkspaceForm(workspaceCreationPage);
            form.Should().NotBeNull();

            // verify that a cbr is selected
            var cbrDropdown = FindCbrDropdownInForm(form!);
            cbrDropdown.Should().NotBeNull();
            cbrDropdown.Instance.Value.Should().NotBeNull();

            var budgetInput = FindBudgetInput(form!);
            await workspaceCreationPage.InvokeAsync(async () => await  budgetInput.Instance.ValueChanged.InvokeAsync(budget));
            await workspaceCreationPage.InvokeAsync(form!.Instance.Validate);
            workspaceCreationPage.Render();
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

    }
}
