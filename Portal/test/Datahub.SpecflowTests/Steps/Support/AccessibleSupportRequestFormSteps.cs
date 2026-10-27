using Bunit;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Components.Buttons;
using Datahub.Portal.Pages.Help;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using GcdsWrapper.Blazor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps.Support;

[Binding]
public sealed class AccessibleSupportRequestFormSteps : BunitTestSteps, IDisposable
{
    private IRenderedComponent<SupportRequestForm>? _form;
    private SupportRequestFormData? _submittedRequest;

    [Given("the support request form is rendered")]
    public void GivenTheSupportRequestFormIsRendered()
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
        localizer["MainAcronym"].Returns(new LocalizedString("MainAcronym", "FSDH"));
        Services.AddSingleton(localizer);

        var cultureService = Substitute.For<ICultureService>();
        cultureService.IsFrench.Returns(false);
        Services.AddSingleton(cultureService);

        _form = Render<SupportRequestForm>(parameters => parameters
            .Add(component => component.Workspaces, ["Research Workspace", "Analytics Workspace"])
            .Add(component => component.OnSubmit, request => _submittedRequest = request));
    }

    [Then("the support request form is labelled by its heading")]
    public void ThenTheSupportRequestFormIsLabelledByItsHeading()
    {
        var form = GetForm();
        var region = form.Find("section[role='region']");
        region.GetAttribute("aria-labelledby").Should().Be("support-request-heading");
        form.Find("#support-request-heading").TextContent.Trim().Should().Be("FSDH Support Request");
    }

    [Then("the support request stepper is hidden")]
    public void ThenTheSupportRequestStepperIsHidden()
    {
        GetForm().FindComponents<GcdsStepper>().Should().BeEmpty();
    }

    [Then("the support request form announces step 1 of 5")]
    public void ThenTheSupportRequestFormAnnouncesStepOneOfFive()
    {
        var stepper = GetForm().FindComponent<GcdsStepper>();
        stepper.Instance.CurrentStep.Should().Be(1);
        stepper.Instance.TotalSteps.Should().Be(5);
        stepper.Markup.Should().Contain("Select support request topics");
    }

    [Then("the support request form actions use button semantics")]
    public void ThenTheSupportRequestFormActionsUseButtonSemantics()
    {
        var buttons = GetForm().FindComponents<GcdsButton>();
        buttons.Should().ContainSingle();
        buttons[0].Instance.Type.Should().Be(GcdsButtonType.Button);
        GetForm().FindComponents<DHButton>().Should().BeEmpty();
    }

    [When("I advance to the support request details")]
    public async Task WhenIAdvanceToTheSupportRequestDetails()
    {
        await ClickButtonAsync("Get Started");
        GetForm().FindComponent<GcdsStepper>().Instance.CurrentStep.Should().Be(1);
        GetForm().FindComponents<GcdsCheckboxes>().Should().ContainSingle();
        GetForm().FindComponents<GcdsTextarea>().Should().BeEmpty();
        GetForm().FindComponents<GcdsRadios>().Should().BeEmpty();
        GetForm().FindComponents<MudSelect<string>>().Should().BeEmpty();
        GetForm().FindComponents<MudTextField<string>>().Should().BeEmpty();
        GetForm().FindComponents<MudRadioGroup<string>>().Should().BeEmpty();
    }

    [When("I try to review the support request without a description")]
    public async Task WhenITryToReviewTheSupportRequestWithoutADescription()
    {
        await ClickButtonAsync("Next");
        await ClickButtonAsync("Next");
    }

    [Then("the support request details remain visible")]
    public void ThenTheSupportRequestDetailsRemainVisible()
    {
        GetForm().FindComponent<GcdsStepper>().Instance.CurrentStep.Should().Be(2);
    }

    [Then("the description displays a required error")]
    public void ThenTheDescriptionDisplaysARequiredError()
    {
        GetForm().FindComponent<GcdsTextarea>().Instance.ErrorMessage
            .Should().Be("Please provide a description.");
    }

    [When("I complete the support request details")]
    public async Task WhenICompleteTheSupportRequestDetails()
    {
        var topics = GetForm().FindComponent<GcdsCheckboxes>();
        await topics.InvokeAsync(() => topics.Instance.ValueChanged.InvokeAsync(["Storage", "Other"]));
        await ClickButtonAsync("Next");

        var description = GetForm().FindComponent<GcdsTextarea>();
        await description.InvokeAsync(() =>
            description.Instance.ValueChanged.InvokeAsync("Storage access is failing."));
        await ClickButtonAsync("Next");

        var workspaces = GetForm().FindComponent<GcdsCheckboxes>();
        await workspaces.InvokeAsync(() =>
            workspaces.Instance.ValueChanged.InvokeAsync(["Research Workspace"]));
        await ClickButtonAsync("Next");

        var language = GetForm().FindComponent<GcdsRadios>();
        await language.InvokeAsync(() => language.Instance.ValueChanged.InvokeAsync("fr"));
    }

    [When("I advance to the support request review")]
    public Task WhenIAdvanceToTheSupportRequestReview() => ClickButtonAsync("Next");

    [Then("the support request review contains my selections")]
    public void ThenTheSupportRequestReviewContainsMySelections()
    {
        var form = GetForm();
        form.FindComponent<GcdsStepper>().Instance.CurrentStep.Should().Be(5);
        form.Markup.Should().Contain("Storage, Other");
        form.Markup.Should().Contain("Research Workspace");
        form.Markup.Should().Contain("Storage access is failing.");
        form.Markup.Should().Contain("French");
    }

    [When("I return to the support request details")]
    public Task WhenIReturnToTheSupportRequestDetails() => ClickButtonAsync("Previous");

    [Then("my support request details are preserved")]
    public async Task ThenMySupportRequestDetailsArePreserved()
    {
        var form = GetForm();
        form.FindComponent<GcdsRadios>().Instance.Value.Should().Be("fr");
        await ClickButtonAsync("Previous");
        form.FindComponent<GcdsCheckboxes>().Instance.Value.Should().BeEquivalentTo("Research Workspace");
        await ClickButtonAsync("Previous");
        form.FindComponent<GcdsTextarea>().Instance.Value.Should().Be("Storage access is failing.");
        await ClickButtonAsync("Previous");
        form.FindComponent<GcdsCheckboxes>().Instance.Value.Should().BeEquivalentTo("Storage", "Other");

        await ClickButtonAsync("Next");
        await ClickButtonAsync("Next");
        await ClickButtonAsync("Next");
    }

    [When("I submit the support request")]
    public Task WhenISubmitTheSupportRequest() => ClickButtonAsync("Submit");

    [Then("the support request data is reported")]
    public void ThenTheSupportRequestDataIsReported()
    {
        _submittedRequest.Should().NotBeNull();
        _submittedRequest!.Topics.Should().BeEquivalentTo("Storage", "Other");
        _submittedRequest.Workspaces.Should().BeEquivalentTo("Research Workspace");
        _submittedRequest.Description.Should().Be("Storage access is failing.");
        _submittedRequest.PreferredLanguage.Should().Be("fr");
    }

    [Then("the support request form is reset")]
    public void ThenTheSupportRequestFormIsReset()
    {
        var form = GetForm();
        form.FindComponents<GcdsStepper>().Should().BeEmpty();
        form.Markup.Should().Contain("Get Started");
    }

    private async Task ClickButtonAsync(string text)
    {
        var button = GetForm().FindComponents<GcdsButton>()
            .Single(candidate => candidate.Markup.Contains(text, StringComparison.Ordinal));
        await button.InvokeAsync(() => button.Instance.OnClick.InvokeAsync());
    }

    private IRenderedComponent<SupportRequestForm> GetForm()
    {
        return _form ?? throw new InvalidOperationException("The support request form has not been rendered.");
    }

    void IDisposable.Dispose()
    {
        // BunitTestSteps disposes the context asynchronously in its AfterScenario hook.
    }
}
