using Bunit;
using Bunit.TestDoubles;
using Datahub.Application.Services;
using Datahub.Application.Services.Notification;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Data;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Users;
using Datahub.Infrastructure.Services.UserManagement;
using Datahub.Portal.Pages.Workspace.Users;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.FeatureManagement;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps.Workspace;

[Binding]
public sealed class WorkspaceExternalUsersPageSteps : BunitTestSteps, IDisposable
{
    private const string WorkspaceAcronym = "TEST";

    private SpecFlowDbContextFactory _factory = null!;
    private IDialogService _dialogService = null!;
    private IRenderedComponent<WorkspaceExternalUsersPage> _page = null!;
    private int _invitationId;
    private int _externalUserId;

    [Given("an expired invitation is shown on the external workspace users page")]
    public Task GivenAnExpiredInvitationIsShown() =>
        RenderPageAsync(DateTimeOffset.UtcNow.AddDays(-1));

    [Given("an active invitation is shown on the external workspace users page")]
    public Task GivenAnActiveInvitationIsShown() =>
        RenderPageAsync(DateTimeOffset.UtcNow.AddDays(1));

    [Given("an expired invitation is shown and the deletion service will refuse it")]
    public async Task GivenAnExpiredInvitationIsShownAndDeletionWillBeRefused()
    {
        var invitationService = Substitute.For<IExternalUserInvitationService>();
        invitationService.CanInviteExternalUsers(WorkspaceAcronym).Returns(true);
        invitationService.DeleteExpiredInvitationAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(false);
        await RenderPageAsync(DateTimeOffset.UtcNow.AddDays(-1), invitationService);
    }

    [When("the workspace lead chooses to delete the invitation")]
    public void WhenTheWorkspaceLeadChoosesToDeleteTheInvitation()
    {
        FindPageAction("Delete invitation").Should().NotBeNull();
    }

    [When("the workspace lead cancels invitation deletion")]
    public void WhenTheWorkspaceLeadCancelsInvitationDeletion()
    {
        ConfigureDialogResult(false);
        FindPageAction("Delete invitation").Click();
    }

    [When("the workspace lead confirms invitation deletion")]
    public void WhenTheWorkspaceLeadConfirmsInvitationDeletion()
    {
        ConfigureDialogResult(true);
        FindPageAction("Delete invitation").Click();
    }

    [Then("the delete invitation action should be shown")]
    public void ThenTheDeleteInvitationActionShouldBeShown() =>
        _page.Markup.Should().Contain("Delete invitation");

    [Then("the delete invitation action should not be shown")]
    public void ThenTheDeleteInvitationActionShouldNotBeShown() =>
        _page.Markup.Should().NotContain("Delete invitation");

    [Then("the resend invitation action should be shown")]
    public void ThenTheResendInvitationActionShouldBeShown() =>
        _page.Markup.Should().Contain("Resend invitation");

    [Then("the invitation should remain on the external workspace users page")]
    public async Task ThenTheInvitationShouldRemain()
    {
        _page.Markup.Should().Contain("external@example.com");
        await using var context = await _factory.CreateDbContextAsync();
        (await context.ExternalUserRequests.AnyAsync(i => i.RequestID == _invitationId)).Should().BeTrue();
    }

    [Then("the invitation should be removed from the external workspace users page")]
    public async Task ThenTheInvitationShouldBeRemoved()
    {
        _page.WaitForAssertion(() => _page.Markup.Should().NotContain("external@example.com"));
        await using var context = await _factory.CreateDbContextAsync();
        (await context.ExternalUserRequests.AnyAsync(i => i.RequestID == _invitationId)).Should().BeFalse();
    }

    [Then("the external user profile should remain in the database")]
    public async Task ThenTheExternalUserProfileShouldRemain()
    {
        await using var context = await _factory.CreateDbContextAsync();
        (await context.ExternalUsers.AnyAsync(u => u.Id == _externalUserId)).Should().BeTrue();
    }

    [Then("invitation deletion success feedback should be shown")]
    public void ThenInvitationDeletionSuccessFeedbackShouldBeShown() =>
        GetSnackbar().Received().Add("Invitation deleted", Severity.Success);

    [Then("invitation deletion failure feedback should be shown")]
    public void ThenInvitationDeletionFailureFeedbackShouldBeShown() =>
        _page.WaitForAssertion(() =>
            GetSnackbar().Received().Add("Unable to delete invitation", Severity.Error));

