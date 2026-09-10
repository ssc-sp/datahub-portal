using AngleSharp.Dom;
using Bunit;
using Datahub.Application.Services;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Users;
using Datahub.Portal.Pages.Workspace.Users;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps.Workspace;

[Binding]
public class AccessibleAddUserFormsSteps : BunitTestSteps, IDisposable
{
    private IRenderedComponent<AddNewEntraUsersToProjectForm>? _entraForm;
    private IRenderedComponent<AddNewExternalUsersToProjectForm>? _externalForm;
    private bool _cancelled;

    [Given("the Entra add-user form is rendered")]
    public void GivenTheEntraAddUserFormIsRendered()
    {
        ConfigureCommonServices();
        Services.AddSingleton(Substitute.For<IMSGraphService>());
        Services.AddSingleton(Substitute.For<IUserEnrollmentService>());
        Services.AddSingleton(Substitute.For<IUserInformationService>());

        _entraForm = Render<AddNewEntraUsersToProjectForm>(parameters => parameters
            .Add(form => form.ProjectAcronym, "TEST")
            .Add(form => form.CurrentProjectUsers, [])
            .Add(form => form.OnCancelled, () => _cancelled = true));
    }

    [Given("the external add-user form is rendered")]
    public void GivenTheExternalAddUserFormIsRendered()
    {
        ConfigureCommonServices();
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        Services.AddSingleton<IDbContextFactory<DatahubProjectDBContext>>(new SpecFlowDbContextFactory(options));
        Services.AddSingleton(Substitute.For<IUserInformationService>());
        Services.AddSingleton(Substitute.For<IExternalUserInvitationService>());
        Services.AddSingleton(Substitute.For<ISnackbar>());

        _externalForm = Render<AddNewExternalUsersToProjectForm>(parameters => parameters
            .Add(form => form.ProjectAcronym, "TEST")
            .Add(form => form.Inviter, new PortalUser { Id = 1, Email = "lead@example.gc.ca" })
            .Add(form => form.GetGcInvitationLink, _ => ("https://example.test/en", "https://example.test/fr"))
            .Add(form => form.OnCancelled, () => _cancelled = true));
    }

    [Then("the add-user form is not a dialog")]
    public void ThenTheAddUserFormIsNotADialog()
    {
        FindAllInCurrentForm(".mud-dialog").Should().BeEmpty();
        FindAllInCurrentForm("[role='dialog']").Should().BeEmpty();
    }

    [Then("the Entra add-user form is labelled by its heading")]
    public void ThenTheEntraAddUserFormIsLabelledByItsHeading()
    {
        ArgumentNullException.ThrowIfNull(_entraForm);
        var form = _entraForm;
        var region = form.Find("section[role='region']");
        region.GetAttribute("aria-labelledby").Should().Be("entra-invite-heading");
        form.Find("#entra-invite-heading").TextContent.Trim().Should().Be("Invite New Users");
    }

    [Then("the external add-user form is labelled by its heading")]
    public void ThenTheExternalAddUserFormIsLabelledByItsHeading()
    {
        ArgumentNullException.ThrowIfNull(_externalForm);
        var form = _externalForm;
        var region = form.Find("section[role='region']");
        region.GetAttribute("aria-labelledby").Should().Be("external-invite-heading");
        form.Find("#external-invite-heading").TextContent.Trim().Should().Be("Invite New Users");
    }

    [Then("the external add-user form announces step 1 of 4")]
    public void ThenTheExternalAddUserFormAnnouncesStepOneOfFour()
    {
        ArgumentNullException.ThrowIfNull(_externalForm);
        var form = _externalForm;
        form.Find("[role='status']").TextContent.Should().Contain("Step 1 of 4");
        form.Find("h3").TextContent.Trim().Should().Be("Enter the user's primary email address");
    }

    [Then("the add-user form actions use button semantics")]
    public void ThenTheAddUserFormActionsUseButtonSemantics()
    {
        var buttons = FindAllInCurrentForm("button");
        var cancelButton = buttons.Single(button => button.TextContent.Contains("Cancel", StringComparison.Ordinal));
        cancelButton.GetAttribute("type").Should().Be("button");

        var forwardButton = buttons.Single(button =>
            button.TextContent.Contains(_entraForm is not null ? "Add New Users" : "Next", StringComparison.Ordinal));
        forwardButton.GetAttribute("type").Should().Be("button");
    }

    [When("the user cancels the add-user form")]
    public void WhenTheUserCancelsTheAddUserForm()
    {
        FindAllInCurrentForm("button")
            .Single(button => button.TextContent.Contains("Cancel", StringComparison.Ordinal))
            .Click();
    }

    [Then("the add-user form reports that it was cancelled")]
    public void ThenTheAddUserFormReportsThatItWasCancelled()
    {
        _cancelled.Should().BeTrue();
    }

    private IEnumerable<IElement> FindAllInCurrentForm(string selector)
    {
        if (_entraForm is not null)
        {
            return _entraForm.FindAll(selector);
        }

        if (_externalForm is not null)
        {
            return _externalForm.FindAll(selector);
        }

        throw new InvalidOperationException("The add-user form has not been rendered.");
    }

    private void ConfigureCommonServices()
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
    }

    void IDisposable.Dispose()
    {
        // BunitTestSteps disposes the context asynchronously in its AfterScenario hook.
    }
}
