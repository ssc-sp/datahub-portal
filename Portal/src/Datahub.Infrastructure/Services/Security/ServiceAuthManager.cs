using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Datahub.Application.Services.Security;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Datahub.Infrastructure.Services.Security;

public class ServiceAuthManager : IServiceAuthManager
{
    private const int ENTRA_AUTH_KEY = 1;
    private const int PROJECT_ADMIN_KEY = 2;
    private const int EXTERNAL_AUTH_KEY = 3;

    private IMemoryCache serviceAuthCache;
    private readonly IDbContextFactory<DatahubProjectDBContext> dbFactory;
    private readonly ILogger<ServiceAuthManager> _logger;

    private ConcurrentDictionary<string, bool> viewingAsGuest = new();

    public ServiceAuthManager(IMemoryCache serviceAuthCache, IDbContextFactory<DatahubProjectDBContext> dbFactory, ILogger<ServiceAuthManager> logger)
    {
        this.serviceAuthCache = serviceAuthCache;
        this.dbFactory = dbFactory;
        _logger = logger;
    }

    public List<string> GetAllProjects()
    {
        using var ctx = dbFactory.CreateDbContext();
        return ctx.Projects.Where(p => p.Project_Acronym_CD != null).Select(p => p.Project_Acronym_CD).ToList();
    }

    public void SetViewingAsGuest(string userId, bool isGuest)
    {
        viewingAsGuest.AddOrUpdate(userId, isGuest, (k, v) => isGuest);
    }

    public bool GetViewingAsGuest(string userId)
    {
        return viewingAsGuest.ContainsKey(userId) && viewingAsGuest[userId];
    }

    public List<string> GetAdminProjectRoles(string userId)
    {
        if (userId != null && viewingAsGuest.ContainsKey(userId) && viewingAsGuest[userId])
        {
            return new List<string>();
        }

        var projects = GetAllProjects();
        projects = projects.Select(x => $"{x}-admin").ToList();
        return projects;
    }

    private static CancellationTokenSource resetCacheToken = new CancellationTokenSource();

    public static readonly Regex Email_Extractor = new Regex(".*<(.*@.*)>", RegexOptions.Compiled);

