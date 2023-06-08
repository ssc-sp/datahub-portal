﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Core.Services.UserManagement;

namespace Datahub.Core.Services.Security;

public class ServiceAuthManager
{
    private IMemoryCache serviceAuthCache;
    private readonly IDbContextFactory<DatahubProjectDBContext> dbFactory;
    private readonly IMSGraphService mSGraphService;
    private const int AUTH_KEY = 1;
    private const int PROJECT_ADMIN_KEY = 2;

    private ConcurrentDictionary<string, bool> _viewingAsGuest = new();

    public ServiceAuthManager(IMemoryCache serviceAuthCache, IDbContextFactory<DatahubProjectDBContext> dbFactory, IMSGraphService mSGraphService)
    {
        this.serviceAuthCache = serviceAuthCache;
        this.dbFactory = dbFactory;
        this.mSGraphService = mSGraphService;
    }

    internal List<string> GetAllProjects()
    {
        using var ctx = dbFactory.CreateDbContext();
        return ctx.Projects.Where(p => p.Project_Acronym_CD != null).Select(p => p.Project_Acronym_CD).ToList();
    }

    public void SetViewingAsGuest(string userId, bool isGuest)
    {
        _viewingAsGuest.AddOrUpdate(userId, isGuest, (k, v) => isGuest);
    }

    public bool GetViewingAsGuest(string userId)
    {
        return _viewingAsGuest.ContainsKey(userId) && _viewingAsGuest[userId];
    }

    public List<string> GetAdminProjectRoles(string userId)
    {
        if (userId != null && _viewingAsGuest.ContainsKey(userId) && _viewingAsGuest[userId])
        {
            return new List<string>();
        }

        var projects = GetAllProjects();
        projects = projects.Select(x => $"{x}-admin").ToList();
        return projects;
    }

    private static CancellationTokenSource _resetCacheToken = new CancellationTokenSource();

    public static readonly Regex Email_Extractor = new Regex(".*<(.*@.*)>", RegexOptions.Compiled);

    public static readonly Regex Email_Regex = new Regex(@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
            var percentage = 1.0;//100%
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
        bool isProjectAdmin = allProjectAdmins.ContainsKey(projectAcronym) ? allProjectAdmins[projectAcronym].Contains(userid) : false;

        var options = new MemoryCacheEntryOptions().SetPriority(CacheItemPriority.Normal).SetAbsoluteExpiration(TimeSpan.FromHours(1));
        options.AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token));

        return isProjectAdmin;
    }

    public List<string> GetProjectAdminsEmails(string projectAcronym)
    {
        using var ctx = dbFactory.CreateDbContext();

        return ctx.Project_Users
            .Where(a => 
                a.Project.Project_Acronym_CD == projectAcronym 
                && (a.RoleId == (int) Project_Role.RoleNames.Admin || a.RoleId == (int) Project_Role.RoleNames.WorkspaceLead)  
                && !string.IsNullOrEmpty(a.PortalUser.Email))
            .Select(f => f.PortalUser.Email)
            .ToList();
    }

    public List<string> GetProjectMailboxEmails(string projectAcronym)
    {
        using var ctx = dbFactory.CreateDbContext();
        var mailboxEmails = ctx.Projects.Where(u => u.Project_Acronym_CD == projectAcronym).Select(s => s.Project_Admin).FirstOrDefault();

        if (!string.IsNullOrEmpty(mailboxEmails))
        {
            return ExtractEmails(mailboxEmails);
        }
        else
        {
            return GetProjectAdminsEmails(projectAcronym);
        }
    }

    private async Task<Dictionary<string, List<string>>> CheckCacheForAdmins()
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
                    u.RoleId == (int) Project_Role.RoleNames.Admin 
                    || u.RoleId == (int) Project_Role.RoleNames.WorkspaceLead)
                .ToListAsync();

            foreach (var admin in adminsFromProjectUsersTable)
            {
                if (allProjectAdmins.TryGetValue(admin.Project.Project_Acronym_CD, out var projectAdmin))
                {
                    projectAdmin.Add(admin.PortalUser.GraphGuid);
                }
                else
                {
                    allProjectAdmins.Add(admin.Project.Project_Acronym_CD, new List<string> { admin.PortalUser.GraphGuid });
                }
            }

            serviceAuthCache.Set(PROJECT_ADMIN_KEY, allProjectAdmins, TimeSpan.FromHours(1));

        }

        return allProjectAdmins;
    }

    public async Task<ImmutableList<Datahub_Project>> GetUserAuthorizations(string userGraphId)
    {
        List<Datahub_Project_User> usersAuthorization;
        if (!serviceAuthCache.TryGetValue(AUTH_KEY, out usersAuthorization))
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();


            usersAuthorization = await ctx.Project_Users
                .AsNoTracking()
                .Include(a => a.Project)
                .Include(a => a.PortalUser)
                .ToListAsync();
            //var cacheEntryOptions = new MemoryCacheEntryOptions()
            //                // Set cache entry size by extension method.
            //                .SetSize(1)
            //                // Keep in cache for this time, reset time if accessed.
            //                .SetSlidingExpiration(TimeSpan.FromHours(1));
            serviceAuthCache.Set(AUTH_KEY, usersAuthorization, TimeSpan.FromHours(1));
        }
        return usersAuthorization.Where(a => a.PortalUser.GraphGuid == userGraphId)
            .Select(a => a.Project)
            .Distinct()
            .ToImmutableList();
    }
}
