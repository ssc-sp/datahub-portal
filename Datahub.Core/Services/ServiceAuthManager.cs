﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Datahub.Core.EFCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace Datahub.Core.Services
{
    public class ServiceAuthManager
    {
        private IMemoryCache serviceAuthCache;
        private readonly IMemoryCache projectAdminCache;
        private readonly IDbContextFactory<DatahubProjectDBContext> dbFactory;
        private readonly IMSGraphService mSGraphService;
        private const int AUTH_KEY = 1;
        private const int PROJECT_ADMIN_KEY = 2;

        public ServiceAuthManager(IMemoryCache serviceAuthCache, IMemoryCache projectAdminCache, IDbContextFactory<DatahubProjectDBContext> dbFactory, IMSGraphService mSGraphService)
        {
            this.serviceAuthCache = serviceAuthCache;
            this.projectAdminCache = projectAdminCache;
            this.dbFactory = dbFactory;
            this.mSGraphService = mSGraphService;
        }

        internal List<string> GetAllProjects()
        {
            using var ctx = dbFactory.CreateDbContext();
            return ctx.Projects.Where(p => p.Project_Acronym_CD != null).Select(p => p.Project_Acronym_CD).ToList();
        }

        public List<string> GetAdminProjectRoles()
        {
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


        public async Task<string> GetAdminUserString(string projectAcronym)
        {
            var adminEmailList = GetProjectAdminsEmails(projectAcronym);
            StringBuilder admins = new();
           
            foreach (var admin in adminEmailList)
            {
                var userId = await mSGraphService.GetUserIdFromEmailAsync(admin, CancellationToken.None);
                var userName = await mSGraphService.GetUserName(userId, CancellationToken.None);
                admins.Append($"{userName}; ");
            }
            return admins.ToString().Trim().TrimEnd(';');
        }
        //public async Task ClearProjectAdminCache()
        //{
        //    await Task.Run(() =>
        //    {
        //        serviceAuthCache.Remove(PROJECT_ADMIN_KEY);
        //        if (_resetCacheToken != null && !_resetCacheToken.IsCancellationRequested && _resetCacheToken.Token.CanBeCanceled)
        //        {
        //            _resetCacheToken.Cancel();
        //            _resetCacheToken.Dispose();
        //        }
        //        _resetCacheToken = new CancellationTokenSource();
        //    });

        //    await checkCacheForAdmins();

        //}



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

            Dictionary<string, List<string>> allProjectAdmins;
            allProjectAdmins = await checkCacheForAdmins();
            bool isProjectAdmin = allProjectAdmins.ContainsKey(projectAcronym) ? allProjectAdmins[projectAcronym].Contains(userid) : false;


            var options = new MemoryCacheEntryOptions().SetPriority(CacheItemPriority.Normal).SetAbsoluteExpiration(TimeSpan.FromHours(1));
            options.AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token));

            return isProjectAdmin;
        }

        public List<string> GetProjectAdminsEmails(string projectAcronym)
        { 
            using var ctx = dbFactory.CreateDbContext();
            var project = ctx.Projects.Where(p => p.Project_Acronym_CD.ToLower() == projectAcronym.ToLower()).First();

            return ctx.Project_Users.Where(a => a.Project == project && a.IsAdmin && !string.IsNullOrEmpty(a.User_Name)).Select(f => f.User_Name).ToList();

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
        public async Task RegisterProjectAdmin(Datahub_Project project, string currentUserId)
        {
            using var ctx = dbFactory.CreateDbContext();
            var users = ctx.Project_Users.Where(u => u.Project == project).ToList();

            //clean admins not in list            
            var extractedEmails = ExtractEmails(project.Project_Admin);

            foreach (var user in users.Where(u => u.IsAdmin))
            {
                var email = await mSGraphService.GetUserEmail(user.User_ID, CancellationToken.None);
                if (!extractedEmails.Contains(email.ToLower()))
                {
                    user.IsAdmin = false;                    
                }
            }


            foreach (var email in extractedEmails)
            {
                var adminUserid = await mSGraphService.GetUserIdFromEmailAsync(email, CancellationToken.None);
                if (!string.IsNullOrEmpty(adminUserid))
                {
                    //if user exists but is not admin
                    if (users.Any(u => u.User_ID == adminUserid && !u.IsAdmin))
                    {
                        var user = users.Where(u => u.User_ID == adminUserid).First();
                        user.IsAdmin = true;
                    }
                    //else if user doesnt exist
                    else if (!users.Any(u => u.User_ID == adminUserid))
                    {
                        var proj = ctx.Projects.Where(p => p == project).FirstOrDefault();
                        var user = new Datahub_Project_User()
                        {
                            User_ID = adminUserid,
                            Project = proj,
                            ApprovedUser = currentUserId,
                            Approved_DT = DateTime.Now,
                            IsAdmin = true,
                            IsDataApprover = false,
                            User_Name = email
                        };

                        ctx.Project_Users.Add(user);
                    }
                }
            }

            await ctx.SaveChangesAsync();
            //await ClearProjectAdminCache();
            InvalidateAuthCache();
            ctx.Dispose();
        }
        private async Task<List<string>> GetProjectRoles()
        {
            var allProjectAdmins = await checkCacheForAdmins();
            List<string> projectRoles = new();
            foreach (var item in allProjectAdmins)
            {
                projectRoles.Add($"{item.Key}-admin");
            }
            return projectRoles;
        }

        private async Task<Dictionary<string, List<string>>> checkCacheForAdmins()
        {
            Dictionary<string, List<string>> allProjectAdmins;
            if (!serviceAuthCache.TryGetValue(PROJECT_ADMIN_KEY, out allProjectAdmins))
            {
                allProjectAdmins = new Dictionary<string, List<string>>();
                using var ctx = dbFactory.CreateDbContext();

                var adminsFromProjectUsersTable = await ctx.Project_Users.Include(a => a.Project).Where(u => u.IsAdmin).ToListAsync();

                foreach (var admin in adminsFromProjectUsersTable)
                {
                    if (allProjectAdmins.ContainsKey(admin.Project?.Project_Acronym_CD))
                    {
                        allProjectAdmins[admin.Project.Project_Acronym_CD].Add(admin.User_ID);
                    }
                    else
                    {
                        allProjectAdmins.Add(admin.Project.Project_Acronym_CD, new List<string>() { admin.User_ID });
                    }
                }

                serviceAuthCache.Set(PROJECT_ADMIN_KEY, allProjectAdmins, TimeSpan.FromHours(1));

            }

            return allProjectAdmins;
        }

        public async Task<ImmutableList<Datahub_Project>> GetUserAuthorizations(string userId)
        {
            List<Datahub_Project_User> usersAuthorization;
            if (!serviceAuthCache.TryGetValue(AUTH_KEY, out usersAuthorization))
            {
                using var ctx = dbFactory.CreateDbContext();


                usersAuthorization = await ctx.Project_Users.Include(a => a.Project).ToListAsync();
                //var cacheEntryOptions = new MemoryCacheEntryOptions()
                //                // Set cache entry size by extension method.
                //                .SetSize(1)
                //                // Keep in cache for this time, reset time if accessed.
                //                .SetSlidingExpiration(TimeSpan.FromHours(1));
                serviceAuthCache.Set(AUTH_KEY, usersAuthorization, TimeSpan.FromHours(1));
            }
            return usersAuthorization.Where(a => a.User_ID == userId)
                .Select(a => a.Project)
                .Distinct()
                .ToImmutableList();

        }


    }
}
