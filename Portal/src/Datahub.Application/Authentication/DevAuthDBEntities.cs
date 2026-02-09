using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Datahub.Application.Authentication;

public class DevAuthDBEntities
{
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
    private readonly DevAuthOptions _options;
    private readonly ILogger<DevAuthDBEntities> _logger;

    public DevAuthDBEntities(
  IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
   IOptions<DevAuthOptions> options,
   ILogger<DevAuthDBEntities> logger)
    {
        _dbContextFactory = dbContextFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task EnsureDevUserAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.UserEmail))
        {
            _logger.LogInformation("DevAuth: UserEmail not configured, skipping dev user bootstrap.");
            return;
        }

        await using var ctx = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var emailNorm = _options.UserEmail.Trim().ToLowerInvariant();
        var displayName = string.IsNullOrWhiteSpace(_options.UserName) ? _options.UserEmail : _options.UserName;

        // Ensure ExternalUser for this dev account using fixed DevUserObjectId
        var devOid = DevelopmentAuthStateProvider.DevUserObjectId;

        var externalUser = await ctx.ExternalUsers.Include(e => e.PortalUser)
                           .FirstOrDefaultAsync(e => e.ExternalSubject == devOid, cancellationToken);

        if (externalUser is null)
        {
            externalUser = new ExternalUser
            {
                Organization = "DEV-ORG",
                // Assuming Oid maps to ExternalSubject (if not, set both)
                ExternalSubject = devOid, // Set required ExternalSubject
                FirstName = displayName, // Or parse from displayName/email as needed
                LastName = displayName,  // Or parse from displayName/email as needed
                UserExpiryDate = DateTimeOffset.UtcNow.AddYears(1), // Set a sensible expiry date
                PortalUser = new PortalUser
                {
                    Email = emailNorm,
                    DisplayName = displayName
                }
                // Add other properties as needed, but do not omit any existing lines
            };
            externalUser = ctx.ExternalUsers.Add(externalUser).Entity;
            await ctx.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("DevAuth: created ExternalUser for {Email} with OID {Oid}", emailNorm, devOid);
        }
        externalUser.PortalUser.ExternalUser = externalUser;
        await ctx.SaveChangesAsync(cancellationToken);

        if (_options.Workspaces is null || _options.Workspaces.Count == 0)
        {
            _logger.LogInformation("DevAuth: no workspaces configured; skipping workspace role bootstrap.");
            return;
        }

        var acronyms = _options.Workspaces
                  .Where(a => !string.IsNullOrWhiteSpace(a))
                  .Select(a => a.Trim().ToUpperInvariant())
                  .Distinct()
                  .ToArray();

        if (acronyms.Length == 0)
            return;

        var projects = await ctx.Projects
             .Where(p => acronyms.Contains(p.Project_Acronym_CD))
              .ToListAsync(cancellationToken);

        foreach (var acronym in acronyms)
        {
            var project = projects.FirstOrDefault(p => p.Project_Acronym_CD == acronym);
            if (project is null)
            {
                _logger.LogWarning("DevAuth: workspace {Acronym} not found in DB, skipping.", acronym);
            }
            else
            {
                var existingLink = await ctx.UserRolesLinks.FirstOrDefaultAsync(
                        l => l.PortalUserId == externalUser.PortalUser.Id && l.Project_ID == project.Project_ID,
                        cancellationToken);

                if (existingLink is null)
                {

                    // default dev role: web app and storage (external user role)
                    var role = await ctx.Project_Roles
                         .FirstOrDefaultAsync(r => r.Id == (int)Project_Role.RoleNames.WebAppAndStorage, cancellationToken);

                    if (role is null)
                    {
                        _logger.LogWarning("DevAuth: web app and storage role not found; cannot assign workspace role for {Acronym}", acronym);
                        continue;
                    }

                    var link = new UserRoleLinks
                    {
                        PortalUserId = externalUser.PortalUser.Id,
                        Project_ID = project.Project_ID,
                        RoleId = role.Id
                    };
                    ctx.UserRolesLinks.Add(link);
                    _logger.LogInformation(
                       "DevAuth: added user {Email} as {Role} on workspace {Acronym}",
                        emailNorm, role.Name, acronym);
                }
            }
        }

        await ctx.SaveChangesAsync(cancellationToken);
    }
}
