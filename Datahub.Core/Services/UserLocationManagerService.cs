using Microsoft.AspNetCore.Components.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Datahub.Core.EFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datahub.Core.UserTracking;

namespace Datahub.Core.Services
{
    public class UserLocationManagerService
    {
        private ILogger<UserLocationManagerService> _logger;
        private IUserInformationService _userInformationService;
        private IDbContextFactory<UserTrackingContext> _contextFactory;


        public UserLocationManagerService(ILogger<UserLocationManagerService> logger,
                                        IUserInformationService userInformationService,
                                        IDbContextFactory<UserTrackingContext> contextFactory)
        {
            _logger = logger;
            _userInformationService = userInformationService;
            _contextFactory = contextFactory;
        }

        public async Task RegisterNavigation(UserRecentLink link, bool isNew)
        {
            try
            {
                var user = await _userInformationService.GetUserAsync();
                var userId = user.Id;

                //var userRecentActions = new UserRecentLink() { url = eventArgs.Location, title = "my title", accessedTime = DateTimeOffset.Now, icon = "myicon" };
                using (var efCoreDatahubContext = _contextFactory.CreateDbContext())
                {
                    if (!isNew)
                        efCoreDatahubContext.Attach(link);
                    else
                    {
                        var userRecent = await efCoreDatahubContext.UserRecent.FirstOrDefaultAsync(u => u.UserId == userId);


                        if (userRecent == null)
                        {
                            userRecent = new UserRecent() { UserId = userId };
                            userRecent.UserRecentActions.Add(link);
                            efCoreDatahubContext.UserRecent.Add(userRecent);
                        }
                        else
                        {
                            if (userRecent.UserRecentActions.Count >= 5)
                            {
                                RemoveOldestNavigation(userRecent);
                            }
                            userRecent.UserRecentActions.Add(link);
                            //efCoreDatahubContext.UserRecent.Update(userRecent);
                        }
                    }
                    await efCoreDatahubContext.SaveChangesAsync();

                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cannot update navigation");
            }
        }

        private void RemoveOldestNavigation(UserRecent recentNavigations)
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
