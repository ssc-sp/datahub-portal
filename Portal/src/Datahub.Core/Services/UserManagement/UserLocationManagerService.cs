using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.UserTracking;

namespace Datahub.Core.Services.UserManagement;

public class UserLocationManagerService
{
    private readonly ILogger<UserLocationManagerService> _logger;
    private readonly IUserInformationService _userInformationService;
    private readonly IDbContextFactory<DatahubProjectDBContext> _portalContext;


    public UserLocationManagerService(ILogger<UserLocationManagerService> logger,
        IUserInformationService userInformationService,
        IDbContextFactory<DatahubProjectDBContext> portalContext)
    {
        _logger = logger;
        _userInformationService = userInformationService;
        _portalContext = portalContext;
    }


    private const ushort MaxLocationHistory = 6;

    public async Task RegisterNavigation(UserRecentLink link, bool isNew)
    {
        try
        {
            var user = await _userInformationService.GetCurrentGraphUserAsync();
            var userId = user.Id;

            await using var efCoreDatahubContext = await _portalContext.CreateDbContextAsync();

            var existingEntity = await efCoreDatahubContext.UserRecent
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (existingEntity != null)
            {
                efCoreDatahubContext.UserRecent.Remove(existingEntity);
                await efCoreDatahubContext.SaveChangesAsync();
            }
            var links = GetRecentLinks(existingEntity, link);
            var newUserRecent = new UserRecent { UserId = userId, UserRecentActions = links };
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
            return new List<UserRecentLink>() { link };

        userRecent.UserRecentActions.Add(link);

        return userRecent.UserRecentActions
            .OrderByDescending(x => x.accessedTime)
            .DistinctBy(x => (x.DataProject, x.LinkType))
            .Take(MaxLocationHistory)
            .ToList();
    }

    public async Task DeleteUserRecent(string userId)
    {
        await using var efCoreDatahubContext = await _portalContext.CreateDbContextAsync();
        var userRecentActions = efCoreDatahubContext.UserRecent.FirstOrDefault(u => u.UserId == userId);
        if (userRecentActions != null)
        {
            efCoreDatahubContext.UserRecent.Remove(userRecentActions);
            await efCoreDatahubContext.SaveChangesAsync();
        }
    }

    public async Task<UserRecent> ReadRecentNavigations(string userId)
    {
        await using var efCoreDatahubContext = await _portalContext.CreateDbContextAsync();
        var userRecentActions = await efCoreDatahubContext.UserRecent
            .FirstOrDefaultAsync(u => u.UserId == userId);
        return userRecentActions;
    }

    public async Task RegisterNavigation(UserRecent recent)
    {
        await using var efCoreDatahubContext = await _portalContext.CreateDbContextAsync();
        efCoreDatahubContext.UserRecent.Add(recent);
        await efCoreDatahubContext.SaveChangesAsync();
    }
}