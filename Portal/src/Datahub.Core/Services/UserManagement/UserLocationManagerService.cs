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

    public async Task RegisterNavigation(UserRecentLink link, bool isNew)
    {
        try
        {
            var user = await _userInformationService.GetAuthenticatedPortalUser();

            await using var efCoreDatahubContext = await _portalContext.CreateDbContextAsync();

            //remove existing entry for the same LinkType and DataProject if it exists
            var existingEntity = efCoreDatahubContext.UserRecentLinks
                .Include(l => l.User)
                .FirstOrDefault(l => l.User == user && l.LinkType == link.LinkType &&
                            l.DataProject == link.DataProject);
            

            if (existingEntity != null)
            {
                efCoreDatahubContext.UserRecentLinks.Remove(existingEntity);
                await efCoreDatahubContext.SaveChangesAsync();
            }
            link.UserId = user.Id;
            efCoreDatahubContext.UserRecentLinks.Add(link);

            await efCoreDatahubContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cannot update navigation");
        }
    }

    public async Task<ICollection<UserRecentLink>> GetRecentLinks(string userId)
    {
        await using var efCoreDatahubContext = await _portalContext.CreateDbContextAsync();
        return efCoreDatahubContext.UserRecentLinks
            .Include(l => l.User)
            .Where(l => l.User.GraphGuid == userId).ToList();
    }
}