    private async Task RenderPageAsync(
        DateTimeOffset invitationExpiry,
        IExternalUserInvitationService? invitationServiceOverride = null)
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        var mudBlazorAssembly = typeof(MudTable<>).Assembly;
        var internalMudLocalizerType = mudBlazorAssembly.GetType("MudBlazor.InternalMudLocalizer")!;
        var localizationInterceptor = Substitute.For<ILocalizationInterceptor>();
        localizationInterceptor.Handle(Arg.Any<string>(), Arg.Any<object[]>()).Returns(call =>
        {
            var key = call.ArgAt<string>(0);
            return new LocalizedString(key, key);
        });
        var internalMudLocalizer = Activator.CreateInstance(
            internalMudLocalizerType,
            localizationInterceptor)!;
        Services.AddSingleton(internalMudLocalizerType, internalMudLocalizer);
        Services.AddSingleton(Substitute.For<ISnackbar>());
        _dialogService = Substitute.For<IDialogService>();
        Services.AddSingleton(_dialogService);

        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _factory = new SpecFlowDbContextFactory(options);
        (_invitationId, _externalUserId) = await SeedInvitationAsync(invitationExpiry);

        Services.AddSingleton<IDbContextFactory<DatahubProjectDBContext>>(_factory);
        Services.AddSingleton(Substitute.For<IDatahubAuditingService>());
        Services.AddSingleton(Substitute.For<IUserInformationService>());
        Services.AddSingleton(Substitute.For<IProjectUserManagementService>());

        var cultureService = Substitute.For<ICultureService>();
        cultureService.Culture.Returns(ICultureService.English);
        Services.AddSingleton(cultureService);

        var featureManager = Substitute.For<IFeatureManagerSnapshot>();
        featureManager.IsEnabledAsync(Arg.Any<string>()).Returns(true);
        Services.AddSingleton(featureManager);

        var localizer = Substitute.For<IStringLocalizer>();
        localizer[Arg.Any<string>()].Returns(call =>
        {
            var key = call.Arg<string>();
            return new LocalizedString(key, key);
        });
        Services.AddSingleton(localizer);

        var invitationService = invitationServiceOverride ?? new ExternalUserInvitationService(
            _factory,
            Substitute.For<IGCNotifyService>(),
            NullLogger<ExternalUserInvitationService>.Instance);
        Services.AddSingleton(invitationService);

        var auth = this.AddAuthorization();
        auth.SetAuthorized("Workspace Lead");
        auth.SetRoles($"{WorkspaceAcronym}{RoleConstants.WORKSPACE_LEAD_SUFFIX}");

        _page = Render<WorkspaceExternalUsersPage>(parameters =>
            parameters.Add(p => p.WorkspaceAcronym, WorkspaceAcronym));
    }

    private async Task<(int InvitationId, int ExternalUserId)> SeedInvitationAsync(DateTimeOffset invitationExpiry)
    {
        await using var context = await _factory.CreateDbContextAsync();
        var project = new Datahub_Project
        {
            Project_Acronym_CD = WorkspaceAcronym,
            Project_Name = "Test workspace",
            Project_Name_Fr = "Espace de travail test"
        };
        var role = new Project_Role
        {
            Id = (int)Project_Role.RoleNames.Guest,
            Name = "Guest",
            Description = "Guest"
        };
        var invitedPortalUser = new PortalUser
        {
            Email = "external@example.com",
            DisplayName = "External User",
            EntraUser = new EntraUser { GraphGuid = Guid.NewGuid().ToString(), PortalUser = null! }
        };
        var externalUser = new ExternalUser
        {
            FirstName = "External",
            LastName = "User",
            Organization = "Test Org",
            UserExpiryDate = DateTimeOffset.UtcNow.AddYears(1),
            PortalUser = invitedPortalUser
        };
        var inviter = new PortalUser
        {
            Email = "lead@example.com",
            DisplayName = "Workspace Lead",
            EntraUser = new EntraUser { GraphGuid = Guid.NewGuid().ToString(), PortalUser = null! }
        };
        var invitation = new WorkspaceInvitation
        {
            User = externalUser,
            InvitedBy = inviter,
            Project = project,
            InvitationToken = Guid.NewGuid(),
            InvitedEmail = invitedPortalUser.Email,
            InvitationExpiry = invitationExpiry,
            InvitationCode = "AAAA-BBBB-CCCC",
            InvitationRationale_EN = "Test",
            Request_DT = DateTimeOffset.UtcNow.AddDays(-2),
            Requested_Role = role
        };

        context.ExternalUserRequests.Add(invitation);
        await context.SaveChangesAsync();
        return (invitation.RequestID, externalUser.Id);
    }

    private AngleSharp.Dom.IElement FindPageAction(string text) =>
        _page.FindAll("a, button").Single(element => element.TextContent.Contains(text, StringComparison.Ordinal));

    private ISnackbar GetSnackbar() => Services.GetRequiredService<ISnackbar>();

    private void ConfigureDialogResult(bool confirmed)
    {
        _dialogService.ShowMessageBoxAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<DialogOptions>())
            .Returns(Task.FromResult<bool?>(confirmed));
    }

    public new void Dispose()
    {
        // BunitTestSteps disposes the context asynchronously in its AfterScenario hook.
    }
}
