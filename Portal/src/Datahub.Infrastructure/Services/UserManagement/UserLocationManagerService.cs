using Datahub.Application.Services.UserManagement;
using Datahub.Core.Components;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.UserTracking;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services.UserManagement;

public class UserLocationManagerService : IUserLocationManagerService
{
    private readonly ILogger<UserLocationManagerService> logger;
    private readonly IUserInformationService userInformationService;
    private readonly IDbContextFactory<DatahubProjectDBContext> portalContext;

    public UserLocationManagerService(
        ILogger<UserLocationManagerService> logger,
        IUserInformationService userInformationService,
        IDbContextFactory<DatahubProjectDBContext> portalContext)
    {
        this.logger = logger;
        this.userInformationService = userInformationService;
        this.portalContext = portalContext;
    }

    public async Task RegisterNavigation(UserRecentLink link)
    {
        try
        {
            var user = await userInformationService.GetCurrentPortalUserAsync();
            await using var efCoreDatahubContext = await portalContext.CreateDbContextAsync();

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
                    existingEntity.AccessedTime = link.AccessedTime;
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
                    existingEntity.AccessedTime = link.AccessedTime;
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
                existingEntity.AccessedTime = link.AccessedTime;
            }

            await efCoreDatahubContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cannot update navigation");
        }
    }

    public async Task<ICollection<UserRecentLink>> GetRecentLinks(string userId, int maxRecentLinks)
    {
        await using var efCoreDatahubContext = await portalContext.CreateDbContextAsync();

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
            .OrderByDescending(l => l.AccessedTime)
            .Take(maxRecentLinks)
            .ToList();
    }
}