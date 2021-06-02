using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using NRCan.Datahub.Data.Projects;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NRCan.Datahub.Portal.Services
{
    public class ServiceAuthManager
    {
        private IMemoryCache serviceAuthCache;
        private readonly IMemoryCache projectAdminCache;
        private readonly IDbContextFactory<DatahubProjectDBContext> dbFactory;
        private const int AUTH_KEY = 1;
        private const int PROJECT_ADMIN_KEY = 2;

        public ServiceAuthManager(IMemoryCache serviceAuthCache, IMemoryCache projectAdminCache, IDbContextFactory<DatahubProjectDBContext> dbFactory)
        {
            this.serviceAuthCache = serviceAuthCache;
            this.projectAdminCache = projectAdminCache;
            this.dbFactory = dbFactory;
        }

        private static CancellationTokenSource _resetCacheToken = new CancellationTokenSource();

        public static readonly Regex Email_Extractor = new Regex(".*<(.*@.*)>", RegexOptions.Compiled);

        public static readonly Regex Email_Regex = new Regex(@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
            return split.Select(b => ExtractEmail(b)?.ToLowerInvariant()).Where(b => b != null).ToList();            
        }

        public async Task ClearProjectAdminCache()
        {
            serviceAuthCache.Remove(PROJECT_ADMIN_KEY);
            if (_resetCacheToken != null && !_resetCacheToken.IsCancellationRequested && _resetCacheToken.Token.CanBeCanceled)
            {
                _resetCacheToken.Cancel();
                _resetCacheToken.Dispose();
            }

            _resetCacheToken = new CancellationTokenSource();
        }

        public async Task<bool> IsProjectAdmin(string email)
        {
            
            bool isProjectAdmin = false;
            var normEmail = email.ToLowerInvariant();

            if (!projectAdminCache.TryGetValue(normEmail, out isProjectAdmin))
            {
                ImmutableHashSet<string> allProjectAdmins;
                if (!serviceAuthCache.TryGetValue(PROJECT_ADMIN_KEY, out allProjectAdmins))
                {
                    using var ctx = dbFactory.CreateDbContext();
                    var allProjectAdminsList = await ctx.Projects.Select(p => p.Project_Admin).Where(p => p != null).ToListAsync();
                    allProjectAdmins = allProjectAdminsList.SelectMany(ExtractEmails).ToImmutableHashSet();

                    serviceAuthCache.Set(PROJECT_ADMIN_KEY, allProjectAdmins, TimeSpan.FromHours(1));
                }
                isProjectAdmin = allProjectAdmins.Contains(normEmail);
                /* some other code removed for brevity */
                var options = new MemoryCacheEntryOptions().SetPriority(CacheItemPriority.Normal).SetAbsoluteExpiration(TimeSpan.FromHours(1));
                options.AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token));

                projectAdminCache.Set(normEmail, isProjectAdmin, options);
            }
            return isProjectAdmin;
        }

        public async Task<ImmutableList<Datahub_Project>> GetUserAuthorizations(string email)
        {            
            List<Datahub_Project_Access_Request> usersAuthorization;
            if (!serviceAuthCache.TryGetValue(AUTH_KEY, out usersAuthorization))
            {
                using var ctx = dbFactory.CreateDbContext();
                usersAuthorization = await ctx.Access_Requests.Include(a => a.Project).ToListAsync();
                //var cacheEntryOptions = new MemoryCacheEntryOptions()
                //                // Set cache entry size by extension method.
                //                .SetSize(1)
                //                // Keep in cache for this time, reset time if accessed.
                //                .SetSlidingExpiration(TimeSpan.FromHours(1));
                serviceAuthCache.Set(AUTH_KEY, usersAuthorization, TimeSpan.FromHours(1));
            }
            return usersAuthorization.Where(a => a.Completion_DT != null && a.User_Name.Equals(email,StringComparison.InvariantCultureIgnoreCase))
                .Select(a => a.Project)
                .ToImmutableList();
        }


    }
}
