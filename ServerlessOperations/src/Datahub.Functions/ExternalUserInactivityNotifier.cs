using Datahub.Application.Services.Notification;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Users;
using Datahub.Functions.Providers;
using Datahub.Functions.Validators;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions;

public class ExternalUserInactivityNotifier(
    ILoggerFactory loggerFactory,
    IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
    IDateProvider dateProvider,
    EmailValidator emailValidator,
    IGCNotifyService gcNotifyService)
{
    private const int NotifyAt30Days = 30;
    private const int NotifyAt60Days = 60;
    private const int DisableAt90Days = 90;

    private readonly ILogger<ExternalUserInactivityNotifier> _logger = loggerFactory.CreateLogger<ExternalUserInactivityNotifier>();

    [Function("ExternalUserInactivityNotifier")]
    public async Task Run([TimerTrigger("%InactivityCRON%")] TimerInfo timerInfo, CancellationToken ct)
    {
        _logger.LogInformation("Running external user inactivity notifier.");

        await using var ctx = await dbContextFactory.CreateDbContextAsync(ct);

        var externalUsers = await ctx.ExternalUsers
            .Include(e => e.PortalUser)
            .AsTracking()
            .Where(e => e.UserDeactivatedAt == null)
            .ToListAsync(ct);

        foreach (var externalUser in externalUsers)
        {
            var lastActivity = GetLastActivityDate(externalUser);
            if (lastActivity is null)
            {
                continue;
            }

            var daysInactive = (dateProvider.Today - lastActivity.Value.UtcDateTime.Date).Days;
            if (daysInactive < NotifyAt30Days)
            {
                continue;
            }

            var activeRoleLinks = await ctx.UserRolesLinks
                .AsTracking()
                .Include(l => l.Project)
                .Where(l => l.PortalUserId == externalUser.PortalUserId)
                .Where(l => l.RoleId == (int)Project_Role.RoleNames.WebApp
                    || l.RoleId == (int)Project_Role.RoleNames.Storage
                    || l.RoleId == (int)Project_Role.RoleNames.WebAppAndStorage)
                .ToListAsync(ct);

            if (!activeRoleLinks.Any())
            {
                continue;
            }

            var projectIds = activeRoleLinks
                .Select(l => l.Project_ID)
                .Distinct()
                .ToList();

            var workspaceLeads = await GetWorkspaceLeadsAsync(ctx, projectIds, ct);
            if (!workspaceLeads.Any())
            {
                continue;
            }

            var externalUserName = externalUser.PortalUser?.DisplayName 
                ?? $"{externalUser.FirstName} {externalUser.LastName}".Trim()
                ?? "Unknown User";
            var lastLoginStr = lastActivity.Value.UtcDateTime.ToString("yyyy-MM-dd");

            if (daysInactive == NotifyAt30Days || daysInactive == NotifyAt60Days)
            {
                await NotifyWorkspaceLeadsWarningAsync(workspaceLeads, externalUserName, daysInactive, lastLoginStr, activeRoleLinks);
                _logger.LogInformation(
                    "Sent external user inactivity warning for user {PortalUserId} at {DaysInactive} days.",
                    externalUser.PortalUserId,
                    daysInactive);
                continue;
            }

            if (daysInactive < DisableAt90Days)
            {
                continue;
            }

            externalUser.UserDeactivatedAt = DateTimeOffset.UtcNow;
            externalUser.DeactivationReason = $"Automatically deactivated after {daysInactive} days of inactivity.";

            foreach (var roleLink in activeRoleLinks)
            {
                roleLink.RoleId = (int)Project_Role.RoleNames.DisabledUser;
            }

            await ctx.SaveChangesAsync(ct);
            await NotifyWorkspaceLeadsDeactivationAsync(workspaceLeads, externalUserName, daysInactive, activeRoleLinks);

            _logger.LogInformation(
                "Deactivated external user {PortalUserId} after {DaysInactive} days and notified workspace leads.",
                externalUser.PortalUserId,
                daysInactive);
        }
    }

    private static DateTimeOffset? GetLastActivityDate(ExternalUser externalUser)
    {
        return externalUser.LastLoginDateTime
               ?? externalUser.FirstLoginDateTime
               ?? externalUser.PortalUser.LastLoginDateTime
               ?? externalUser.PortalUser.FirstLoginDateTime
               ?? externalUser.CreatedAt;
    }

    private async Task<List<(string Email, string Name)>> GetWorkspaceLeadsAsync(
        DatahubProjectDBContext ctx,
        List<int> projectIds,
        CancellationToken ct)
    {
        var leads = await ctx.UserRolesLinks
            .AsNoTracking()
            .Include(l => l.PortalUser)
            .Where(l => projectIds.Contains(l.Project_ID))
            .Where(l => l.RoleId == (int)Project_Role.RoleNames.WorkspaceLead)
            .Select(l => new { l.PortalUser.Email, l.PortalUser.DisplayName })
            .Where(x => emailValidator.IsValidEmail(x.Email))
            .Distinct()
            .ToListAsync(ct);

        return leads
            .Select(x => (x.Email, x.DisplayName ?? "Workspace Lead"))
            .ToList();
    }

    private async Task NotifyWorkspaceLeadsWarningAsync(
        List<(string Email, string Name)> workspaceLeads,
        string externalUserName,
        int daysInactive,
        string lastLoginDate,
        List<UserRoleLinks> roleLinks)
    {
        // Get unique workspace acronyms from role links
        var workspaceAcronyms = roleLinks
            .Select(r => r.Project?.Project_Acronym_CD)
            .Where(a => a != null)
            .Distinct()
            .ToList();

        var workspaceAcronym = workspaceAcronyms.FirstOrDefault() ?? "DataHub Workspace";

        foreach (var (email, name) in workspaceLeads)
        {
            await gcNotifyService.SendExternalUserInactivityWarningNotification(
                email,
                name,
                externalUserName,
                workspaceAcronym,
                daysInactive,
                lastLoginDate);
        }
    }

    private async Task NotifyWorkspaceLeadsDeactivationAsync(
        List<(string Email, string Name)> workspaceLeads,
        string externalUserName,
        int daysInactive,
        List<UserRoleLinks> roleLinks)
    {
        // Get unique workspace acronyms from role links
        var workspaceAcronyms = roleLinks
            .Select(r => r.Project?.Project_Acronym_CD)
            .Where(a => a != null)
            .Distinct()
            .ToList();

        var workspaceAcronym = workspaceAcronyms.FirstOrDefault() ?? "DataHub Workspace";

        foreach (var (email, name) in workspaceLeads)
        {
            await gcNotifyService.SendExternalUserDeactivationNotification(
                email,
                name,
                externalUserName,
                workspaceAcronym,
                daysInactive,
                DateTimeOffset.UtcNow.UtcDateTime);
        }
    }

    private async Task<List<string>> GetWorkspaceLeadEmailsAsync(
        DatahubProjectDBContext ctx,
        List<int> projectIds,
        CancellationToken ct)
    {
        return await ctx.UserRolesLinks
            .AsNoTracking()
            .Include(l => l.PortalUser)
            .Where(l => projectIds.Contains(l.Project_ID))
            .Where(l => l.RoleId == (int)Project_Role.RoleNames.WorkspaceLead)
            .Select(l => l.PortalUser.Email)
            .Where(email => emailValidator.IsValidEmail(email))
            .Distinct()
            .ToListAsync(ct);
    }

    private async Task NotifyWorkspaceLeadsAsync(IEnumerable<string> emails, int daysInactive)
    {
        foreach (var email in emails)
        {
            await gcNotifyService.SendWorkspaceInactiveNotification(email, daysInactive.ToString());
        }
    }
}
