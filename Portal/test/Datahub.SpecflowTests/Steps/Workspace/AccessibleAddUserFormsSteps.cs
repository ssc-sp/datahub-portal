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
    private IMSGraphService? _graphService;
    private IProjectUserManagementService? _projectUserManagementService;
    private bool _entraFormCompleted;
    private bool _cancelled;
    private IDbContextFactory<DatahubProjectDBContext>? _externalDbContextFactory;

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
        _graphService = Substitute.For<IMSGraphService>();
        _graphService.GetUsersListAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, GraphUser> { [graphUser.Id] = graphUser });
        Services.AddSingleton(_graphService);
        var userInformationService = Substitute.For<IUserInformationService>();
        userInformationService.GetCurrentPortalUserAsync().Returns(new PortalUser
        {
            Id = 7,
            Email = "workspace.admin@example.gc.ca"
        });
        userInformationService.GetEntraUserAsync(graphUser.Id).Returns(new PortalUser
        {
            Email = graphUser.Mail,
            DisplayName = graphUser.DisplayName
        });
        Services.AddSingleton(userInformationService);
        _projectUserManagementService = Substitute.For<IProjectUserManagementService>();
        _projectUserManagementService.ProcessProjectUserCommandsAsync(
                Arg.Any<List<ProjectUserUpdateCommand>>(),
                Arg.Any<List<ProjectUserAddEntraUserCommand>>(),
                Arg.Any<string>())
            .Returns(true);
        Services.AddSingleton(_projectUserManagementService);
        Services.AddSingleton(Substitute.For<ISnackbar>());
        var cultureService = Substitute.For<ICultureService>();
        cultureService.IsFrench.Returns(false);
        Services.AddSingleton(cultureService);

        _entraForm = Render<AddNewEntraUsersToProjectForm>(parameters => parameters
            .Add(form => form.ProjectAcronym, "TEST")
            .Add(form => form.CurrentProjectUsers, [])
            .Add(form => form.OnCompleted, () => _entraFormCompleted = true)
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
        _externalDbContextFactory = new SpecFlowDbContextFactory(options);
        Services.AddSingleton<IDbContextFactory<DatahubProjectDBContext>>(_externalDbContextFactory);
        Services.AddSingleton(Substitute.For<IUserInformationService>());
        Services.AddSingleton(Substitute.For<IExternalUserInvitationService>());
        Services.AddSingleton(Substitute.For<ISnackbar>());
        var cultureService = Substitute.For<ICultureService>();
        cultureService.IsFrench.Returns(false);
        Services.AddSingleton(cultureService);

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

    [Then("the Entra add-user form announces step 1 of 3")]
    public void ThenTheEntraAddUserFormAnnouncesStepOneOfThree()
    {
        ArgumentNullException.ThrowIfNull(_entraForm);
        var stepper = _entraForm.FindComponent<GcdsStepper>();
        stepper.Instance.CurrentStep.Should().Be(1);
        stepper.Instance.TotalSteps.Should().Be(3);
        stepper.Markup.Should().Contain("Enter the user's GC email address");
    }

    [Then("the add-user form actions use button semantics")]
    public void ThenTheAddUserFormActionsUseButtonSemantics()
    {
        var buttons = GetButtonsInCurrentForm();
        var cancelButton = buttons.Single(button => button.Markup.Contains("Cancel", StringComparison.Ordinal));
        cancelButton.Instance.Type.Should().Be(GcdsButtonType.Button);

        var forwardButton = buttons.Single(button =>
            button.Markup.Contains("Next", StringComparison.Ordinal));
        forwardButton.Instance.Type.Should().Be(GcdsButtonType.Button);
    }

    [Then("the form uses GCDS inputs and buttons")]
    public void ThenTheFormUsesGcdsInputsAndButtons()
    {
        if (_entraForm is not null)
        {
            _entraForm.FindComponents<MudAutocomplete<string>>().Should().BeEmpty();
            _entraForm.FindComponents<GcdsInput>().Should().ContainSingle();
            _entraForm.FindComponents<GcdsButton>().Should().HaveCount(2);
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
        await EnterEntraEmailAsync("test.user@example.gc.ca");
    }

    [When("the nonexistent Entra user email {string} is entered")]
    public async Task WhenTheNonexistentEntraUserEmailIsEntered(string email)
    {
        ArgumentNullException.ThrowIfNull(_graphService);
        _graphService.GetUsersListAsync(email, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, GraphUser>());

        await EnterEntraEmailAsync(email);
    }

    [When("the invalid GC email {string} is entered")]
    public async Task WhenTheInvalidGcEmailIsEntered(string email)
    {
        await EnterEntraEmailAsync(email);
    }

    [Then("the invalid GC email error is displayed")]
    public void ThenTheInvalidGcEmailErrorIsDisplayed()
    {
        ArgumentNullException.ThrowIfNull(_entraForm);
        var errorMessage = _entraForm.FindComponent<GcdsErrorMessage>();
        errorMessage.Instance.MessageId.Should().Be("entra-user-email-error");
        errorMessage.Markup.Should().Contain("Please enter a valid GC email address");
    }

    [Then("the Entra form remains on the email step")]
    public void ThenTheEntraFormRemainsOnTheEmailStep()
    {
        ArgumentNullException.ThrowIfNull(_entraForm);
        _entraForm.FindComponent<GcdsStepper>().Instance.CurrentStep.Should().Be(1);
        _entraForm.FindComponents<GcdsSelect>().Should().BeEmpty();
    }

    [Then("Entra ID was not searched")]
    public void ThenEntraIdWasNotSearched()
    {
        ArgumentNullException.ThrowIfNull(_graphService);
        _graphService.DidNotReceive()
            .GetUsersListAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Then("the Entra role step is displayed")]
    public void ThenTheEntraRoleStepIsDisplayed()
    {
        ArgumentNullException.ThrowIfNull(_entraForm);
        var stepper = _entraForm.FindComponent<GcdsStepper>();
        stepper.Instance.CurrentStep.Should().Be(2);
        stepper.Markup.Should().Contain("Select the user's role");
        _entraForm.FindComponent<GcdsSelect>().Instance.Id.Should().Be("entra-user-role");
        _entraForm.Markup.Should().Contain("Test User");
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

    [When("the Entra add-user form advances to review")]
    public async Task WhenTheEntraAddUserFormAdvancesToReview()
    {
        ArgumentNullException.ThrowIfNull(_entraForm);
        var button = FindButton(_entraForm, "Next");
        await button.InvokeAsync(() => button.Instance.OnClick.InvokeAsync(default));
    }

    [Then("the Entra user and role are shown for review")]
    public void ThenTheEntraUserAndRoleAreShownForReview()
    {
        ArgumentNullException.ThrowIfNull(_entraForm);
        var stepper = _entraForm.FindComponent<GcdsStepper>();
        stepper.Instance.CurrentStep.Should().Be(3);
        stepper.Markup.Should().Contain("Review and confirm");
        _entraForm.Markup.Should().Contain("Test User");
        _entraForm.Markup.Should().Contain("test.user@example.gc.ca");
        _entraForm.Markup.Should().Contain("Collaborator");
    }

    [When("the Entra add-user form is submitted")]
    public async Task WhenTheEntraAddUserFormIsSubmitted()
    {
        ArgumentNullException.ThrowIfNull(_entraForm);
        var button = _entraForm.FindComponents<GcdsButton>()
            .Single(candidate => candidate.Markup.Contains("Confirm and add user", StringComparison.Ordinal));
        await button.InvokeAsync(() => button.Instance.OnClick.InvokeAsync(default));
    }

    [Then("the Entra user is submitted as a Collaborator")]
    public void ThenTheEntraUserIsSubmittedAsACollaborator()
    {
        ArgumentNullException.ThrowIfNull(_projectUserManagementService);
        _projectUserManagementService.Received(1).ProcessProjectUserCommandsAsync(
            Arg.Is<List<ProjectUserUpdateCommand>>(updates => updates.Count == 0),
            Arg.Is<List<ProjectUserAddEntraUserCommand>>(users =>
                users.Count == 1
                && users[0].Email == "test.user@example.gc.ca"
                && users[0].RoleId == (int)Project_Role.RoleNames.Collaborator),
            "7");
        _entraFormCompleted.Should().BeTrue();
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

    [Given("a deactivated external user with email {string} exists")]
    public async Task GivenADeactivatedExternalUserExists(string email)
    {
        ArgumentNullException.ThrowIfNull(_externalDbContextFactory);
        await using var context = await _externalDbContextFactory.CreateDbContextAsync();
        var portalUser = new PortalUser
        {
            Email = email,
            DisplayName = "Deactivated User"
        };
        portalUser.ExternalUser = new ExternalUser
        {
            FirstName = "Deactivated",
            LastName = "User",
            Organization = "SSC",
            UserExpiryDate = DateTimeOffset.UtcNow.AddDays(-1),
            UserDeactivatedAt = DateTimeOffset.UtcNow.AddDays(-2),
            PortalUser = portalUser
        };
        context.PortalUsers.Add(portalUser);
        await context.SaveChangesAsync();
    }

    [When("the external email {string} is entered")]
    public async Task WhenTheExternalEmailIsEntered(string email)
    {
        ArgumentNullException.ThrowIfNull(_externalForm);
        var emailField = _externalForm.FindComponent<GcdsInput>();
        await emailField.InvokeAsync(() => emailField.Instance.ValueChanged.InvokeAsync(email));
    }

    [Then("the form identifies a deactivated user")]
    public void ThenTheFormIdentifiesADeactivatedUser()
    {
        ArgumentNullException.ThrowIfNull(_externalForm);
        _externalForm.WaitForAssertion(() =>
        {
            _externalForm.Markup.Should().Contain("Deactivated user found");
            _externalForm.Markup.Should().Contain("Access will be restored after the invitation is accepted");
        });
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
    public void ThenTheExternalAccountExpiryUsesAGcdsDateInput()
    {
        ArgumentNullException.ThrowIfNull(_externalForm);
        _externalForm.FindComponents<MudDatePicker>().Should().BeEmpty();

        var dateInput = _externalForm.FindComponent<GcdsDateInput>();
        dateInput.Instance.Name.Should().Be("external-user-account-expiry");
        dateInput.Instance.ValueExpression.Should().NotBeNull();
        dateInput.Instance.ValueExpression!.Body.NodeType.Should()
            .Be(System.Linq.Expressions.ExpressionType.MemberAccess);
        dateInput.Instance.Min.Should().Be(
            DateTime.Today.AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
    }

    [Then("the reactivation account expiry is editable and in the future")]
    public void ThenTheReactivationAccountExpiryIsEditableAndInTheFuture()
    {
        ArgumentNullException.ThrowIfNull(_externalForm);
        var dateInput = _externalForm.FindComponent<GcdsDateInput>();
        dateInput.Instance.Disabled.Should().BeFalse();
        DateTime.ParseExact(dateInput.Instance.Value!, "yyyy-MM-dd", CultureInfo.InvariantCulture)
            .Should().BeAfter(DateTime.Today);
    }

    [Then("the external account expiry only accepts future dates")]
    public async Task ThenTheExternalAccountExpiryOnlyAcceptsFutureDates()
    {
        ArgumentNullException.ThrowIfNull(_externalForm);

        await SetExternalUserDetailAsync("external-user-first-name", "Jane");
        await SetExternalUserDetailAsync("external-user-last-name", "Doe");
        await SetExternalUserDetailAsync("external-user-organization", "SSC");

        var dateInput = _externalForm.FindComponent<GcdsDateInput>();
        var invalidDates = new[] { DateTime.Today.AddDays(-1), DateTime.Today };

        foreach (var invalidDate in invalidDates)
        {
            var value = invalidDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            await dateInput.InvokeAsync(() => dateInput.Instance.ValueChanged.InvokeAsync(value));

            _externalForm.WaitForAssertion(() =>
            {
                FindButton(_externalForm, "Next").Instance.Disabled.Should().BeTrue();
                _externalForm.FindComponent<GcdsDateInput>().Instance.ErrorMessage.Should()
                    .Be("Account expiry date must be in the future");
            });
        }

        var expiryDate = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        await dateInput.InvokeAsync(() => dateInput.Instance.ValueChanged.InvokeAsync(expiryDate));
        _externalForm.WaitForAssertion(() =>
        {
            _externalForm.FindComponent<GcdsDateInput>().Instance.Value.Should().Be(expiryDate);
            FindButton(_externalForm, "Next").Instance.Disabled.Should().BeFalse();
        });
    }

    private async Task SetExternalUserDetailAsync(string id, string value)
    {
        ArgumentNullException.ThrowIfNull(_externalForm);
        var input = _externalForm.FindComponents<GcdsInput>().Single(component => component.Instance.Id == id);
        await input.InvokeAsync(() => input.Instance.ValueChanged.InvokeAsync(value));
    }

    private static IRenderedComponent<GcdsButton> FindButton(
        IRenderedComponent<AddNewExternalUsersToProjectForm> form, string text)
        => form.FindComponents<GcdsButton>()
            .Single(button => button.Markup.Contains(text, StringComparison.Ordinal));

    private static IRenderedComponent<GcdsButton> FindButton(
        IRenderedComponent<AddNewEntraUsersToProjectForm> form, string text)
        => form.FindComponents<GcdsButton>()
            .Single(button => button.Markup.Contains(text, StringComparison.Ordinal));

    private async Task EnterEntraEmailAsync(string email)
    {
        ArgumentNullException.ThrowIfNull(_entraForm);
        var emailInput = _entraForm.FindComponent<GcdsInput>();

        await emailInput.InvokeAsync(() => emailInput.Instance.ValueChanged.InvokeAsync(email));
        var nextButton = FindButton(_entraForm, "Next");
        await nextButton.InvokeAsync(() => nextButton.Instance.OnClick.InvokeAsync(default));
    }

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
