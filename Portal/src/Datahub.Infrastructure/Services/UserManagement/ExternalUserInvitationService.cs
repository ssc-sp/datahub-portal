using System.Security.Cryptography;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Users;
using Datahub.Core.Model.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Datahub.Application.Services.Notification;
using System.Reflection.Metadata;

namespace Datahub.Infrastructure.Services.UserManagement;

public class ExternalUserInvitationService : IExternalUserInvitationService
{
    private const string InvitationCodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int InvitationCodeLength = 12;
    private const int InvitationCodeSegmentLength = 4;
    private const int MaxInvitationCodeAttempts = 10;
    public const int InvitationDurationDays = 1;

    private readonly IDbContextFactory<DatahubProjectDBContext> _contextFactory;
    private readonly IGCNotifyService _gcNotifyService;
    private readonly ILogger<ExternalUserInvitationService> _logger;

    public ExternalUserInvitationService(
        IDbContextFactory<DatahubProjectDBContext> contextFactory,
        IGCNotifyService gcNotifyService,
        ILogger<ExternalUserInvitationService> logger)
    {
        _contextFactory = contextFactory;
        _gcNotifyService = gcNotifyService;
        _logger = logger;
    }

    public async Task<bool> IsInvitationTokenValidAsync(
        Guid invitationToken,
        CancellationToken cancellationToken = default)
    {
        if (invitationToken == Guid.Empty)
        {
            return false;
        }

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;

        return await context.ExternalUserRequests
            .AsNoTracking()
            .AnyAsync(i =>
                i.InvitationToken == invitationToken &&
                i.InvitationTokenAccepted == null &&
                i.InvitationExpiry >= now,
                cancellationToken);
    }

    public async Task<WorkspaceInvitation> CreateInvitationAsync(
        int externalUserId,
        string projectAcronym,
        string invitedEmail,
        string invitationRationale,
        int projectRoleId,
        PortalUser inviter,
        Func<WorkspaceInvitation, (string enURL, string frURL)> GetCodeAcceptancePageUrl,
        DateTimeOffset? invitationExpiry = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectAcronym);
        ArgumentException.ThrowIfNullOrWhiteSpace(invitedEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(invitationRationale);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await CreateInvitationAsync(
            context,
            externalUserId,
            projectAcronym,
            invitedEmail,
            invitationRationale,
            projectRoleId,
            inviter,
            invitationExpiry,
            GetCodeAcceptancePageUrl,
            cancellationToken);
    }

    public async Task<WorkspaceInvitation?> CancelInvitationAsync(
        int requestId,
        CancellationToken cancellationToken = default)
    {
        if (requestId <= 0)
        {
            return null;
        }

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var invitation = await context.ExternalUserRequests
            .Include(i => i.Project)
            .Include(i => i.User)
            .FirstOrDefaultAsync(i => i.RequestID == requestId, cancellationToken);

        if (invitation is null)
        {
            return null;
        }

        invitation.InvitationExpiry = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Cancelled workspace invitation {RequestId} for external user {ExternalUserId} in project {ProjectAcronym}",
            invitation.RequestID,
            invitation.User.Id,
            invitation.Project.Project_Acronym_CD);

