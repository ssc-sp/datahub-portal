using System.Reflection;
using AngleSharp.Dom;
using Bunit;
using Datahub.Application.Services.ReverseProxy;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.UserManagement;
using Datahub.Application.Services.WebApp;
using Datahub.Core.Components.Buttons;
using Datahub.Core.Model.Context;
using Datahub.Portal.Components;
using Datahub.Portal.Pages.Workspace.WebApp;
using Datahub.Shared.Entities.WorkspaceToolConfiguration;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using GcdsWrapper.Blazor;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps.Workspace;

[Binding]
public sealed class AccessibleWebAppControlPanelSteps : BunitTestSteps, IDisposable
{
    private IRenderedComponent<TestWorkspaceWebAppPage>? _page;

    [Given("a running configured web application is displayed")]
    public void GivenARunningConfiguredWebApplicationIsDisplayed()
        => RenderPage(isProvisioned: true, isConfigured: true, isRunning: true);

    [Given("a stopped configured web application is displayed")]
    public void GivenAStoppedConfiguredWebApplicationIsDisplayed()
        => RenderPage(isProvisioned: true, isConfigured: true, isRunning: false);

    [Given("an unconfigured provisioned web application is displayed")]
    public void GivenAnUnconfiguredProvisionedWebApplicationIsDisplayed()
        => RenderPage(isProvisioned: true, isConfigured: false, isRunning: false);

    [Given("an unprovisioned web application is displayed")]
    public void GivenAnUnprovisionedWebApplicationIsDisplayed()
        => RenderPage(isProvisioned: false, isConfigured: false, isRunning: false);

    [Then("the web application title contains no controls")]
    public void ThenTheWebApplicationTitleContainsNoControls()
    {
        var title = GetPage().FindComponent<DHMainContentTitle>();
        title.Markup.Should().Contain("Web application")
            .And.NotContain("Running")
            .And.NotContain("Configure")
            .And.NotContain("Redeploy");
    }

    [Then("the web application control panel displays the running status")]
    public void ThenTheWebApplicationControlPanelDisplaysTheRunningStatus()
    {
        var panel = GetPanel();
        panel.GetAttribute("aria-labelledby").Should().Be("web-app-control-panel-heading");
        panel.QuerySelector("[role='status']")!.TextContent.Should().Contain("Status:").And.Contain("Running");
    }

    [Then("the control panel uses the expected GCDS buttons")]
    public void ThenTheControlPanelUsesTheExpectedGcdsButtons()
    {
        var buttons = GetPage().FindComponents<GcdsButton>()
            .Where(button => ControlButtonLabels.Any(label => ButtonHasLabel(button, label)))
            .ToArray();

        buttons.Should().HaveCount(4);
        buttons.Should().OnlyContain(button => button.Instance.Type == GcdsButtonType.Button);
        buttons.Should().OnlyContain(button => button.Instance.Size == GcdsButtonSize.Small);
        buttons.Should().OnlyContain(button => button.Instance.OnClick.HasDelegate);
        FindButton("Stop").Instance.Role.Should().Be(GcdsButtonRole.Danger);
        FindButton("Stop").Instance.Disabled.Should().BeFalse();
        FindButton("Restart").Instance.Role.Should().Be(GcdsButtonRole.Secondary);
        FindButton("Restart").Instance.Disabled.Should().BeFalse();
        FindButton("Redeploy").Instance.Role.Should().Be(GcdsButtonRole.Secondary);
        FindButton("Configure").Instance.Role.Should().Be(GcdsButtonRole.Primary);
        FindButton("Configure").Instance.Disabled.Should().BeFalse();
        GetPage().FindComponents<MudStack>()
            .Should().ContainSingle(stack => stack.Instance.Wrap == Wrap.Wrap);

        GetPage().FindComponents<DHButton>()
            .Should().OnlyContain(button => ControlButtonLabels.All(label => !ButtonHasLabel(button, label)));

        GetPage().InvokeAsync(() => GetPage().Instance.SetControlLoadingState(true));
        GetPage().FindComponents<MudProgressCircular>().Should().HaveCount(3);
        FindButton("Configure").Instance.Disabled.Should().BeTrue();
        GetPage().InvokeAsync(() => GetPage().Instance.SetControlLoadingState(false));
    }

    [When("I open and cancel the web application configuration")]
    public async Task WhenIOpenAndCancelTheWebApplicationConfiguration()
    {
        var configure = FindButton("Configure");
        configure.Instance.AdditionalAttributes!["aria-expanded"].Should().Be("false");
        configure.Instance.AdditionalAttributes["aria-controls"].Should().Be("web-app-configuration-form");

        await configure.InvokeAsync(() => configure.Instance.OnClick.InvokeAsync());
        FindButton("Configure").Instance.AdditionalAttributes!["aria-expanded"].Should().Be("true");

        var form = GetPage().FindComponent<WebAppConfigurationForm>();
        await form.InvokeAsync(() => form.Instance.OnCancelled.InvokeAsync());
    }

    [Then("the control panel heading is the configuration return focus target")]
    public void ThenTheControlPanelHeadingIsTheConfigurationReturnFocusTarget()
    {
        GetPage().FindComponents<WebAppConfigurationForm>().Should().BeEmpty();
        var focusTarget = GetPage().Find("#web-app-control-panel-heading").ParentElement!;
        focusTarget.GetAttribute("tabindex").Should().Be("-1");
        focusTarget.GetAttribute("aria-labelledby").Should().Be("web-app-control-panel-heading");
    }

