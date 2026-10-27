using Bunit;
using Bunit.TestDoubles;
using Datahub.Application.Configuration;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Users;
using Datahub.Portal.Pages.Account.Notifications;
using Datahub.Portal.Pages.Workspace.Settings;
using Datahub.Portal.Pages.Workspace.Toolbox.ConfigurationForms;
using Datahub.Shared.Entities.WorkspaceToolConfiguration;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using GcdsWrapper.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps;

[Binding]
public class SettingsFieldGcdsControlsSteps : BunitTestSteps
{
    private IRenderedComponent<WorkspaceBudgetControl>? _budgetControl;
    private IRenderedComponent<WorkspacePreventAutoDeleteControl>? _preventAutoDeleteControl;
    private IRenderedComponent<WorkspaceCbrSettingsLinkControl>? _cbrControl;
    private IRenderedComponent<WorkspaceDeleteControl>? _deleteControl;
    private IRenderedComponent<PostgresConfigurationForm>? _postgresForm;
    private IRenderedComponent<DatabricksConfigurationForm>? _databricksForm;
    private IRenderedComponent<AccountNotificationSettings>? _notificationSettings;
    private PostgresConfiguration? _postgresConfiguration;
    private DatabricksConfiguration? _databricksConfiguration;
    private decimal? _changedBudget;
    private bool? _preventAutoDelete;
    private string? _postgresSku;
    private string? _databricksSku;

    [Given("a disabled GCDS workspace budget control with a value of 125.50")]
    public void GivenADisabledGcdsWorkspaceBudgetControl()
    {
        ConfigureContext();
        _budgetControl = Render<WorkspaceBudgetControl>(parameters => parameters
            .Add(control => control.InitialValue, 125.5m)
            .Add(control => control.Disabled, true)
            .Add(control => control.OnValueChanged, value => _changedBudget = value));
    }

    [When("the GCDS workspace budget is changed to 200.01")]
    public async Task WhenTheGcdsWorkspaceBudgetIsChanged()
    {
        var input = _budgetControl!.FindComponent<GcdsInput>();
        await _budgetControl.InvokeAsync(() => input.Instance.ValueChanged.InvokeAsync("200.01"));
    }

    [Then("the GCDS workspace budget control preserves its ID and disabled state")]
    public void ThenTheGcdsWorkspaceBudgetControlPreservesItsIdAndDisabledState()
    {
        var input = _budgetControl!.FindComponent<GcdsInput>();
        input.Instance.Id.Should().Be("workspace-budget");
        input.Instance.Value.Should().Be("200.01");
        input.Instance.Disabled.Should().BeTrue();
    }

    [Then("the workspace budget change is published as 200.01")]
    public void ThenTheWorkspaceBudgetChangeIsPublished()
    {
        _changedBudget.Should().Be(200.01m);
    }

    [Given("a GCDS prevent auto delete control")]
    public void GivenAGcdsPreventAutoDeleteControl()
    {
        ConfigureContext();
        _preventAutoDeleteControl = Render<WorkspacePreventAutoDeleteControl>(parameters => parameters
            .Add(control => control.InitialValue, false)
            .Add(control => control.OnValueChanged, value => _preventAutoDelete = value));
    }

    [When("the GCDS prevent auto delete option is selected")]
    public async Task WhenTheGcdsPreventAutoDeleteOptionIsSelected()
    {
        var checkboxes = _preventAutoDeleteControl!.FindComponent<GcdsCheckboxes>();
        checkboxes.Instance.Name.Should().Be("prevent-auto-delete");
        await _preventAutoDeleteControl.InvokeAsync(() => checkboxes.Instance.ValueChanged.InvokeAsync(["enabled"]));
    }

    [Then("the prevent auto delete change is published as enabled")]
    public void ThenThePreventAutoDeleteChangeIsPublishedAsEnabled()
    {
        _preventAutoDelete.Should().BeTrue();
    }

    [Given("the GCDS CBR and workspace deletion controls")]
    public void GivenTheGcdsCbrAndWorkspaceDeletionControls()
    {
        ConfigureContext();
        _cbrControl = Render<WorkspaceCbrSettingsLinkControl>(parameters => parameters
            .Add(control => control.WorkspaceAcronym, "TEST"));
        _deleteControl = Render<WorkspaceDeleteControl>(parameters => parameters
            .Add(control => control.WorkspaceAcronym, "TEST"));
    }

    [Then("the CBR action is a secondary link to the workspace CBR settings")]
    public void ThenTheCbrActionIsASecondaryLinkToTheWorkspaceCbrSettings()
    {
        var button = _cbrControl!.FindComponent<GcdsButton>();
        button.Instance.Type.Should().Be(GcdsButtonType.Link);
        button.Instance.Role.Should().Be(GcdsButtonRole.Secondary);
        button.Instance.Href.Should().Be("/w/TEST/settings/cbr");
    }

