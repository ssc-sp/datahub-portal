using Microsoft.AspNetCore.Components.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NRCan.Datahub.Shared.EFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRCan.Datahub.Shared.Services
{
    public class UserLocationManagerService
    {
        private ILogger<UserLocationManagerService> _logger;
        private IUserInformationService _userInformationService;
        private IDbContextFactory<EFCoreDatahubContext> _contextFactory;


        public UserLocationManagerService(ILogger<UserLocationManagerService> logger,
                                        IUserInformationService userInformationService,
                                        IDbContextFactory<EFCoreDatahubContext> contextFactory)
        {
            _logger = logger;
            _userInformationService = userInformationService;
            _contextFactory = contextFactory;
        }

        public async Task RegisterNavigation(UserRecentLink link)
        {
            try
            {
                var user = await _userInformationService.GetUserAsync();
                var userId = user.Id;

                //var userRecentActions = new UserRecentLink() { url = eventArgs.Location, title = "my title", accessedTime = DateTimeOffset.Now, icon = "myicon" };
                using (var efCoreDatahubContext = _contextFactory.CreateDbContext())
                {
                    efCoreDatahubContext.Attach(link);
                    var recentNavigations = await efCoreDatahubContext.UserRecent.FirstOrDefaultAsync(u => u.UserId == userId);


                    if (recentNavigations == null)
                    {
                        recentNavigations = new UserRecent() { UserId = userId };
                        recentNavigations.UserRecentActions.Add(link);
                        efCoreDatahubContext.UserRecent.Add(recentNavigations);
                    }
                    else
                    {
                        if (recentNavigations.UserRecentActions.Count >= 5)
                        {
                            await RemoveOldestNavigation(recentNavigations);
                        }
                        recentNavigations.UserRecentActions.Add(link);
                        efCoreDatahubContext.UserRecent.Update(recentNavigations);
                    }
                    await efCoreDatahubContext.SaveChangesAsync();

                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cannot update navigation");
            }
        }

        private async Task RemoveOldestNavigation(UserRecent recentNavigations)
        {
            var date = recentNavigations.UserRecentActions.Min(x => x.accessedTime);
            var record = recentNavigations.UserRecentActions.Where(x => x.accessedTime == date).First();
            recentNavigations.UserRecentActions.Remove(record);
        }

        public async Task DeleteUserRecent(string userId)
        {
            using (var efCoreDatahubContext = _contextFactory.CreateDbContext())
            {
                var userRecentActions = efCoreDatahubContext.UserRecent.Where(u => u.UserId == userId).FirstOrDefault();
                if (userRecentActions != null)
                {
                    efCoreDatahubContext.UserRecent.Remove(userRecentActions);
                    await efCoreDatahubContext.SaveChangesAsync();
                }
            }
        }

        public async Task<UserRecent> ReadRecentNavigations(string userId)
        {
            using (var efCoreDatahubContext = _contextFactory.CreateDbContext())
            {
                var userRecentActions = await efCoreDatahubContext.UserRecent.FirstOrDefaultAsync(u => u.UserId == userId);
                return userRecentActions;
            }
        }

        public async Task RegisterNavigation(UserRecent recent)
        {
            using (var efCoreDatahubContext = _contextFactory.CreateDbContext())
            {
                efCoreDatahubContext.UserRecent.Add(recent);
                await efCoreDatahubContext.SaveChangesAsync();
            }
        }
    }
}
