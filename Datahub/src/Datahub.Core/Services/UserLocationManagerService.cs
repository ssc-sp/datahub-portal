using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Datahub.Core.UserTracking;

namespace Datahub.Core.Services
{
    public class UserLocationManagerService
    {
        private ILogger<UserLocationManagerService> _logger;
        private IUserInformationService _userInformationService;
        private IDbContextFactory<UserTrackingContext> _userTrackingContextFactory;


        public UserLocationManagerService(ILogger<UserLocationManagerService> logger,
                                        IUserInformationService userInformationService,
                                        IDbContextFactory<UserTrackingContext> userTrackingContextFactory)
        {
            _logger = logger;
            _userInformationService = userInformationService;
            _userTrackingContextFactory = userTrackingContextFactory;
        }


        private const ushort MaxLocationHistory = 6;

        public async Task RegisterNavigation(UserRecentLink link, bool isNew)
        {
            try
            {
                var user = await _userInformationService.GetCurrentGraphUserAsync();
                var userId = user.Id;

                await using var efCoreDatahubContext = await _userTrackingContextFactory.CreateDbContextAsync();

                var existingEntity = await efCoreDatahubContext.UserRecent
                    .FirstOrDefaultAsync(u => u.UserId == userId);
                
                if (existingEntity != null)
                {
                    efCoreDatahubContext.UserRecent.Remove(existingEntity);
                    await efCoreDatahubContext.SaveChangesAsync();
                }
                var links = GetRecentLinks(existingEntity, link);
                var newUserRecent = new UserRecent { UserId = userId, UserRecentActions = links};
                efCoreDatahubContext.UserRecent.Add(newUserRecent);
                
                await efCoreDatahubContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cannot update navigation");
            }
        }

        private static ICollection<UserRecentLink> GetRecentLinks(UserRecent userRecent, UserRecentLink link)
        {
            if (userRecent == null)
                return new List<UserRecentLink>(){ link };

            userRecent.UserRecentActions.Add(link);
            
            return userRecent.UserRecentActions
                .OrderByDescending(x => x.accessedTime)
                .DistinctBy(x => (x.DataProject, x.LinkType))
                .Take(MaxLocationHistory)
                .ToList();
        }

        public async Task DeleteUserRecent(string userId)
        {
            using (var efCoreDatahubContext = _userTrackingContextFactory.CreateDbContext())
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
            await using var efCoreDatahubContext = await _userTrackingContextFactory.CreateDbContextAsync();
            var userRecentActions = await efCoreDatahubContext.UserRecent
                .FirstOrDefaultAsync(u => u.UserId == userId);
            return userRecentActions;
        }

        public async Task RegisterNavigation(UserRecent recent)
        {
            using (var efCoreDatahubContext = _userTrackingContextFactory.CreateDbContext())
            {
                efCoreDatahubContext.UserRecent.Add(recent);
                await efCoreDatahubContext.SaveChangesAsync();
            }
        }
    }
}
