using AngleSharp.Dom;
using Bunit;
using Datahub.Application.Commands;
using Datahub.Application.Services;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Data;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Users;
using Datahub.Portal.Pages.Workspace.Users;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using GcdsWrapper.Blazor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using Reqnroll;
using System.Globalization;
using System.Net.Mail;

namespace Datahub.SpecflowTests.Steps.Workspace;

[Binding]
public class AccessibleAddUserFormsSteps : BunitTestSteps, IDisposable
{
    private IRenderedComponent<AddNewEntraUsersToProjectForm>? _entraForm;
    private IRenderedComponent<AddNewExternalUsersToProjectForm>? _externalForm;
    private List<ProjectUserAddEntraUserCommand>? _completedEntraUsers;
    private bool _cancelled;

    [Given("the Entra add-user form is rendered")]
    public void GivenTheEntraAddUserFormIsRendered()
    {
        ConfigureCommonServices();
        var graphUser = new GraphUser
        {
            Id = "graph-user-id",
            DisplayName = "Test User",
            MailAddress = new MailAddress("test.user@example.gc.ca")
        };
        var graphService = Substitute.For<IMSGraphService>();
        graphService.GetUsersListAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, GraphUser> { [graphUser.Id] = graphUser });
        Services.AddSingleton(graphService);
        Services.AddSingleton(Substitute.For<IUserEnrollmentService>());
        Services.AddSingleton(Substitute.For<IUserInformationService>());

