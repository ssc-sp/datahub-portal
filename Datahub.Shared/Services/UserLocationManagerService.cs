using Microsoft.AspNetCore.Components.Routing;
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
        private EFCoreDatahubContext _efCoreDatahubContext;

        public UserLocationManagerService(ILogger<UserLocationManagerService> logger, 
                                        IUserInformationService userInformationService,
                                        EFCoreDatahubContext efCoreDatahubContext)
        {
            _logger = logger;
            _userInformationService = userInformationService;
            _efCoreDatahubContext = efCoreDatahubContext;
        }

        public async Task RegisterNavigation(LocationChangedEventArgs eventArgs)
        {
            var user = await _userInformationService.GetCurrentUserAsync();
            var userId = user.Id;

            var userRecentActions = new UserRecentActions() { url = eventArgs.Location, title = "my title", accessedTime = DateTimeOffset.Now, icon = "myicon" };
            
            var recentNavigations = await ReadRecentNavigations(userId);

            if (recentNavigations == null)
            {
                recentNavigations = new UserRecent() { UserId = userId };
                recentNavigations.UserRecentActions.Add(userRecentActions);                
                await RegisterNavigation(recentNavigations);
            }
            else
            {
                if (recentNavigations.UserRecentActions.Count >= 5)
                {
                    await RemoveOldestNavigation(recentNavigations);
                }
                recentNavigations.UserRecentActions.Add(userRecentActions);
                _efCoreDatahubContext.UserRecent.Update(recentNavigations);
                await _efCoreDatahubContext.SaveChangesAsync();
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
            var userRecentActions = _efCoreDatahubContext.UserRecent.Where(u => u.UserId == userId).FirstOrDefault();
            if (userRecentActions != null)
            { 
                _efCoreDatahubContext.UserRecent.Remove(userRecentActions);
                await _efCoreDatahubContext.SaveChangesAsync();
            }
        }
        
        public async Task<UserRecent> ReadRecentNavigations(string userId)
        {
            var userRecentActions = _efCoreDatahubContext.UserRecent.Where(u => u.UserId == userId).FirstOrDefault();
            return userRecentActions;
        }

        public async Task RegisterNavigation(UserRecent recent)
        {            
            _efCoreDatahubContext.UserRecent.Add(recent);
            await _efCoreDatahubContext.SaveChangesAsync();
        }
    }
}
