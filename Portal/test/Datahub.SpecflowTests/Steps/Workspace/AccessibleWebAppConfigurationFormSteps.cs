using Bunit;
using Datahub.Core.Components.Buttons;
using Datahub.Portal.Pages.Workspace.WebApp;
using Datahub.Shared.Entities.WorkspaceToolConfiguration;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using GcdsWrapper.Blazor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps.Workspace;

[Binding]
public sealed class AccessibleWebAppConfigurationFormSteps : BunitTestSteps, IDisposable
{
    private IRenderedComponent<WebAppConfigurationForm>? _form;
    private AppServiceConfiguration? _initialConfiguration;
    private AppServiceConfiguration? _submittedConfiguration;
    private bool _cancelled;

    [Given("the web app configuration form has an existing private configuration")]
    public void GivenTheWebAppConfigurationFormHasAnExistingPrivateConfiguration()
    {
        RenderForm(new AppServiceConfiguration
        {
            Framework = AppServiceTemplates.CUSTOM,
            GitRepo = "https://gitprovider.example/repository.git",
            ComposePath = "deploy/compose.yaml",
            IsGitRepoPrivate = true,
            GitToken = "secret-value"
        });
    }

    [Given("the web app configuration form is empty")]
    public void GivenTheWebAppConfigurationFormIsEmpty()
    {
        RenderForm(new AppServiceConfiguration
        {
            Framework = AppServiceTemplates.CUSTOM,
            GitRepo = string.Empty,
            ComposePath = string.Empty
        });
    }

    [Then("the web app configuration is a labelled inline region")]
    public void ThenTheWebAppConfigurationIsALabelledInlineRegion()
    {
        var form = GetForm();
        var region = form.Find("section[role='region']");
        region.Id.Should().Be("web-app-configuration-form");
        region.GetAttribute("aria-labelledby").Should().Be("web-app-configuration-heading");
        form.Find("#web-app-configuration-heading").TextContent.Trim()
            .Should().Be("Configure Web Application");
        form.FindAll("[role='dialog']").Should().BeEmpty();
        form.FindComponents<MudDialog>().Should().BeEmpty();
    }

    [Then("the web app configuration uses GCDS controls")]
    public void ThenTheWebAppConfigurationUsesGcdsControls()
    {
        var form = GetForm();
        form.FindComponents<GcdsSelect>().Should().ContainSingle();
        form.FindComponents<GcdsRadios>().Should().ContainSingle();
        form.FindComponents<GcdsInput>().Should().HaveCount(3);
        form.FindComponents<GcdsNotice>().Should().ContainSingle();
        form.FindComponents<GcdsButton>().Should().HaveCount(2)
            .And.OnlyContain(button => button.Instance.Type == GcdsButtonType.Button);
        form.FindComponents<DHButton>().Should().BeEmpty();
        form.FindComponents<MudSelect<string>>().Should().BeEmpty();
        form.FindComponents<MudTextField<string>>().Should().BeEmpty();
    }

    [Then("the existing web app token is masked")]
    public void ThenTheExistingWebAppTokenIsMasked()
    {
        FindInput("web-app-access-token").Instance.Value.Should().Be("secret********");
        GetForm().Markup.Should().NotContain("secret-value");
    }

    [When("I select a private web app repository")]
    public async Task WhenISelectAPrivateWebAppRepository()
    {
        var visibility = GetForm().FindComponent<GcdsRadios>();
        await visibility.InvokeAsync(() => visibility.Instance.ValueChanged.InvokeAsync("private"));
    }

    [When("I save the web app configuration")]
    public Task WhenISaveTheWebAppConfiguration() => ClickButtonAsync("Save");

    [Then("the web app configuration is not submitted")]
    public void ThenTheWebAppConfigurationIsNotSubmitted()
        => _submittedConfiguration.Should().BeNull();

    [Then("the required web app configuration errors are displayed")]
    public void ThenTheRequiredWebAppConfigurationErrorsAreDisplayed()
    {
        FindInput("web-app-git-repository").Instance.ErrorMessage.Should().Be("Url cannot be empty");
        FindInput("web-app-compose-path").Instance.ErrorMessage
            .Should().Be("Path to docker compose is necessary, file name must be included");
        FindInput("web-app-access-token").Instance.ErrorMessage
            .Should().Be("Access tokens are required for private repos");
    }

