using System.Security.Cryptography;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services.UserManagement;

public class ExternalUserInvitationService : IExternalUserInvitationService
{
    private const string InvitationCodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int InvitationCodeLength = 12;
    private const int InvitationCodeSegmentLength = 4;
    private const int MaxInvitationCodeAttempts = 10;

    private readonly IDbContextFactory<DatahubProjectDBContext> _contextFactory;
    private readonly ILogger<ExternalUserInvitationService> _logger;

    public ExternalUserInvitationService(
        IDbContextFactory<DatahubProjectDBContext> contextFactory,
        ILogger<ExternalUserInvitationService> logger)
    {
        _contextFactory = contextFactory;
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
        DateTimeOffset? invitationExpiry = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectAcronym);
        ArgumentException.ThrowIfNullOrWhiteSpace(invitedEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(invitationRationale);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var externalUser = await context.ExternalUsers
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
            InvitationExpiry = invitationExpiry ?? DateTimeOffset.UtcNow.AddDays(14),
            InvitationCode = await GenerateUniqueInvitationCodeAsync(context, cancellationToken),
            InvitationRationale_EN = invitationRationale.Trim(),
            ExternalSubjectInvited = string.IsNullOrWhiteSpace(externalUser.ExternalSubject)
                ? null
                : externalUser.ExternalSubject,
            Request_DT = DateTimeOffset.UtcNow
        };

        context.ExternalUserRequests.Add(invitation);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created workspace invitation {RequestId} for external user {ExternalUserId} in project {ProjectAcronym}",
            invitation.RequestID,
            externalUserId,
            projectAcronym);

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
}
