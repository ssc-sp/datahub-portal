using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Datahub.Core.Components;
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

    public async Task RegisterNavigation(UserRecentLink link)
    {
        try
        {
            var user = await _userInformationService.GetCurrentPortalUserAsync();
            await using var efCoreDatahubContext = await _portalContext.CreateDbContextAsync();

            //remove existing entry for the same LinkType and DataProject if it exists
            var existingEntity = await efCoreDatahubContext.UserRecentLinks
                .FirstOrDefaultAsync(l => l.UserId == user.Id && l.LinkType == link.LinkType &&
                    (l.LinkType == DatahubLinkType.DataProject || l.DataProject == link.DataProject));

            // if the link is new, we need to add it to the database
            if (existingEntity == null)
            {
                link.UserId = user.Id;
                efCoreDatahubContext.UserRecentLinks.Add(link);
            }
            // if the link is not new but is a resource article
            else if (existingEntity.LinkType == DatahubLinkType.ResourceArticle)
            {
                // we need to check if the resource article id is the same as the existing one
                if (existingEntity.ResourceArticleId == link.ResourceArticleId)
                {
                    // if it is, we need to update the accessed time
                    existingEntity.accessedTime = link.accessedTime;
                }
                else
                {
                    // otherwise, we need to add a new entry
                    link.UserId = user.Id;
                    efCoreDatahubContext.UserRecentLinks.Remove(existingEntity);
                    efCoreDatahubContext.UserRecentLinks.Add(link);
                }
            }
            // override the data project link
            else if (existingEntity.LinkType == DatahubLinkType.DataProject)
            {
                if (existingEntity.DataProject == link.DataProject)
                {
                    existingEntity.accessedTime = link.accessedTime;
                }
                else
                {
                    // otherwise, we need to add a new entry
                    link.UserId = user.Id;
                    efCoreDatahubContext.UserRecentLinks.Remove(existingEntity);
                    efCoreDatahubContext.UserRecentLinks.Add(link);
                }
            }
            // if the link is not new but is not a resource article
            else
            {
                // we need to update the accessed time
                existingEntity.accessedTime = link.accessedTime;
            }

            await efCoreDatahubContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cannot update navigation");
        }
    }

    public async Task<ICollection<UserRecentLink>> GetRecentLinks(string userId, int maxRecentLinks)
    {
        await using var efCoreDatahubContext = await _portalContext.CreateDbContextAsync();

        var allowedLinks = new HashSet<DatahubLinkType>()
        {
            DatahubLinkType.DataProject,
            DatahubLinkType.Storage,
            DatahubLinkType.Databricks,
            DatahubLinkType.ResourceArticle
        };

        return efCoreDatahubContext.UserRecentLinks
            .AsNoTracking()
            .Include(l => l.User)
            .Where(l => l.User.GraphGuid == userId && (l.DataProject != null || l.ResourceArticleId != null) && allowedLinks.Contains(l.LinkType))
            .OrderByDescending(l => l.accessedTime)
            .Take(maxRecentLinks)
            .ToList();
    }
}