    [Then("the workspace deletion action is a danger button with the expected target")]
    public async Task ThenTheWorkspaceDeletionActionIsADangerButtonWithTheExpectedTarget()
    {
        var button = _deleteControl!.FindComponent<GcdsButton>();
        button.Instance.Role.Should().Be(GcdsButtonRole.Danger);
        await _deleteControl.InvokeAsync(() => button.Instance.OnClick.InvokeAsync());
        Services.GetRequiredService<NavigationManager>().Uri.Should().EndWith("/w/TEST/dw");
    }

    [Given("GCDS PostgreSQL and Databricks configuration controls")]
    public void GivenGcdsPostgresqlAndDatabricksConfigurationControls()
    {
        ConfigureContext();
        _postgresConfiguration = new PostgresConfiguration();
        _postgresForm = Render<PostgresConfigurationForm>(parameters => parameters
            .Add(form => form.Configuration, _postgresConfiguration));
        _databricksConfiguration = new DatabricksConfiguration();
        _databricksForm = Render<DatabricksConfigurationForm>(parameters => parameters
            .Add(form => form.Configuration, _databricksConfiguration));
    }

    [When("available PostgreSQL and Databricks tiers are selected")]
    public async Task WhenAvailablePostgresqlAndDatabricksTiersAreSelected()
    {
        var postgresSelect = _postgresForm!.FindComponent<GcdsSelect>();
        postgresSelect.Instance.Id.Should().Be(PostgresConfigurationForm.PostgresSkuSelectId);
        _postgresSku = PostgresTier.GetPostgresTiers().First(tier => tier.IsAvailable).PSQL_SKU;
        await _postgresForm.InvokeAsync(() => postgresSelect.Instance.ValueChanged.InvokeAsync(_postgresSku));

        var machineLearning = _databricksForm!.FindComponents<GcdsCheckboxes>()
            .Single(control => control.Instance.Name == DatabricksConfigurationForm.EnableMLCheckBoxId);
        await _databricksForm.InvokeAsync(() => machineLearning.Instance.ValueChanged.InvokeAsync(["enabled"]));

        var machineLearningSelect = _databricksForm.FindComponents<GcdsSelect>()
            .Single(select => select.Instance.Id == DatabricksConfigurationForm.MLTierSelectId);
        _databricksSku = DatabricksTier.GetMachineLearningTiers().First(tier => tier.IsAvailable).DatabricksSKU;
        await _databricksForm.InvokeAsync(() => machineLearningSelect.Instance.ValueChanged.InvokeAsync(_databricksSku));
    }

    [Then("the toolbox configurations contain the selected tiers")]
    public void ThenTheToolboxConfigurationsContainTheSelectedTiers()
    {
        _postgresConfiguration!.PSQL_SKU.Should().Be(_postgresSku);
        _databricksConfiguration!.EnableMachineLearning.Should().BeTrue();
        _databricksConfiguration.MachineLearningTierSku.Should().Be(_databricksSku);
    }

    [Given("the GCDS notification preference control")]
    public void GivenTheGcdsNotificationPreferenceControl()
    {
        ConfigureContext();
        Services.AddSingleton(Substitute.For<IUserInformationService>());
        Services.AddSingleton(new DatahubPortalConfiguration());
        var user = new PortalUser
        {
            Email = "notification@example.gc.ca",
            EntraUser = new EntraUser { GraphGuid = "notification-user", PortalUser = null! }
        };
        var authorization = this.AddAuthorization();
        authorization.SetAuthorized("Notification User");
        authorization.SetRoles(user.UserUID()!);

        _notificationSettings = Render<AccountNotificationSettings>(parameters => parameters
            .Add(settings => settings.ViewedUser, user));
    }

    [Then("all notifications are selected and the radio group is disabled")]
    public void ThenAllNotificationsAreSelectedAndTheRadioGroupIsDisabled()
    {
        var radios = _notificationSettings!.FindComponent<GcdsRadios>();
        radios.Instance.Value.Should().Be("all");
        radios.Instance.Disabled.Should().BeTrue();
    }

    private void ConfigureContext()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        var localizer = Substitute.For<IStringLocalizer>();
        localizer[Arg.Any<string>()]
            .Returns(call => new LocalizedString(call.Arg<string>(), call.Arg<string>()));
        localizer[Arg.Any<string>(), Arg.Any<object[]>()]
            .Returns(call => new LocalizedString(
                call.ArgAt<string>(0),
                string.Format(call.ArgAt<string>(0), call.ArgAt<object[]>(1))));
        Services.AddSingleton(localizer);
    }
}
