using Bunit;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Services.Projects;
using Datahub.Infrastructure.Offline;
using Datahub.Portal.Pages.Workspace.Settings;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps.Workspace;

[Binding]
public class WorkspaceCbrSettingsPageSteps(
    ScenarioContext scenarioContext, 
    IWebHostEnvironment hostingEnvironment
) : TestContext
{
    private const string RelativePathToSrc = "../../../../../src";

    private const string WORKSPACE_CBR_SETTINGS_PAGE_CTX_KEY = "workspaceCbrSettingsPage";

    // Per-scenario users
    private PortalUser cbrOwnerUser;
    private PortalUser otherWorkspaceLeadUser;

    private async Task SetupServices()
    {
        Services.AddSingleton(hostingEnvironment);

        var portalConfiguration = new DatahubPortalConfiguration()
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

        var mockRequestManagementService = Substitute.For<IRequestManagementService>();
        Services.AddSingleton(mockRequestManagementService);

        JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
        JSInterop.SetupModule("./_content/Datahub.Portal/Components/SkipLink.razor.js");
        JSInterop.SetupVoid("mudPopover.initialize", _ => true);
        JSInterop.Setup<int>("mudpopoverHelper.countProviders");
        JSInterop.SetupVoid("mudPopover.connect", _ => true);
        JSInterop.SetupVoid("mudElementRef.addOnBlurEvent", _ => true);
        JSInterop.SetupVoid("mudElementRef.removeOnBlurEvent", _ => true);
        Services.AddStub<IDatahubAuditingService>();
    }

    [Given("authorization as a CBR Owner for the CBR budget management page")]
    public void GivenAuthorizationAsACbrOwnerForTheCbrBudgetManagementPage()
    {
        cbrOwnerUser = CommonCbrTestUtils.CreateCbrOwnerUser();
        otherWorkspaceLeadUser = CommonCbrTestUtils.CreateOtherWorkspaceLead();

        var userInfoService = Substitute.For<IUserInformationService>();
        userInfoService.GetCurrentPortalUserAsync().Returns(cbrOwnerUser);
        Services.AddSingleton(userInfoService);

        CommonCbrTestUtils.AddLoggedInUserAuthorization(this, Testing.WorkspaceAcronym, true, false);
    }

    [Given("a workspace CBR budget management page")]
    public async Task GivenAWorkspaceCbrBudgetManagementPage()
    {
        await SetupServices();

        cbrOwnerUser ??= CommonCbrTestUtils.CreateCbrOwnerUser();
        otherWorkspaceLeadUser ??= CommonCbrTestUtils.CreateOtherWorkspaceLead();

        var dbContextFactory = await CommonCbrTestUtils.GenerateCbrTestDatabase(cbrOwnerUser, otherWorkspaceLeadUser);
        Services.AddSingleton<IDbContextFactory<DatahubProjectDBContext>>(dbContextFactory);

        var authenticatedCbrBudgetPage = RenderComponent<CascadingAuthenticationState>(parameters =>
        {
            parameters.AddChildContent<MudPopoverProvider>();
            parameters.AddChildContent<CBRBudgetManagementPage>(childParams =>
            {
                childParams.Add(p => p.WorkspaceAcronym, Testing.WorkspaceAcronym);
                childParams.Add(p => p.ElevatedWorkspaceAccessEnabled, true);

            });
        });

        scenarioContext[WORKSPACE_CBR_SETTINGS_PAGE_CTX_KEY] = authenticatedCbrBudgetPage.FindComponent<CBRBudgetManagementPage>();
    }

    private IRenderedComponent<CBRBudgetManagementPage> GetCbrBudgetManagementPageFromContext()
    {
        var cbrPage = scenarioContext[WORKSPACE_CBR_SETTINGS_PAGE_CTX_KEY] as IRenderedComponent<CBRBudgetManagementPage>;
        cbrPage.Should().NotBeNull();
        return cbrPage!;
    }

    private static IRenderedComponent<MudButton>? FindSubmitButton(IRenderedComponent<CBRBudgetManagementPage> cbrPage)
    {
        var allButtons = cbrPage.FindComponents<MudButton>();
        var submitButton = allButtons.FirstOrDefault(b => b.Instance.Tag is string tagStr && tagStr == CBRBudgetManagementPage.SUBMIT_BUTTON_TAG);
        return submitButton;
    }

    private static IRenderedComponent<MudTr>? FindBudgetRowForWorksapceWithAcronym(IRenderedComponent<CBRBudgetManagementPage> cbrPage, string workspaceAcronym)
    {
        var budgetTable = cbrPage.FindComponent<MudTable<WorkspaceBudgetManagementItem>>();
        var rows = budgetTable.FindComponents<MudTr>();
        var budgetRow = rows.FirstOrDefault(r => r.Instance.Item is WorkspaceBudgetManagementItem budgetItem && budgetItem.Workspace.Project_Acronym_CD ==  workspaceAcronym);
        return budgetRow;
    }

    private static IRenderedComponent<MudIconButton> FindEditButtonInBudgetRow(IRenderedComponent<MudTr> budgetRow)
    {
        // get the icon button based on its icon
        var rowIconButtons = budgetRow.FindComponents<MudIconButton>();
        var editButton = rowIconButtons.FirstOrDefault(b => b.Instance is MudIconButton button && button.Icon == Icons.Material.Outlined.Edit);
        editButton.Should().NotBeNull();
        return editButton!;
    }

    private static IRenderedComponent<MudTextField<decimal>>? FindBudgetInputInBudgetRow(IRenderedComponent<MudTr> budgetRow)
    {
        var rowTextInputs = budgetRow.FindComponents<MudTextField<decimal>>();
        var budgetInput = rowTextInputs.FirstOrDefault(t => t.Instance.Tag is string tagStr && tagStr == CBRBudgetManagementPage.BUDGET_INPUT_TAG);
        return budgetInput;
    }

    private static IRenderedComponent<MudIconButton> FindCommitEditIconInEditableBudgetRow(IRenderedComponent<MudTr> budgetRow)
    {
        var rowIconButtons = budgetRow.FindComponents<MudIconButton>();
        var commitButton = rowIconButtons.FirstOrDefault(b => b.Instance is MudIconButton button && button.Icon == budgetRow.Instance.Context!.Table!.CommitEditIcon);
        commitButton.Should().NotBeNull();
        return commitButton!;
    }

    private static IRenderedComponent<MudIcon>? FindBudgetValidationErrorIcon(IRenderedComponent<CBRBudgetManagementPage> cbrPage)
    {
        var pageIcons = cbrPage.FindComponents<MudIcon>();
        var validationErrorIcon = pageIcons.FirstOrDefault(i => i.Instance.Tag is string tagStr && tagStr == CBRBudgetManagementPage.BUDGET_VALIDATION_ERROR_TAG);
        return validationErrorIcon;
    }

    private void CheckSubmitButtonDisabledStatus(bool disabled)
    {
        var cbrPage = GetCbrBudgetManagementPageFromContext();
        var submitButton = FindSubmitButton(cbrPage);
        submitButton.Should().NotBeNull();
        submitButton!.Instance.Disabled.Should().Be(disabled);
    }

    [Then("the submit button should be disabled")]
    public void ThenTheSubmitButtonShouldBeDisabled()
    {
        CheckSubmitButtonDisabledStatus(true);
    }

    [Then("the submit button should be enabled")]
    public void ThenTheSubmitButtonShouldBeEnabled()
    {
        CheckSubmitButtonDisabledStatus(false);
    }

    [When("the CBR budget row for the test workspace is edited")]
    public async Task WhenTheCbrBudgetRowForTheTestWorkspaceIsEdited()
    {
        var cbrPage = GetCbrBudgetManagementPageFromContext();
        var budgetRow = FindBudgetRowForWorksapceWithAcronym(cbrPage, Testing.WorkspaceAcronym);
        budgetRow.Should().NotBeNull();
        var editButton = FindEditButtonInBudgetRow(budgetRow!);
        await cbrPage.InvokeAsync(editButton.Instance.OnClick.InvokeAsync);
        cbrPage.Render();
    }

    [When("the CBR budget for the test workspace is changed to {decimal}")]
    public async Task WhenTheCbrBudgetForTheTestWorkspaceIsChangedTo(decimal amount)
    {
        var cbrPage = GetCbrBudgetManagementPageFromContext();
        var budgetRow = FindBudgetRowForWorksapceWithAcronym(cbrPage, Testing.WorkspaceAcronym);
        budgetRow.Should().NotBeNull();
        var budgetInput = FindBudgetInputInBudgetRow(budgetRow!);
        budgetInput.Should().NotBeNull();
        await cbrPage.InvokeAsync(async () => await budgetInput!.Instance.ValueChanged.InvokeAsync(amount));
        
        cbrPage.Render();
    }

    [When("the edited CBR budget for the test workspace is committed")]
    public async Task WhenTheEditedCbrBudgetForTheTestWorkspaceIsConfirmed()
    {
        var cbrPage = GetCbrBudgetManagementPageFromContext();
        var budgetRow = FindBudgetRowForWorksapceWithAcronym(cbrPage, Testing.WorkspaceAcronym);
        budgetRow.Should().NotBeNull();
        var commitButton = FindCommitEditIconInEditableBudgetRow(budgetRow!);
        await cbrPage.InvokeAsync(commitButton.Instance.OnClick.InvokeAsync);
        cbrPage.Render();
    }

    [Then("the CBR budget validation error should be shown")]
    public void ThenTheCbrBudgetValidationErrorShouldBeShown()
    {
        var cbrPage = GetCbrBudgetManagementPageFromContext();
        var validationErrorIcon = FindBudgetValidationErrorIcon(cbrPage);
        validationErrorIcon.Should().NotBeNull();
    }

    [Then("the CBR budget validation error should not be shown")]
    public void ThenTheCbrBudgetValidationErrorShouldNotBeShown()
    {
        var cbrPage = GetCbrBudgetManagementPageFromContext();
        var validationErrorIcon = FindBudgetValidationErrorIcon(cbrPage);
        validationErrorIcon.Should().BeNull();
    }
}
