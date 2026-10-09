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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using Reqnroll;
using System.Globalization;
using System.Net.Mail;
using System.Reflection;

namespace Datahub.SpecflowTests.Steps.Workspace;

[Binding]
public class AccessibleAddUserFormsSteps : BunitTestSteps, IDisposable
{
    private IRenderedComponent<AddNewEntraUsersToProjectForm>? _entraForm;
    private IRenderedComponent<AddNewExternalUsersToProjectForm>? _externalForm;
    private IRenderedComponent<ManageExternalUserDialog>? _manageExternalUserDialog;
    private IRenderedComponent<ExtendExternalUserDialog>? _extendExternalUserDialog;
    private List<ProjectUserAddEntraUserCommand>? _completedEntraUsers;
    private IDbContextFactory<DatahubProjectDBContext>? _externalDbContextFactory;
    private IExternalUserInvitationService? _externalInvitationService;
    private bool _cancelled;
    private bool _externalInviteCompleted;
    private DateTime _selectedExpiryDate;
    private ExternalUser? _dialogExternalUser;
    private IMudDialogInstance? _dialogInstance;

    private const string ExistingExternalUserEmail = "existing.external@example.com";

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
            context.Projects.Add(new Datahub_Project
            {
                Project_Acronym_CD = "TEST",
                Project_Name = "Test workspace",
                Project_Name_Fr = "Espace de travail de test"
            });
            context.SaveChanges();
        }
        _externalDbContextFactory = new SpecFlowDbContextFactory(options);
        Services.AddSingleton(_externalDbContextFactory);
        Services.AddSingleton(Substitute.For<IUserInformationService>());
        _externalInvitationService = Substitute.For<IExternalUserInvitationService>();
        _externalInvitationService.CreateInvitationAsync(
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<PortalUser>(),
                Arg.Any<Func<WorkspaceInvitation, (string enURL, string frURL)>>(),
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<CancellationToken>())
            .Returns(new WorkspaceInvitation
            {
                InvitationToken = Guid.NewGuid(),
                InvitedEmail = ExistingExternalUserEmail,
                InvitationRationale_EN = "Initial Invitation",
                Requested_Role = Project_Role.GetAll().First(role => role.IsExternalRole)
            });
        Services.AddSingleton(_externalInvitationService);
        Services.AddSingleton(Substitute.For<ISnackbar>());
        var cultureService = Substitute.For<ICultureService>();
        cultureService.IsFrench.Returns(false);
        Services.AddSingleton(cultureService);

        _externalForm = Render<AddNewExternalUsersToProjectForm>(parameters => parameters
            .Add(form => form.ProjectAcronym, "TEST")
            .Add(form => form.Inviter, new PortalUser { Id = 1, Email = "lead@example.gc.ca" })
            .Add(form => form.GetGcInvitationLink, _ => ("https://example.test/en", "https://example.test/fr"))
            .Add(form => form.OnCompleted, () => _externalInviteCompleted = true)
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
        dateInput.Instance.Value.Should().BeNull();
        dateInput.Instance.Min.Should().Be(DateTime.Today.AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        dateInput.Instance.ValueExpression.Should().NotBeNull();
        dateInput.Instance.ValueExpression!.Body.NodeType.Should()
            .Be(System.Linq.Expressions.ExpressionType.MemberAccess);

        const string expiryDate = "2030-12-31";
        await dateInput.InvokeAsync(() => dateInput.Instance.ValueChanged.InvokeAsync(expiryDate));
        _externalForm.WaitForAssertion(() =>
            _externalForm.FindComponent<GcdsDateInput>().Instance.Value.Should().Be(expiryDate));
    }

    [When("valid external user details are entered with an expiry (.*)")]
    public async Task WhenValidExternalUserDetailsAreEnteredWithAnExpiry(string expiry)
    {
        ArgumentNullException.ThrowIfNull(_externalForm);

        await SetInputValueAsync("external-user-first-name", "Jane");
        await SetInputValueAsync("external-user-last-name", "Doe");
        await SetInputValueAsync("external-user-organization", "SSC");

        var roleId = ((int)Project_Role.RoleNames.WebApp).ToString(CultureInfo.InvariantCulture);
        var roleSelect = _externalForm.FindComponent<GcdsSelect>();
        await roleSelect.InvokeAsync(() => roleSelect.Instance.ValueChanged.InvokeAsync(roleId));

        _selectedExpiryDate = expiry switch
        {
            "past" => DateTime.Today.AddDays(-1),
            "today" => DateTime.Today,
            "tomorrow" => DateTime.Today.AddDays(1),
            _ => throw new InvalidOperationException($"Unknown expiry value '{expiry}'.")
        };

        var dateValue = _selectedExpiryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var dateInput = _externalForm.FindComponent<GcdsDateInput>();
        await dateInput.InvokeAsync(() => dateInput.Instance.ValueChanged.InvokeAsync(dateValue));
    }

    [Then("the external expiry validation is (.*)")]
    public void ThenTheExternalExpiryValidationIs(string validation)
    {
        ArgumentNullException.ThrowIfNull(_externalForm);
        var dateInput = _externalForm.FindComponent<GcdsDateInput>();
        var nextButton = FindButton(_externalForm, "Next");

        if (validation == "accepted")
        {
            dateInput.Instance.ErrorMessage.Should().BeNull();
            nextButton.Instance.Disabled.Should().BeFalse();
            return;
        }

        dateInput.Instance.ErrorMessage.Should().Be("Account expiry date must be in the future.");
        nextButton.Instance.Disabled.Should().BeTrue();
    }

    [Given("an existing external user has an expired account")]
    public async Task GivenAnExistingExternalUserHasAnExpiredAccount()
    {
        ArgumentNullException.ThrowIfNull(_externalDbContextFactory);
        await using var context = await _externalDbContextFactory.CreateDbContextAsync();
        var portalUser = new PortalUser
        {
            Email = ExistingExternalUserEmail,
            DisplayName = "Existing External"
        };
        var externalUser = new ExternalUser
        {
            FirstName = "Existing",
            LastName = "External",
            Organization = "SSC",
            UserExpiryDate = DateTimeOffset.UtcNow.AddDays(-1),
            PortalUser = portalUser
        };
        portalUser.ExternalUser = externalUser;
        context.PortalUsers.Add(portalUser);
        await context.SaveChangesAsync();
    }

    [When("the existing external user's email address is entered")]
    public async Task WhenTheExistingExternalUsersEmailAddressIsEntered()
    {
        ArgumentNullException.ThrowIfNull(_externalForm);
        var emailField = _externalForm.FindComponent<GcdsInput>();
        await emailField.InvokeAsync(() => emailField.Instance.ValueChanged.InvokeAsync(ExistingExternalUserEmail));
        _externalForm.WaitForAssertion(() => FindButton(_externalForm, "Next").Instance.Disabled.Should().BeFalse());
    }

    [Then("the existing external user's expiry is editable")]
    public void ThenTheExistingExternalUsersExpiryIsEditable()
    {
        ArgumentNullException.ThrowIfNull(_externalForm);
        _externalForm.FindComponent<GcdsDateInput>().Instance.Disabled.Should().BeFalse();
    }

    [When("the external invitation is completed")]
    public async Task WhenTheExternalInvitationIsCompleted()
    {
        ArgumentNullException.ThrowIfNull(_externalForm);

        var nextButton = FindButton(_externalForm, "Next");
        await nextButton.InvokeAsync(() => nextButton.Instance.OnClick.InvokeAsync(default));

        await SetInputValueAsync("external-user-collaboration-objectives", "Collaborate on a shared deliverable.");
        nextButton = FindButton(_externalForm, "Next");
        await nextButton.InvokeAsync(() => nextButton.Instance.OnClick.InvokeAsync(default));

        var submitButton = FindButton(_externalForm, "Send user invite");
        await submitButton.InvokeAsync(() => submitButton.Instance.OnClick.InvokeAsync(default));
    }

    [Then("the existing external user's future expiry is persisted")]
    public async Task ThenTheExistingExternalUsersFutureExpiryIsPersisted()
    {
        ArgumentNullException.ThrowIfNull(_externalForm);
        ArgumentNullException.ThrowIfNull(_externalDbContextFactory);

        _externalForm.WaitForAssertion(() => _externalInviteCompleted.Should().BeTrue());
        await using var context = await _externalDbContextFactory.CreateDbContextAsync();
        var externalUser = await context.ExternalUsers.SingleAsync(user => user.PortalUser.Email == ExistingExternalUserEmail);
        externalUser.UserExpiryDate.Should().Be(
            new DateTimeOffset(DateTime.SpecifyKind(_selectedExpiryDate.Date, DateTimeKind.Utc)));
    }

    [Given("the manage external user expiry dialog is rendered")]
    public Task GivenTheManageExternalUserExpiryDialogIsRendered()
        => RenderExternalUserExpiryDialogAsync("manage");

    [Given("the extend external user expiry dialog is rendered")]
    public Task GivenTheExtendExternalUserExpiryDialogIsRendered()
        => RenderExternalUserExpiryDialogAsync("extend");

    private async Task RenderExternalUserExpiryDialogAsync(string dialog)
    {
        ConfigureCommonServices();
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _externalDbContextFactory = new SpecFlowDbContextFactory(options);
        Services.AddSingleton(_externalDbContextFactory);
        Services.AddSingleton(Substitute.For<IMSGraphService>());
        Services.AddSingleton(Substitute.For<IUserEnrollmentService>());
        Services.AddSingleton(Substitute.For<IUserInformationService>());
        Services.AddSingleton(Substitute.For<ILogger<ManageExternalUserDialog>>());
        Services.AddSingleton(Substitute.For<ISnackbar>());

        var role = new Project_Role
        {
            Id = (int)Project_Role.RoleNames.WebApp,
            Name = "Web Application Access",
            Description = "Web Application Access",
            IsExternalRole = true
        };
        var project = new Datahub_Project
        {
            Project_Acronym_CD = "TEST",
            Project_Name = "Test workspace",
            Project_Name_Fr = "Espace de travail de test"
        };
        var portalUser = new PortalUser
        {
            Email = "dialog.external@example.com",
            DisplayName = "Dialog External"
        };
        _dialogExternalUser = new ExternalUser
        {
            FirstName = "Dialog",
            LastName = "External",
            Organization = "SSC",
            UserExpiryDate = DateTimeOffset.UtcNow.AddDays(-1),
            PortalUser = portalUser
        };
        portalUser.ExternalUser = _dialogExternalUser;
        var userRoleLink = new UserRoleLinks
        {
            PortalUser = portalUser,
            Project = project,
            Role = role,
            RoleId = role.Id
        };

        await using (var context = await _externalDbContextFactory.CreateDbContextAsync())
        {
            context.UserRolesLinks.Add(userRoleLink);
            await context.SaveChangesAsync();
        }

        _dialogInstance = Substitute.For<IMudDialogInstance>();
        if (dialog == "manage")
        {
            _manageExternalUserDialog = Render<ManageExternalUserDialog>(parameters => parameters
                .AddCascadingValue(_dialogInstance)
                .Add(component => component.SelectedExternalUser, userRoleLink)
                .Add(component => component.CurrentUser, new PortalUser { Email = "lead@example.gc.ca" }));
            return;
        }

        _extendExternalUserDialog = Render<ExtendExternalUserDialog>(parameters => parameters
            .AddCascadingValue(_dialogInstance)
            .Add(component => component.SelectedUser, _dialogExternalUser));
    }

    [When("the account expiry is changed to (.*) and saved")]
    public async Task WhenTheAccountExpiryIsChangedAndSaved(string expiry)
    {
        _selectedExpiryDate = expiry switch
        {
            "blank" => default,
            "past" => DateTime.Today.AddDays(-1),
            "today" => DateTime.Today,
            "tomorrow" => DateTime.Today.AddDays(1),
            _ => throw new InvalidOperationException($"Unknown expiry value '{expiry}'.")
        };
        DateTime? dateValue = expiry == "blank" ? null : _selectedExpiryDate;

        if (_manageExternalUserDialog is not null)
        {
            SetPrivateField(_manageExternalUserDialog.Instance, "accountExpiry", dateValue);
            await InvokePrivateTaskAsync(_manageExternalUserDialog.Instance, "SaveUserChanges");
            return;
        }

        ArgumentNullException.ThrowIfNull(_extendExternalUserDialog);
        SetPrivateField(_extendExternalUserDialog.Instance, "_newExpiryDate", dateValue);
        await InvokePrivateTaskAsync(_extendExternalUserDialog.Instance, "SaveUserChanges");
    }

    [Then("the external user expiry change is rejected")]
    public async Task ThenTheExternalUserExpiryChangeIsRejected()
    {
        ArgumentNullException.ThrowIfNull(_dialogInstance);
        ArgumentNullException.ThrowIfNull(_externalDbContextFactory);
        ArgumentNullException.ThrowIfNull(_dialogExternalUser);

        _dialogInstance.DidNotReceive().Close(Arg.Any<DialogResult>());
        await using var context = await _externalDbContextFactory.CreateDbContextAsync();
        var storedUser = await context.ExternalUsers.SingleAsync(user => user.Id == _dialogExternalUser.Id);
        storedUser.UserExpiryDate.Should().BeBefore(DateTimeOffset.UtcNow);
    }

    [Then("the external user's future expiry is persisted by the dialog")]
    public async Task ThenTheExternalUsersFutureExpiryIsPersistedByTheDialog()
    {
        ArgumentNullException.ThrowIfNull(_dialogInstance);
        ArgumentNullException.ThrowIfNull(_externalDbContextFactory);
        ArgumentNullException.ThrowIfNull(_dialogExternalUser);

        _dialogInstance.Received(1).Close(Arg.Any<DialogResult>());
        await using var context = await _externalDbContextFactory.CreateDbContextAsync();
        var storedUser = await context.ExternalUsers.SingleAsync(user => user.Id == _dialogExternalUser.Id);
        storedUser.UserExpiryDate.Should().Be(
            new DateTimeOffset(DateTime.SpecifyKind(_selectedExpiryDate.Date, DateTimeKind.Utc)));
    }

    private async Task SetInputValueAsync(string id, string value)
    {
        ArgumentNullException.ThrowIfNull(_externalForm);
        var input = _externalForm.FindComponents<GcdsInput>().Single(component => component.Instance.Id == id);
        await input.InvokeAsync(() => input.Instance.ValueChanged.InvokeAsync(value));
    }

    private static void SetPrivateField<TComponent>(TComponent component, string fieldName, object? value)
    {
        var field = typeof(TComponent).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Field '{fieldName}' was not found.");
        field.SetValue(component, value);
    }

    private static async Task InvokePrivateTaskAsync<TComponent>(TComponent component, string methodName)
    {
        var method = typeof(TComponent).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Method '{methodName}' was not found.");
        var task = method.Invoke(component, null) as Task
            ?? throw new InvalidOperationException($"Method '{methodName}' did not return a task.");
        await task;
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
