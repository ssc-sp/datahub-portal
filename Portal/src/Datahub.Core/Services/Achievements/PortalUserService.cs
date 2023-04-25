using Datahub.Core.Model.Datahub;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Linq;
using System.Threading.Tasks;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Services.UserManagement;

namespace Datahub.Core.Services.Achievements;

public class PortalUserService : IPortalUserService
{
    private readonly IUserInformationService _userInformationService;
    private readonly IDbContextFactory<DatahubProjectDBContext> _contextFactory;
    private readonly CultureService _cultureService;

    public PortalUserService(IUserInformationService userInformationService,
        IDbContextFactory<DatahubProjectDBContext> contextFactory,
        CultureService cultureService)
    {
        _userInformationService = userInformationService;
        _contextFactory = contextFactory;
        _cultureService = cultureService;
    }

    public async Task<List<string>> GetUserAchivements()
    {
        using var ctx = await GetContext();

        var portalUser = await GetPortalUser(ctx);
        if (portalUser is null)
            return new();

        return portalUser.Achievements.Select(a => a.AchievementId).ToList();
    }

    public async Task AddUserAchivements(IEnumerable<string> achivements)
    {
        using var ctx = await GetContext();

        var portalUser = await GetPortalUser(ctx) ?? await CreatePortalUser(ctx);

        foreach (var id in achivements)
        {
            UserAchievement achiv = new()
            {
                PortalUserId = portalUser.Id,
                AchievementId = id
            };
            ctx.UserAchievements.Add(achiv);
        }

        await ctx.SaveChangesAsync();
    }

    private async Task<DatahubProjectDBContext> GetContext() => await _contextFactory.CreateDbContextAsync();

    private async Task<PortalUser> GetPortalUser(DatahubProjectDBContext ctx)
    {
        var graphId = await _userInformationService.GetUserIdString();
        return await ctx.PortalUsers
            .Include(u => u.Achievements)
            .Where(u => u.GraphGuid == graphId)
            .FirstOrDefaultAsync();
    }

    private async Task<PortalUser> CreatePortalUser(DatahubProjectDBContext ctx)
    {
        var graphId = await _userInformationService.GetUserIdString();
        var email = await _userInformationService.GetUserEmail();
        var displayName = await _userInformationService.GetDisplayName();

        PortalUser portalUser = new()
        {
            GraphGuid = graphId,
            Language = _cultureService.Culture,
            Email = email,
            DisplayName = displayName            
        };

        ctx.PortalUsers.Add(portalUser);
        await ctx.SaveChangesAsync();

        return portalUser;
    }
}