    public static readonly Regex Email_Regex =
        new Regex(
            @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string? ExtractEmail(string input)
    {
        if (Email_Regex.IsMatch(input))
            return input;
        var match = Email_Extractor.Match(input);
        if (match.Success)
        {
            var ingroup = match.Groups[1].Value.ToLowerInvariant();
            if (Email_Regex.IsMatch(ingroup))
                return ingroup;
        }

        return null;
    }

    public static List<string> ExtractEmails(string emailList)
    {
        var split = emailList.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(email => email.Trim()).ToArray();
        return [.. split.Select(b => ExtractEmail(b)?.ToLowerInvariant()).Where(b => b != null)];
    }

    public bool InvalidateAuthCache()
    {
        var cache = serviceAuthCache as MemoryCache;
        if (cache != null)
        {
            //https://stackoverflow.com/questions/49176244/asp-net-core-clear-cache-from-imemorycache-set-by-set-method-of-cacheextensions/49425102#49425102
            //this weird trick removes all the entries
            var percentage = 1.0; //100%
            cache.Compact(percentage);
            _logger.LogDebug("ServiceAuth cache invalidated (compacted to {Percentage:P0}).", percentage);
            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task<bool> IsProjectAdmin(PortalUser portalUser, string projectAcronym)
    {
        if (portalUser.EntraUser == null)
        {
            return false;
        }
        return await IsProjectAdmin(portalUser.EntraUser.GraphGuid, projectAcronym);
    }

    public async Task<bool> IsProjectAdmin(string userid, string projectAcronym)
    {
        var allProjectAdmins = await CheckCacheForAdmins();
        bool isProjectAdmin = allProjectAdmins.ContainsKey(projectAcronym)
            ? allProjectAdmins[projectAcronym].Contains(userid)
            : false;

        var options = new MemoryCacheEntryOptions().SetPriority(CacheItemPriority.Normal)
            .SetAbsoluteExpiration(TimeSpan.FromHours(1));
        options.AddExpirationToken(new CancellationChangeToken(resetCacheToken.Token));

        return isProjectAdmin;
    }

    public List<string> GetProjectAdminsEmails(string projectAcronym)
    {
        using var ctx = dbFactory.CreateDbContext();

        return ctx.UserRolesLinks
            .Where(a =>
                a.Project.Project_Acronym_CD == projectAcronym
                && (a.RoleId == (int)Project_Role.RoleNames.Admin ||
                    a.RoleId == (int)Project_Role.RoleNames.WorkspaceLead)
                && !string.IsNullOrEmpty(a.PortalUser.Email))
            .Select(f => f.PortalUser.Email)
            .ToList();
    }

    public List<string> GetProjectMailboxEmails(string projectAcronym)
    {
        using var ctx = dbFactory.CreateDbContext();
        var mailboxEmails = ctx.Projects.Where(u => u.Project_Acronym_CD == projectAcronym).Select(s => s.Project_Admin)
            .FirstOrDefault();

        if (!string.IsNullOrEmpty(mailboxEmails))
        {
            return ExtractEmails(mailboxEmails);
        }
        else
        {
            return GetProjectAdminsEmails(projectAcronym);
        }
    }

    public async Task<Dictionary<string, List<string>>> CheckCacheForAdmins()
    {
        Dictionary<string, List<string>>? allProjectAdmins;
        if (!serviceAuthCache.TryGetValue(PROJECT_ADMIN_KEY, out allProjectAdmins))
        {
            allProjectAdmins = new Dictionary<string, List<string>>();
            await using var ctx = await dbFactory.CreateDbContextAsync();

            var adminsFromProjectUsersTable = await ctx.UserRolesLinks
                .AsNoTracking()
                .Include(a => a.Project)
                .Include(a => a.PortalUser)
                .ThenInclude(p => p.EntraUser)
                .Where(u =>
                    u.PortalUser != null && u.PortalUser.EntraUser != null &&
                    (u.RoleId == (int)Project_Role.RoleNames.Admin
                    || u.RoleId == (int)Project_Role.RoleNames.WorkspaceLead))
                .ToListAsync();

            foreach (var admin in adminsFromProjectUsersTable)
            {
                if (allProjectAdmins.TryGetValue(admin.Project.Project_Acronym_CD, out var projectAdmin))
                {
                    projectAdmin.Add(admin.PortalUser!.EntraUser!.GraphGuid);
                }
                else
                {
                    allProjectAdmins.Add(
                        admin.Project.Project_Acronym_CD,
                        new List<string> { admin.PortalUser!.EntraUser!.GraphGuid });
                }
            }

            serviceAuthCache.Set(PROJECT_ADMIN_KEY, allProjectAdmins, TimeSpan.FromHours(1));
            _logger.LogDebug("Loaded project admin cache with {ProjectCount} projects and set expiration to1 hour.", allProjectAdmins.Count);
        }
        else
        {
            _logger.LogDebug("Project admin cache hit.");
        }

        return allProjectAdmins!;
    }

    public async Task<ImmutableList<(Project_Role Role, Datahub_Project Project)>> GetEntraUserAuthorizations(string userGraphId)
    {
        if (serviceAuthCache.TryGetValue(
            ENTRA_AUTH_KEY,
            out var usersAuthorizationObj) && usersAuthorizationObj is Dictionary<string, List<(Project_Role, Datahub_Project)>> usersAuthorization)
        {
            if (usersAuthorization.TryGetValue(userGraphId, out var userAuths))
            {
                _logger.LogDebug("Entra user authorizations cache hit for user {UserGraphId}. Roles: {RoleCount}", userGraphId, userAuths.Count);
                return userAuths
                    .ToImmutableList();
            }
            else
            {
                _logger.LogDebug("Entra user authorizations cache miss for user {UserGraphId}.", userGraphId);
            }
        }
        else
        {
            _logger.LogDebug("Entra user authorizations global cache miss; loading from DB.");
        }

        await using var ctx = await dbFactory.CreateDbContextAsync();

        var entraUsersRoles = await ctx.UserRolesLinks
            .AsNoTracking()
            .Include(a => a.Project)
            .Include(a => a.PortalUser)
            .ThenInclude(p => p.EntraUser)
            .Include(a => a.Role)
            .Where(u => u.PortalUser != null && u.PortalUser.EntraUser != null)
            .ToListAsync();

        var newUsersAuthorization = entraUsersRoles
            .GroupBy(u => u.PortalUser!.EntraUser!.GraphGuid)
            .ToDictionary(u => u.Key, u =>
                u.Select(a => (a.Role!, a.Project!))
                    .ToList());

        serviceAuthCache.Set(ENTRA_AUTH_KEY, newUsersAuthorization, TimeSpan.FromMinutes(5));
        _logger.LogDebug("Loaded Entra user authorizations from DB for {UserCount} users; cache set to5 minutes.", newUsersAuthorization.Count);

        // if the user is not in the dictionary, return an empty list
        if (newUsersAuthorization.TryGetValue(userGraphId, out var newUserAuths))
        {
            _logger.LogDebug("Returning Entra authorizations for user {UserGraphId}. Roles: {RoleCount}", userGraphId, newUserAuths.Count);
            return newUserAuths.ToImmutableList();
        }

        _logger.LogDebug("No Entra authorizations found for user {UserGraphId}. Returning empty.", userGraphId);
        return ImmutableList<(Project_Role, Datahub_Project)>.Empty;
    }

    public async Task<bool> IsUserCbrOwner(string userEmail)
    {
        using var ctx = await dbFactory.CreateDbContextAsync();
        return await ctx.GCHostingWorkspaceDetails.AnyAsync(d => d.LeadEmail == userEmail);
    }

    public async Task<List<string>> GetUserCbrWorkspaceAcronyms(string userEmail)
    {
        await using var ctx = await dbFactory.CreateDbContextAsync();
        return await ctx.GCHostingWorkspaceDetails
            .Where(d => d.LeadEmail == userEmail)
            .SelectMany(g => g.WorkspacesInBudget)
            .Select(w => w.Project_Acronym_CD)
            .ToListAsync();
    }

    public async Task<ImmutableList<(Project_Role Role, Datahub_Project Project)>> GetExternalUserAuthorizations(string externalId)
    {
        // Try cache first
        if (serviceAuthCache.TryGetValue(
            EXTERNAL_AUTH_KEY,
            out var externalAuthorizationObj) && externalAuthorizationObj is Dictionary<string, List<(Project_Role, Datahub_Project)>> externalAuthorization)
        {
            if (externalAuthorization.TryGetValue(externalId, out var cachedAuths))
            {
                _logger.LogDebug("External user authorizations cache hit for externalId {ExternalId}. Roles: {RoleCount}", externalId, cachedAuths.Count);
                return cachedAuths.ToImmutableList();
            }
            else
            {
                _logger.LogDebug("External user authorizations cache miss for externalId {ExternalId}.", externalId);
            }
        }
        else
        {
            _logger.LogDebug("External user authorizations global cache miss; loading from DB.");
        }

        await using var ctx = await dbFactory.CreateDbContextAsync();

        var externalUserRoles = await ctx.UserRolesLinks
            .AsNoTracking()
            .Include(a => a.Project)
            .Include(a => a.PortalUser)
            .ThenInclude(p => p.ExternalUser)
            .Include(a => a.Role)
            .Where(u => u.PortalUser != null && u.PortalUser.ExternalUser != null)
            .ToListAsync();

        var newExternalAuthorization = externalUserRoles
            .GroupBy(u => u.PortalUser!.ExternalUser!.ExternalSubject)
            .ToDictionary(g => g.Key, g =>
                g.Select(a => (Role: a.Role!, Project: a.Project!))
                    .Where(rp => rp.Role != null && rp.Role.IsExternalRole)
                    .ToList());

        serviceAuthCache.Set(EXTERNAL_AUTH_KEY, newExternalAuthorization, TimeSpan.FromMinutes(5));
        _logger.LogDebug("Loaded external user authorizations from DB for {UserCount} users; cache set to 5 minutes.", newExternalAuthorization.Count);

        if (newExternalAuthorization.TryGetValue(externalId, out var newAuths))
        {
            _logger.LogDebug("Returning external authorizations for externalId {ExternalId}. Roles: {RoleCount}", externalId, newAuths.Count);
            return newAuths.ToImmutableList();
        }

        _logger.LogDebug("No external authorizations found for externalId {ExternalId}. Returning empty.", externalId);
        return ImmutableList<(Project_Role, Datahub_Project)>.Empty;
    }
}
