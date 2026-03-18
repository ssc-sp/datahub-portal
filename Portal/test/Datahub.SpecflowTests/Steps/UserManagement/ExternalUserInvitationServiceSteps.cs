using Datahub.Application.Services.Notification;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Users;
using Datahub.Infrastructure.Services.UserManagement;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps.UserManagement;

[Binding]
public class ExternalUserInvitationServiceSteps(
    ScenarioContext scenarioContext,
    IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
    IGCNotifyService gcNotifyService,
    ExternalUserInvitationService service)
{
    private const string InvitationKey = "invitation";
    private const string InviterKey = "inviter";
    private const string TokenValidKey = "tokenValid";
    private const string CompletionResultKey = "completionResult";
    private const string ThrownExceptionKey = "thrownException";
    private const string CancelResultKey = "cancelResult";
    private const string ExternalSubjectKey = "externalSubject";

    private static readonly string TestProjectAcronym = "TEST";
    private static readonly int TestExternalUserId = 1;

    private static Project_Role GuestRole => new()
    {
        Id = (int)Project_Role.RoleNames.Guest,
        Name = "Guest",
        Description = "Guest"
    };

    #region Background

    [Given(@"a project ""(.*)"" exists in the database")]
    public async Task GivenAProjectExistsInTheDatabase(string acronym)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        ctx.Projects.Add(new Datahub_Project
        {
            Project_Acronym_CD = acronym,
            Project_Name = acronym,
            Project_Name_Fr = acronym
        });
        ctx.Project_Roles.Add(GuestRole);
        await ctx.SaveChangesAsync();
    }

    [Given(@"an external user with id (\d+) exists in the database")]
    public async Task GivenAnExternalUserWithIdExistsInTheDatabase(int id)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var portalUser = new PortalUser
        {
            Id = id,
            Email = $"external{id}@example.com",
            DisplayName = $"External User {id}",
            EntraUser = new EntraUser { GraphGuid = Guid.NewGuid().ToString(), PortalUser = null! }
        };
        var externalUser = new ExternalUser
        {
            Id = id,
            FirstName = "External",
            LastName = $"User{id}",
            Organization = "Test Org",
            UserExpiryDate = DateTimeOffset.UtcNow.AddYears(1),
            PortalUser = portalUser,
            PortalUserId = id
        };
        ctx.PortalUsers.Add(portalUser);
        ctx.ExternalUsers.Add(externalUser);
        await ctx.SaveChangesAsync();
    }

    [Given(@"a portal user ""(.*)"" exists as the inviter")]
    public async Task GivenAPortalUserExistsAsTheInviter(string email)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var inviter = new PortalUser
        {
            Id = 100,
            Email = email,
            DisplayName = "Test Inviter",
            EntraUser = new EntraUser { GraphGuid = Guid.NewGuid().ToString(), PortalUser = null! }
        };
        ctx.PortalUsers.Add(inviter);
        await ctx.SaveChangesAsync();
        scenarioContext[InviterKey] = inviter;
    }

    #endregion

    #region Given – invitation state

    [Given(@"a valid workspace invitation exists for the external user")]
    public async Task GivenAValidWorkspaceInvitationExistsForTheExternalUser()
    {
        var invitation = await CreateStoredInvitationAsync(
            invitationExpiry: DateTimeOffset.UtcNow.AddDays(7),
            tokenAccepted: null);
        scenarioContext[InvitationKey] = invitation;
    }

    [Given(@"an expired workspace invitation exists for the external user")]
    public async Task GivenAnExpiredWorkspaceInvitationExistsForTheExternalUser()
    {
        var invitation = await CreateStoredInvitationAsync(
            invitationExpiry: DateTimeOffset.UtcNow.AddDays(-1),
            tokenAccepted: null);
        scenarioContext[InvitationKey] = invitation;
    }

    [Given(@"a workspace invitation that was already accepted exists for the external user")]
    public async Task GivenAWorkspaceInvitationThatWasAlreadyAcceptedExistsForTheExternalUser()
    {
        var invitation = await CreateStoredInvitationAsync(
            invitationExpiry: DateTimeOffset.UtcNow.AddDays(7),
            tokenAccepted: DateTimeOffset.UtcNow.AddMinutes(-5));
        scenarioContext[InvitationKey] = invitation;
    }

    #endregion

    #region When – IsInvitationTokenValidAsync

    [When(@"the invitation token validity is checked")]
    public async Task WhenTheInvitationTokenValidityIsChecked()
    {
        var invitation = (WorkspaceInvitation)scenarioContext[InvitationKey];
        var result = await service.IsInvitationTokenValidAsync(invitation.InvitationToken);
        scenarioContext[TokenValidKey] = result;
    }

    [When(@"the invitation token validity is checked with an empty Guid")]
    public async Task WhenTheInvitationTokenValidityIsCheckedWithAnEmptyGuid()
    {
        var result = await service.IsInvitationTokenValidAsync(Guid.Empty);
        scenarioContext[TokenValidKey] = result;
    }

    #endregion

    #region When – CreateInvitationAsync

    [When(@"an invitation is created for the external user in project ""(.*)""")]
    public async Task WhenAnInvitationIsCreatedForTheExternalUserInProject(string projectAcronym)
    {
        var inviter = (PortalUser)scenarioContext[InviterKey];
        try
        {
            var invitation = await service.CreateInvitationAsync(
                externalUserId: TestExternalUserId,
                projectAcronym: projectAcronym,
                invitedEmail: "external1@example.com",
                invitationRationale: "Test invitation",
                projectRoleId: (int)Project_Role.RoleNames.Guest,
                inviter: inviter,
                GetCodeAcceptancePageUrl: _ => "https://test.example.com/accept",
                invitationExpiry: DateTimeOffset.UtcNow.AddDays(7));
            scenarioContext[InvitationKey] = invitation;
        }
        catch (Exception ex)
        {
            scenarioContext[ThrownExceptionKey] = ex;
        }
    }

    [When(@"an invitation is created for a non-existent external user in project ""(.*)""")]
    public async Task WhenAnInvitationIsCreatedForANonExistentExternalUserInProject(string projectAcronym)
    {
        var inviter = (PortalUser)scenarioContext[InviterKey];
        try
        {
            await service.CreateInvitationAsync(
                externalUserId: 9999,
                projectAcronym: projectAcronym,
                invitedEmail: "nobody@example.com",
                invitationRationale: "Test",
                projectRoleId: (int)Project_Role.RoleNames.Guest,
                inviter: inviter,
                GetCodeAcceptancePageUrl: _ => "https://test.example.com/accept");
        }
        catch (Exception ex)
        {
            scenarioContext[ThrownExceptionKey] = ex;
        }
    }

    #endregion

    #region When – CancelInvitationAsync

    [When(@"the invitation is cancelled")]
    public async Task WhenTheInvitationIsCancelled()
    {
        var invitation = (WorkspaceInvitation)scenarioContext[InvitationKey];
        var result = await service.CancelInvitationAsync(invitation.RequestID);
        scenarioContext[CancelResultKey] = result;
    }

    [When(@"a non-existent invitation is cancelled")]
    public async Task WhenANonExistentInvitationIsCancelled()
    {
        var result = await service.CancelInvitationAsync(99999);
        scenarioContext[CancelResultKey] = result;
    }

    #endregion

    #region When – ResendInvitationAsync

    [When(@"the invitation is resent to the external user in project ""(.*)""")]
    public async Task WhenTheInvitationIsResentToTheExternalUserInProject(string projectAcronym)
    {
        var inviter = (PortalUser)scenarioContext[InviterKey];
        var newInvitation = await service.ResendInvitationAsync(
            externalUserId: TestExternalUserId,
            projectAcronym: projectAcronym,
            invitedEmail: "external1@example.com",
            projectRoleId: (int)Project_Role.RoleNames.Guest,
            GetCodeAcceptancePageUrl: _ => "https://test.example.com/accept",
            inviter: inviter);
        scenarioContext["newInvitation"] = newInvitation;
    }

    #endregion

    #region When – CompleteInvitationAsync

    [When(@"the invitation is completed with the correct code and a new external subject")]
    public async Task WhenTheInvitationIsCompletedWithTheCorrectCodeAndANewExternalSubject()
    {
        var invitation = (WorkspaceInvitation)scenarioContext[InvitationKey];
        var externalSubject = Guid.NewGuid().ToString();
        scenarioContext[ExternalSubjectKey] = externalSubject;
        var result = await service.CompleteInvitationAsync(
            invitation.InvitationToken,
            invitation.InvitationCode,
            externalSubject);
        scenarioContext[CompletionResultKey] = result;
    }

    [When(@"the invitation is completed with an incorrect code")]
    public async Task WhenTheInvitationIsCompletedWithAnIncorrectCode()
    {
        var invitation = (WorkspaceInvitation)scenarioContext[InvitationKey];
        var result = await service.CompleteInvitationAsync(
            invitation.InvitationToken,
            "XXXX-XXXX-XXXX",
            Guid.NewGuid().ToString());
        scenarioContext[CompletionResultKey] = result;
    }

    #endregion

    #region Then – token validity

    [Then(@"the token should be valid")]
    public void ThenTheTokenShouldBeValid()
    {
        ((bool)scenarioContext[TokenValidKey]).Should().BeTrue();
    }

    [Then(@"the token should not be valid")]
    public void ThenTheTokenShouldNotBeValid()
    {
        ((bool)scenarioContext[TokenValidKey]).Should().BeFalse();
    }

    #endregion

    #region Then – create invitation

    [Then(@"the invitation should be stored in the database")]
    public async Task ThenTheInvitationShouldBeStoredInTheDatabase()
    {
        var invitation = (WorkspaceInvitation)scenarioContext[InvitationKey];
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var stored = await ctx.ExternalUserRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.RequestID == invitation.RequestID);
        stored.Should().NotBeNull();
    }

    [Then(@"the invitation should have a future expiry date")]
    public void ThenTheInvitationShouldHaveAFutureExpiryDate()
    {
        var invitation = (WorkspaceInvitation)scenarioContext[InvitationKey];
        invitation.InvitationExpiry.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [Then(@"a notification email should be sent")]
    public async Task ThenANotificationEmailShouldBeSent()
    {
        await gcNotifyService.Received(1).SendExternalUserInviteNotification(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>());
    }

    [Then(@"an invalid operation exception should be thrown")]
    public void ThenAnInvalidOperationExceptionShouldBeThrown()
    {
        scenarioContext[ThrownExceptionKey].Should().BeOfType<InvalidOperationException>();
    }

    #endregion

    #region Then – cancel invitation

    [Then(@"the invitation expiry should be set to approximately now")]
    public async Task ThenTheInvitationExpiryShouldBeSetToApproximatelyNow()
    {
        var invitation = (WorkspaceInvitation)scenarioContext[InvitationKey];
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var stored = await ctx.ExternalUserRequests
            .AsNoTracking()
            .FirstAsync(i => i.RequestID == invitation.RequestID);
        stored.InvitationExpiry.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Then(@"the result should be null")]
    public void ThenTheResultShouldBeNull()
    {
        scenarioContext[CancelResultKey].Should().BeNull();
    }

    #endregion

    #region Then – resend invitation

    [Then(@"the original invitation should be expired")]
    public async Task ThenTheOriginalInvitationShouldBeExpired()
    {
        var original = (WorkspaceInvitation)scenarioContext[InvitationKey];
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var stored = await ctx.ExternalUserRequests
            .AsNoTracking()
            .FirstAsync(i => i.RequestID == original.RequestID);
        stored.InvitationExpiry.Should().BeBefore(DateTimeOffset.UtcNow);
    }

    [Then(@"a new invitation should be created in the database")]
    public async Task ThenANewInvitationShouldBeCreatedInTheDatabase()
    {
        var newInvitation = (WorkspaceInvitation)scenarioContext["newInvitation"];
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var stored = await ctx.ExternalUserRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.RequestID == newInvitation.RequestID);
        stored.Should().NotBeNull();
        stored!.InvitationExpiry.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    #endregion

    #region Then – complete invitation

    [Then(@"the invitation should be marked as accepted")]
    public async Task ThenTheInvitationShouldBeMarkedAsAccepted()
    {
        var invitation = (WorkspaceInvitation)scenarioContext[InvitationKey];
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var stored = await ctx.ExternalUserRequests
            .AsNoTracking()
            .FirstAsync(i => i.RequestID == invitation.RequestID);
        stored.InvitationTokenAccepted.Should().NotBeNull();
        stored.InvitationCodeAccepted.Should().NotBeNull();
    }

    [Then(@"the external user should have the requested role in the project")]
    public async Task ThenTheExternalUserShouldHaveTheRequestedRoleInTheProject()
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var externalUser = await ctx.ExternalUsers
            .AsNoTracking()
            .FirstAsync(u => u.Id == TestExternalUserId);
        var roleLink = await ctx.UserRolesLinks
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.PortalUserId == externalUser.PortalUserId);
        roleLink.Should().NotBeNull();
        roleLink!.RoleId.Should().Be((int)Project_Role.RoleNames.Guest);
    }

    [Then(@"the completion should return false")]
    public void ThenTheCompletionShouldReturnFalse()
    {
        ((bool)scenarioContext[CompletionResultKey]).Should().BeFalse();
    }

    #endregion

    #region Helpers

    private async Task<WorkspaceInvitation> CreateStoredInvitationAsync(
        DateTimeOffset invitationExpiry,
        DateTimeOffset? tokenAccepted)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();

        var externalUser = await ctx.ExternalUsers
            .Include(u => u.PortalUser)
            .FirstAsync(u => u.Id == TestExternalUserId);

        var project = await ctx.Projects
            .FirstAsync(p => p.Project_Acronym_CD == TestProjectAcronym);

        var role = await ctx.Project_Roles
            .FirstAsync(r => r.Id == (int)Project_Role.RoleNames.Guest);

        var inviter = (PortalUser)scenarioContext[InviterKey];
        ctx.Attach(inviter);

        var code = GenerateDummyCode();
        var invitation = new WorkspaceInvitation
        {
            User = externalUser,
            Project = project,
            InvitedBy = inviter,
            InvitationToken = Guid.NewGuid(),
            InvitedEmail = "external1@example.com",
            InvitationExpiry = invitationExpiry,
            InvitationTokenAccepted = tokenAccepted,
            InvitationCode = code,
            InvitationRationale_EN = "Test",
            Request_DT = DateTimeOffset.UtcNow,
            Requested_Role = role
        };

        ctx.ExternalUserRequests.Add(invitation);
        await ctx.SaveChangesAsync();
        return invitation;
    }

    private static string GenerateDummyCode() => "AAAA-BBBB-CCCC";

    #endregion
}