    [Then("the existing private web app configuration is submitted")]
    public void ThenTheExistingPrivateWebAppConfigurationIsSubmitted()
    {
        _submittedConfiguration.Should().NotBeNull();
        _submittedConfiguration!.GitRepo.Should().Be("https://gitprovider.example/repository.git");
        _submittedConfiguration.ComposePath.Should().Be("deploy/compose.yaml");
        _submittedConfiguration.IsGitRepoPrivate.Should().BeTrue();
        _submittedConfiguration.GitToken.Should().Be("secret-value");
    }

    [When("I enter a web app repository URL containing a credential")]
    public async Task WhenIEnterAWebAppRepositoryUrlContainingACredential()
    {
        var repository = FindInput("web-app-git-repository");
        await repository.InvokeAsync(() => repository.Instance.ValueChanged.InvokeAsync(
            "https://embedded-token@gitprovider.example/repository.git"));
    }

    [When("I enter the web app compose path")]
    public async Task WhenIEnterTheWebAppComposePath()
    {
        var composePath = FindInput("web-app-compose-path");
        await composePath.InvokeAsync(() => composePath.Instance.ValueChanged.InvokeAsync("compose.yaml"));
    }

    [Then("the sanitized private web app configuration is submitted")]
    public void ThenTheSanitizedPrivateWebAppConfigurationIsSubmitted()
    {
        _submittedConfiguration.Should().NotBeNull();
        _submittedConfiguration!.GitRepo.Should().Be("https://gitprovider.example/repository.git");
        _submittedConfiguration.IsGitRepoPrivate.Should().BeTrue();
        _submittedConfiguration.GitToken.Should().Be("embedded-token");
    }

    [When("I edit and cancel the web app configuration")]
    public async Task WhenIEditAndCancelTheWebAppConfiguration()
    {
        var composePath = FindInput("web-app-compose-path");
        await composePath.InvokeAsync(() => composePath.Instance.ValueChanged.InvokeAsync("changed.yaml"));
        await ClickButtonAsync("Cancel");
    }

    [Then("the web app configuration reports cancellation")]
    public void ThenTheWebAppConfigurationReportsCancellation()
    {
        _cancelled.Should().BeTrue();
        _submittedConfiguration.Should().BeNull();
    }

    [Then("the original web app configuration is unchanged")]
    public void ThenTheOriginalWebAppConfigurationIsUnchanged()
    {
        _initialConfiguration.Should().NotBeNull();
        _initialConfiguration!.GitRepo.Should().Be("https://gitprovider.example/repository.git");
        _initialConfiguration.ComposePath.Should().Be("deploy/compose.yaml");
        _initialConfiguration.GitToken.Should().Be("secret-value");
    }

    private void RenderForm(AppServiceConfiguration configuration)
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

        _initialConfiguration = configuration;
        _form = Render<WebAppConfigurationForm>(parameters => parameters
            .Add(component => component.Configuration, configuration)
            .Add(component => component.OnSubmit, submitted => _submittedConfiguration = submitted)
            .Add(component => component.OnCancelled, () => _cancelled = true));
    }

    private IRenderedComponent<GcdsInput> FindInput(string id)
        => GetForm().FindComponents<GcdsInput>().Single(input => input.Instance.Id == id);

    private async Task ClickButtonAsync(string text)
    {
        var button = GetForm().FindComponents<GcdsButton>()
            .Single(candidate => candidate.Markup.Contains(text, StringComparison.Ordinal));
        await button.InvokeAsync(() => button.Instance.OnClick.InvokeAsync());
    }

    private IRenderedComponent<WebAppConfigurationForm> GetForm()
        => _form ?? throw new InvalidOperationException("The web app configuration form has not been rendered.");

    void IDisposable.Dispose()
    {
        // BunitTestSteps disposes the context asynchronously in its AfterScenario hook.
    }
}