    [Then("the web application control panel displays the stopped status")]
    public void ThenTheWebApplicationControlPanelDisplaysTheStoppedStatus()
        => GetPanel().QuerySelector("[role='status']")!.TextContent.Should().Contain("Stopped");

    [Then("the control panel offers the GCDS start action")]
    public void ThenTheControlPanelOffersTheGcdsStartAction()
    {
        FindButton("Start").Instance.Role.Should().Be(GcdsButtonRole.Secondary);
        FindButton("Restart").Instance.Disabled.Should().BeTrue();
        GetPage().FindComponents<GcdsButton>().Should().NotContain(button => ButtonHasLabel(button, "Stop"));
    }

    [Then("the web application control panel only offers configuration")]
    public void ThenTheWebApplicationControlPanelOnlyOffersConfiguration()
    {
        GetPanel().QuerySelectorAll("[role='status']").Should().BeEmpty();
        GetPage().FindComponents<GcdsButton>().Should().ContainSingle()
            .Which.Markup.Should().Contain("Configure");
    }

    [Then("the web application control panel is not displayed")]
    public void ThenTheWebApplicationControlPanelIsNotDisplayed()
        => GetPage().FindAll("#web-app-control-panel").Should().BeEmpty();

    private void RenderPage(bool isProvisioned, bool isConfigured, bool isRunning)
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();

        var localizer = Substitute.For<IStringLocalizer>();
        localizer[Arg.Any<string>()]
            .Returns(call => new LocalizedString(call.ArgAt<string>(0), call.ArgAt<string>(0)));
        localizer[Arg.Any<string>(), Arg.Any<object[]>()]
            .Returns(call => new LocalizedString(
                call.ArgAt<string>(0),
                string.Format(call.ArgAt<string>(0), call.ArgAt<object[]>(1))));
        Services.AddSingleton(localizer);
        Services.AddSingleton(Substitute.For<ICultureService>());
        Services.AddSingleton(Substitute.For<IKeyVaultUserService>());
        Services.AddSingleton(Substitute.For<ISnackbar>());
        Services.AddSingleton(Substitute.For<ILogger<WorkspaceWebAppPage>>());
        Services.AddSingleton(Substitute.For<IWorkspaceWebAppManagementService>());
        Services.AddSingleton(Substitute.For<IReverseProxyManagerService>());
        Services.AddSingleton(Substitute.For<IReverseProxyConfigService>());
        Services.AddTransient<MicrosoftIdentityConsentAndConditionalAccessHandler>();

        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        Services.AddSingleton<IDbContextFactory<DatahubProjectDBContext>>(new SpecFlowDbContextFactory(options));

        _page = Render<TestWorkspaceWebAppPage>(parameters => parameters
            .Add(component => component.WorkspaceAcronym, "CONTROL")
            .Add(component => component.IsProvisionedForTest, isProvisioned)
            .Add(component => component.IsConfiguredForTest, isConfigured)
            .Add(component => component.IsRunningForTest, isRunning));
    }

    private IElement GetPanel() => GetPage().Find("#web-app-control-panel");

    private IRenderedComponent<GcdsButton> FindButton(string label)
        => GetPage().FindComponents<GcdsButton>().Single(button => ButtonHasLabel(button, label));

    private static bool ButtonHasLabel<TComponent>(IRenderedComponent<TComponent> button, string label)
        where TComponent : IComponent
        => button.Markup.Contains(label, StringComparison.Ordinal);

    private IRenderedComponent<TestWorkspaceWebAppPage> GetPage()
        => _page ?? throw new InvalidOperationException("The web application page has not been rendered.");

    private static readonly HashSet<string> ControlButtonLabels =
        ["Start", "Stop", "Restart", "Redeploy", "Configure"];

    void IDisposable.Dispose()
    {
        // BunitTestSteps disposes the context asynchronously in its AfterScenario hook.
    }
}

public sealed class TestWorkspaceWebAppPage : WorkspaceWebAppPage
{
    [Parameter] public bool IsProvisionedForTest { get; set; }
    [Parameter] public bool IsConfiguredForTest { get; set; }
    [Parameter] public bool IsRunningForTest { get; set; }

    protected override Task OnInitializedAsync()
    {
        SetPrivateField("_isElevated", true);
        SetPrivateField("_isProvisioned", IsProvisionedForTest);
        SetPrivateField("_isConfigured", IsConfiguredForTest);
        SetPrivateField("_webAppState", IsRunningForTest);
        SetPrivateField("_appConfiguration", new AppServiceConfiguration
        {
            Framework = "custom",
            GitRepo = "https://git.example/application.git",
            ComposePath = "compose.yaml",
            Id = "app-id",
            HostName = "application.example"
        });
        return Task.CompletedTask;
    }

    public void SetControlLoadingState(bool isLoading)
    {
        SetPrivateField("_isStartStopping", isLoading);
        SetPrivateField("_isRestarting", isLoading);
        SetPrivateField("_isRedeploying", isLoading);
        SetPrivateField("_isLoadingConfiguration", isLoading);
        StateHasChanged();
    }

    private void SetPrivateField(string name, object value)
    {
        var field = typeof(WorkspaceWebAppPage).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Could not find {name}.");
        field.SetValue(this, value);
    }
}
