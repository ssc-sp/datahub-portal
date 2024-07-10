using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Datahub.Application.Services.Security;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Datahub.Infrastructure.Services.Security;

public class ServiceAuthManager : IServiceAuthManager
{
    private const int AUTH_KEY = 1;
    private const int PROJECT_ADMIN_KEY = 2;

    private IMemoryCache serviceAuthCache;
    private readonly IDbContextFactory<DatahubProjectDBContext> dbFactory;

    private ConcurrentDictionary<string, bool> viewingAsGuest = new();

    public ServiceAuthManager(IMemoryCache serviceAuthCache, IDbContextFactory<DatahubProjectDBContext> dbFactory)
    {
        this.serviceAuthCache = serviceAuthCache;
        this.dbFactory = dbFactory;
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

    public static string ExtractEmail(string input)
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
        return split.Select(b => ExtractEmail(b)?.ToLowerInvariant()).Where(b => b != null).ToList();
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
            return true;
        }
        else
        {
            return false;
        }
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

        return ctx.Project_Users
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
        Dictionary<string, List<string>> allProjectAdmins;
        if (!serviceAuthCache.TryGetValue(PROJECT_ADMIN_KEY, out allProjectAdmins))
        {
            allProjectAdmins = new Dictionary<string, List<string>>();
            await using var ctx = await dbFactory.CreateDbContextAsync();

            var adminsFromProjectUsersTable = await ctx.Project_Users
                .AsNoTracking()
                .Include(a => a.Project)
                .Include(a => a.PortalUser)
                .Where(u =>
                    u.RoleId == (int)Project_Role.RoleNames.Admin
                    || u.RoleId == (int)Project_Role.RoleNames.WorkspaceLead)
                .ToListAsync();

            foreach (var admin in adminsFromProjectUsersTable)
            {
                if (allProjectAdmins.TryGetValue(admin.Project.Project_Acronym_CD, out var projectAdmin))
                {
                    projectAdmin.Add(admin.PortalUser.GraphGuid);
                }
                else
                {
                    allProjectAdmins.Add(
                        admin.Project.Project_Acronym_CD,
                        new List<string> { admin.PortalUser.GraphGuid });
                }
            }

            serviceAuthCache.Set(PROJECT_ADMIN_KEY, allProjectAdmins, TimeSpan.FromHours(1));
        }

        return allProjectAdmins;
    }

    public async Task<ImmutableList<(Project_Role Role, Datahub_Project Project)>> GetUserAuthorizations(string userGraphId)
    {
        if (serviceAuthCache.TryGetValue(
            AUTH_KEY,
            out Dictionary<string, List<(Project_Role, Datahub_Project)>> usersAuthorization))
        {
            if (usersAuthorization.TryGetValue(userGraphId, out var userAuths))
            {
                return userAuths
                    .ToImmutableList();
            }
        }

        await using var ctx = await dbFactory.CreateDbContextAsync();

        var usersRoles = await ctx.Project_Users
            .AsNoTracking()
            .Include(a => a.Project)
            .Include(a => a.PortalUser)
            .Include(a => a.Role)
            .ToListAsync();

        usersAuthorization = usersRoles
            .Where(u => u.PortalUser is not null)
            .GroupBy(u => u.PortalUser.GraphGuid)
            .ToDictionary(u => u.Key, u =>
                u.Select(a => (a.Role, a.Project))
                    .ToList());

        serviceAuthCache.Set(AUTH_KEY, usersAuthorization, TimeSpan.FromMinutes(5));

        // if the user is not in the dictionary, return an empty list
        if (!usersAuthorization.ContainsKey(userGraphId))
        {
            return ImmutableList<(Project_Role, Datahub_Project)>.Empty;
        }

        return usersAuthorization[userGraphId]
            .ToImmutableList();
    }
}