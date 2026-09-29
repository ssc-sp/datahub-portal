using Bunit;
using Datahub.Application.Services.UserManagement;
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
        var stepper = form.FindComponent<GcdsStepper>();
        stepper.Instance.CurrentStep.Should().Be(1);
        stepper.Instance.TotalSteps.Should().Be(5);
        stepper.Markup.Should().Contain("Additional Information");
        form.FindComponents<GcdsNotice>().Should().ContainSingle();
        form.FindComponents<GcdsButton>().Should().HaveCount(2)
            .And.OnlyContain(button => button.Instance.Type == GcdsButtonType.Button);
        form.FindComponents<DHButton>().Should().BeEmpty();
        form.FindComponents<MudSelect<string>>().Should().BeEmpty();
        form.FindComponents<MudTextField<string>>().Should().BeEmpty();
    }

    [Then("the existing web app token is masked")]
    public async Task ThenTheExistingWebAppTokenIsMasked()
    {
        await NavigateToStepAsync(3);
        FindInput("web-app-access-token").Instance.Value.Should().Be("secret********");
        GetForm().Markup.Should().NotContain("secret-value");
    }

    [When("I select a private web app repository")]
    public async Task WhenISelectAPrivateWebAppRepository()
    {
        await NavigateToStepAsync(3);
        var visibility = GetForm().FindComponent<GcdsRadios>();
        await visibility.InvokeAsync(() => visibility.Instance.ValueChanged.InvokeAsync("private"));
    }

    [When("I save the web app configuration")]
    public async Task WhenISaveTheWebAppConfiguration()
    {
        await NavigateToStepAsync(5);
        if (CurrentStep == 5)
        {
            GetForm().Markup.Should().Contain("********").And.NotContain("secret-value");
            await ClickButtonAsync("Save");
        }
    }

    [Then("the web app configuration is not submitted")]
    public void ThenTheWebAppConfigurationIsNotSubmitted()
        => _submittedConfiguration.Should().BeNull();

    [Then("the required web app configuration errors are displayed")]
    public void ThenTheRequiredWebAppConfigurationErrorsAreDisplayed()
    {
        FindInput("web-app-git-repository").Instance.ErrorMessage.Should().Be("Url cannot be empty");
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
        await NavigateToStepAsync(3);
        var repository = FindInput("web-app-git-repository");
        await repository.InvokeAsync(() => repository.Instance.ValueChanged.InvokeAsync(
            "https://embedded-token@gitprovider.example/repository.git"));
    }

    [When("I enter a public web app repository URL")]
    public async Task WhenIEnterAPublicWebAppRepositoryUrl()
    {
        await NavigateToStepAsync(3);
        var repository = FindInput("web-app-git-repository");
        await repository.InvokeAsync(() => repository.Instance.ValueChanged.InvokeAsync(
            "https://gitprovider.example/public-repository.git"));
    }

    [When("I enter the web app compose path")]
    public async Task WhenIEnterTheWebAppComposePath()
    {
        await NavigateToStepAsync(4);
        var composePath = FindInput("web-app-compose-path");
        await composePath.InvokeAsync(() => composePath.Instance.ValueChanged.InvokeAsync("compose.yaml"));
    }

    [When("I advance to the web app configuration review")]
    public Task WhenIAdvanceToTheWebAppConfigurationReview() => NavigateToStepAsync(5);

    [Then("the public web app configuration review contains my input")]
    public void ThenThePublicWebAppConfigurationReviewContainsMyInput()
    {
        CurrentStep.Should().Be(5);
        var markup = GetForm().Markup;
        markup.Should().Contain("Docker compose")
            .And.Contain("https://gitprovider.example/public-repository.git")
            .And.Contain("Public")
            .And.Contain("N/A")
            .And.Contain("compose.yaml");
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
        await NavigateToStepAsync(4);
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
        Services.AddSingleton(Substitute.For<ICultureService>());

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

    private int CurrentStep => GetForm().FindComponent<GcdsStepper>().Instance.CurrentStep;

    private async Task NavigateToStepAsync(int step)
    {
        while (CurrentStep < step)
        {
            var previousStep = CurrentStep;
            await ClickButtonAsync("Next");
            if (CurrentStep == previousStep)
            {
                break;
            }
        }
    }

    private IRenderedComponent<WebAppConfigurationForm> GetForm()
        => _form ?? throw new InvalidOperationException("The web app configuration form has not been rendered.");

    void IDisposable.Dispose()
    {
        // BunitTestSteps disposes the context asynchronously in its AfterScenario hook.
    }
}