        return invitation;
    }

    public async Task<WorkspaceInvitation> ResendInvitationAsync(
        int externalUserId,
        string projectAcronym,
        string invitedEmail,
        int projectRoleId,
        Func<WorkspaceInvitation, (string enURL, string frURL)> GetCodeAcceptancePageUrl,
        PortalUser inviter,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectAcronym);
        ArgumentException.ThrowIfNullOrWhiteSpace(invitedEmail);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;

        var activeInvitations = await context.ExternalUserRequests
            .Where(i => i.User.Id == externalUserId)
            .Where(i => i.Project.Project_Acronym_CD == projectAcronym)
            .Where(i => i.InvitationTokenAccepted == null)
            .Where(i => i.InvitationExpiry >= now)
            .ToListAsync(cancellationToken);

        foreach (var invitation in activeInvitations)
        {
            invitation.InvitationExpiry = now;
        }

        if (activeInvitations.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        return await CreateInvitationAsync(
            context,
            externalUserId,
            projectAcronym,
            invitedEmail,
            "Resent Invitation",
            projectRoleId,
            inviter,
            null,
            GetCodeAcceptancePageUrl,
            cancellationToken);
    }

    public async Task<bool> CompleteInvitationAsync(
        Guid invitationToken,
        string invitationCode,
        string externalUserOid,
        CancellationToken cancellationToken = default)
    {
        if (invitationToken == Guid.Empty || string.IsNullOrWhiteSpace(invitationCode) || string.IsNullOrWhiteSpace(externalUserOid))
        {
            throw new InvalidOperationException("Invitation token, code, and external user OID must be provided.");
        }

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var normalizedCode = NormalizeInvitationCode(invitationCode);

        var invitation = await context.ExternalUserRequests
            .Include(i => i.User)
            .Include(i => i.Project)
            .Include(i => i.Requested_Role)
            .FirstOrDefaultAsync(i => i.InvitationToken == invitationToken, cancellationToken);

        if (invitation is null || invitation.InvitationTokenAccepted is not null || invitation.InvitationExpiry < now)
        {
            return false;
        }

        if (!string.Equals(NormalizeInvitationCode(invitation.InvitationCode), normalizedCode, StringComparison.Ordinal))
        {
            return false;
        }

        var trimmedOid = externalUserOid.Trim();

        var existingUserWithOid = await context.ExternalUsers
            .FirstOrDefaultAsync(u => u.ExternalSubject == trimmedOid && u.Id != invitation.User.Id, cancellationToken);

        ExternalUser activeUser;
        if (existingUserWithOid is not null)
        {
            invitation.User = existingUserWithOid;
            activeUser = existingUserWithOid;
        }
        else
        {
            invitation.User.ExternalSubject = trimmedOid;
            activeUser = invitation.User;
        }

        activeUser.FirstLoginDateTime ??= now;

        invitation.InvitationCodeAccepted = now;
        invitation.InvitationTokenAccepted = now;

        var projectUserLink = await context.UserRolesLinks
            .FirstOrDefaultAsync(
                x => x.Project_ID == invitation.Project.Project_ID && x.PortalUserId == activeUser.PortalUserId,
                cancellationToken);

        if (projectUserLink is null)
        {
            context.UserRolesLinks.Add(new UserRoleLinks
            {
                Project_ID = invitation.Project.Project_ID,
                PortalUserId = activeUser.PortalUserId,
                RoleId = invitation.Requested_Role.Id,
                Approved_DT = now.UtcDateTime
            });
        }
        else
        {
            projectUserLink.RoleId = invitation.Requested_Role.Id;
            projectUserLink.Approved_DT ??= now.UtcDateTime;
        }

        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Completed invitation {RequestId} for external subject {ExternalSubject} in project {ProjectAcronym}",
            invitation.RequestID,
            activeUser.ExternalSubject,
            invitation.Project.Project_Acronym_CD);

        return true;
    }

    private async Task<WorkspaceInvitation> CreateInvitationAsync(
        DatahubProjectDBContext context,
        int externalUserId,
        string projectAcronym,
        string invitedEmail,
        string invitationRationale,
        int projectRoleId,
        PortalUser inviter,
        DateTimeOffset? invitationExpiry,
        Func<WorkspaceInvitation, (string enURL, string frURL)> GetCodeAcceptancePageUrl,
        CancellationToken cancellationToken)
    {
        // attach inviter to context to avoid duplicate key error if inviter is also the user being invited
        context.Attach(inviter);
        var requestedRole = await context.Project_Roles
            .FirstAsync(r => r.Id == projectRoleId, cancellationToken);
        var externalUser = await context.ExternalUsers.Include(p => p.PortalUser)
            .FirstOrDefaultAsync(u => u.Id == externalUserId, cancellationToken);

        if (externalUser is null)
        {
            throw new InvalidOperationException($"External user {externalUserId} was not found.");
        }

        var project = await context.Projects
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym, cancellationToken);

        if (project is null)
        {
            throw new InvalidOperationException($"Project {projectAcronym} was not found.");
        }

        var invitation = new WorkspaceInvitation
        {
            User = externalUser,
            Project = project,
            InvitationToken = Guid.NewGuid(),
            InvitedEmail = invitedEmail.Trim(),
            InvitationExpiry = invitationExpiry ?? DateTimeOffset.UtcNow.AddDays(InvitationDurationDays),
            InvitationCode = await GenerateUniqueInvitationCodeAsync(context, cancellationToken),
            InvitationRationale_EN = invitationRationale.Trim(),
            InvitedBy = inviter,
            ExternalSubjectInvited = string.IsNullOrWhiteSpace(externalUser.ExternalSubject)
                ? null
                : externalUser.ExternalSubject,
            Request_DT = DateTimeOffset.UtcNow,
            Requested_Role = requestedRole
        };

        context.ExternalUserRequests.Add(invitation);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created workspace invitation {RequestId} for external user {ExternalUserId} in project {ProjectAcronym}",
            invitation.RequestID,
            externalUserId,
            projectAcronym);

        var invitationUrl = GetCodeAcceptancePageUrl(invitation);
        await _gcNotifyService.SendExternalUserInviteNotification(
            invitation.InvitedEmail,
            externalUser.PortalUser.DisplayName ?? "<user>",
            project.ProjectName ?? "Workspace",
            inviter.DisplayName ?? "Inviter",
            invitationUrl.enURL,
            invitationUrl.frURL);

        return invitation;
    }

    private async Task<string> GenerateUniqueInvitationCodeAsync(
        DatahubProjectDBContext context,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaxInvitationCodeAttempts; attempt++)
        {
            var invitationCode = GenerateInvitationCode();
            var exists = await context.ExternalUserRequests
                .AnyAsync(i => i.InvitationCode == invitationCode, cancellationToken);

            if (!exists)
            {
                return invitationCode;
            }
        }

        throw new InvalidOperationException("Unable to generate a unique invitation code.");
    }

    private static string GenerateInvitationCode()
    {
        Span<char> chars = stackalloc char[InvitationCodeLength];

        for (var i = 0; i < InvitationCodeLength; i++)
        {
            chars[i] = InvitationCodeAlphabet[RandomNumberGenerator.GetInt32(InvitationCodeAlphabet.Length)];
        }

        return string.Create(
            InvitationCodeLength + ((InvitationCodeLength / InvitationCodeSegmentLength) - 1),
            chars,
            static (buffer, source) =>
            {
                var sourceIndex = 0;
                for (var i = 0; i < buffer.Length; i++)
                {
                    if (i > 0 && i % (InvitationCodeSegmentLength + 1) == InvitationCodeSegmentLength)
                    {
                        buffer[i] = '-';
                        continue;
                    }

                    buffer[i] = source[sourceIndex++];
                }
            });
    }

    private static string NormalizeInvitationCode(string code)
    {
        return code.Trim().Replace("-", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
    }
}