        _entraForm = Render<AddNewEntraUsersToProjectForm>(parameters => parameters
            .Add(form => form.ProjectAcronym, "TEST")
            .Add(form => form.CurrentProjectUsers, [])
            .Add(form => form.OnCompleted, users => _completedEntraUsers = users)
            .Add(form => form.OnCancelled, () => _cancelled = true));
    }

    [Given("the external add-user form is rendered")]
    public void GivenTheExternalAddUserFormIsRendered()
    {
        ConfigureCommonServices();
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using (var context = new DatahubProjectDBContext(options))
        {
            context.Project_Roles.AddRange(Project_Role.GetAll().Where(role => role.IsExternalRole));
            context.SaveChanges();
        }
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
        var stepper = _externalForm.FindComponent<GcdsStepper>();
        stepper.Instance.CurrentStep.Should().Be(1);
        stepper.Instance.TotalSteps.Should().Be(4);
        stepper.Markup.Should().Contain("Enter the user's primary email address");
    }

    [Then("the add-user form actions use button semantics")]
    public void ThenTheAddUserFormActionsUseButtonSemantics()
    {
        var buttons = GetButtonsInCurrentForm();
        var cancelButton = buttons.Single(button => button.Markup.Contains("Cancel", StringComparison.Ordinal));
        cancelButton.Instance.Type.Should().Be(GcdsButtonType.Button);

        var forwardButton = buttons.Single(button =>
            button.Markup.Contains(_entraForm is not null ? "Add New Users" : "Next", StringComparison.Ordinal));
        forwardButton.Instance.Type.Should().Be(GcdsButtonType.Button);
    }

    [Then("the form uses GCDS inputs and buttons")]
    public void ThenTheFormUsesGcdsInputsAndButtons()
    {
        if (_entraForm is not null)
        {
            _entraForm.FindComponents<MudAutocomplete<string>>().Should().BeEmpty();
            _entraForm.FindComponents<GcdsInput>().Should().ContainSingle();
            _entraForm.FindComponents<GcdsButton>().Should().HaveCount(3);
            return;
        }

        ArgumentNullException.ThrowIfNull(_externalForm);
        _externalForm.FindComponents<MudTextField<string>>().Should().BeEmpty();
        _externalForm.FindComponents<GcdsInput>().Should().ContainSingle();
        _externalForm.FindComponents<GcdsButton>().Should().HaveCount(2);
    }

    [When("the user cancels the add-user form")]
    public async Task WhenTheUserCancelsTheAddUserForm()
    {
        var button = GetButtonsInCurrentForm()
            .Single(candidate => candidate.Markup.Contains("Cancel", StringComparison.Ordinal));
        await button.InvokeAsync(() => button.Instance.OnClick.InvokeAsync(default));
    }

    [Then("the add-user form reports that it was cancelled")]
    public void ThenTheAddUserFormReportsThatItWasCancelled()
    {
        _cancelled.Should().BeTrue();
    }

    [When("an Entra user is selected")]
    public async Task WhenAnEntraUserIsSelected()
    {
        ArgumentNullException.ThrowIfNull(_entraForm);
        var emailInput = _entraForm.FindComponent<GcdsInput>();
        const string email = "test.user@example.gc.ca";

        await emailInput.InvokeAsync(() => emailInput.Instance.ValueChanged.InvokeAsync(email));
        var addButton = _entraForm.FindComponents<GcdsButton>()
            .Single(button => button.Markup.Contains("Add user", StringComparison.Ordinal));
        await addButton.InvokeAsync(() => addButton.Instance.OnClick.InvokeAsync(default));
    }

    [Then("the pending Entra user is displayed as an accessible list item")]
    public void ThenThePendingEntraUserIsDisplayedAsAnAccessibleListItem()
    {
        ArgumentNullException.ThrowIfNull(_entraForm);
        _entraForm.FindAll("table").Should().BeEmpty();

        var heading = _entraForm.Find("#pending-users-heading");
        heading.TagName.Should().Be("GCDS-HEADING");
        heading.GetAttribute("tag").Should().Be("h3");
        heading.TextContent.Trim().Should().Be("Users to be added:");

        var list = _entraForm.Find("ul.pending-user-list");
        list.GetAttribute("aria-labelledby").Should().Be("pending-users-heading");
        var listItem = list.Children.Should().ContainSingle().Which;
        listItem.TagName.Should().Be("LI");
        var legend = listItem.QuerySelector("fieldset legend");
        legend.Should().NotBeNull();
        legend!.TextContent.Trim().Should().Be("Test User");
        listItem.QuerySelector("gcds-select").Should().NotBeNull();
    }

    [When("the Entra user's role is changed to Collaborator")]
    public async Task WhenTheEntraUsersRoleIsChangedToCollaborator()
    {
        ArgumentNullException.ThrowIfNull(_entraForm);
        var roleSelect = _entraForm.FindComponent<GcdsSelect>();
        roleSelect.Instance.ValueExpression.Should().NotBeNull();
        roleSelect.Instance.ValueExpression!.Body.NodeType.Should().Be(System.Linq.Expressions.ExpressionType.MemberAccess);

        var collaboratorRoleId = ((int)Project_Role.RoleNames.Collaborator).ToString(CultureInfo.InvariantCulture);
        await roleSelect.InvokeAsync(() => roleSelect.Instance.ValueChanged.InvokeAsync(collaboratorRoleId));
    }

    [When("the Entra add-user form is submitted")]
    public async Task WhenTheEntraAddUserFormIsSubmitted()
    {
        ArgumentNullException.ThrowIfNull(_entraForm);
        var button = _entraForm.FindComponents<GcdsButton>()
            .Single(candidate => candidate.Markup.Contains("Add New Users", StringComparison.Ordinal));
        await button.InvokeAsync(() => button.Instance.OnClick.InvokeAsync(default));
    }

    [Then("the Entra user is submitted as a Collaborator")]
    public void ThenTheEntraUserIsSubmittedAsACollaborator()
    {
        _completedEntraUsers.Should().ContainSingle()
            .Which.RoleId.Should().Be((int)Project_Role.RoleNames.Collaborator);
    }

    [When("a valid external email address is entered")]
    public async Task WhenAValidExternalEmailAddressIsEntered()
    {
        ArgumentNullException.ThrowIfNull(_externalForm);
        var emailField = _externalForm.FindComponent<GcdsInput>();
        await emailField.InvokeAsync(() => emailField.Instance.ValueChanged.InvokeAsync("external.user@example.com"));

        _externalForm.WaitForAssertion(() =>
            FindButton(_externalForm, "Next").Instance.Disabled.Should().BeFalse());
    }

    [When("the external add-user form advances to user details")]
    public async Task WhenTheExternalAddUserFormAdvancesToUserDetails()
    {
        ArgumentNullException.ThrowIfNull(_externalForm);
        var button = FindButton(_externalForm, "Next");
        await button.InvokeAsync(() => button.Instance.OnClick.InvokeAsync(default));
        _externalForm.WaitForAssertion(() =>
        {
            var stepper = _externalForm.FindComponent<GcdsStepper>();
            stepper.Instance.CurrentStep.Should().Be(2);
            stepper.Markup.Should().Contain("Enter the user details");
        });
    }

    [Then("the external role is selected with a GCDS select")]
    public async Task ThenTheExternalRoleIsSelectedWithAGcdsSelect()
    {
        ArgumentNullException.ThrowIfNull(_externalForm);
        _externalForm.FindComponents<MudSelect<Project_Role>>().Should().BeEmpty();

        var roleSelect = _externalForm.FindComponent<GcdsSelect>();
        roleSelect.Instance.Id.Should().Be("external-user-role");
        roleSelect.Instance.ValueExpression.Should().NotBeNull();
        roleSelect.Instance.ValueExpression!.Body.NodeType.Should().Be(System.Linq.Expressions.ExpressionType.MemberAccess);

        var roleId = ((int)Project_Role.RoleNames.WebApp).ToString(CultureInfo.InvariantCulture);
        await roleSelect.InvokeAsync(() => roleSelect.Instance.ValueChanged.InvokeAsync(roleId));
        _externalForm.WaitForAssertion(() =>
            _externalForm.FindComponent<GcdsSelect>().Instance.Value.Should().Be(roleId));
    }

    [Then("the external account expiry uses a GCDS date input")]
    public async Task ThenTheExternalAccountExpiryUsesAGcdsDateInput()
    {
        ArgumentNullException.ThrowIfNull(_externalForm);
        _externalForm.FindComponents<MudDatePicker>().Should().BeEmpty();

        var dateInput = _externalForm.FindComponent<GcdsDateInput>();
        dateInput.Instance.Name.Should().Be("external-user-account-expiry");
        dateInput.Instance.ValueExpression.Should().NotBeNull();
        dateInput.Instance.ValueExpression!.Body.NodeType.Should()
            .Be(System.Linq.Expressions.ExpressionType.MemberAccess);

        const string expiryDate = "2030-12-31";
        await dateInput.InvokeAsync(() => dateInput.Instance.ValueChanged.InvokeAsync(expiryDate));
        _externalForm.WaitForAssertion(() =>
            _externalForm.FindComponent<GcdsDateInput>().Instance.Value.Should().Be(expiryDate));
    }

    private static IRenderedComponent<GcdsButton> FindButton(
        IRenderedComponent<AddNewExternalUsersToProjectForm> form, string text)
        => form.FindComponents<GcdsButton>()
            .Single(button => button.Markup.Contains(text, StringComparison.Ordinal));

    private IReadOnlyList<IRenderedComponent<GcdsButton>> GetButtonsInCurrentForm()
    {
        if (_entraForm is not null)
        {
            return _entraForm.FindComponents<GcdsButton>();
        }

        if (_externalForm is not null)
        {
            return _externalForm.FindComponents<GcdsButton>();
        }

        throw new InvalidOperationException("The add-user form has not been rendered.");
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
