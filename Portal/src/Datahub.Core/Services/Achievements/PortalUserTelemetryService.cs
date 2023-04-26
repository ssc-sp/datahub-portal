﻿using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Datahub;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Datahub.Core.Services.Achievements;

public class PortalUserTelemetryService : IPortalUserTelemetryService
{
    private readonly IAchievementEngineFactory _engineFactory;
    private readonly IDbContextFactory<DatahubProjectDBContext> _contextFactory;
    private readonly IUserInformationService _userInformationService;
    private readonly ILogger<PortalUserTelemetryService> _logger;

    public PortalUserTelemetryService(ILogger<PortalUserTelemetryService> logger,
        IAchievementEngineFactory engineFactory,
        IDbContextFactory<DatahubProjectDBContext> contextFactory,
        IUserInformationService userInformationService)
    {
        _logger = logger;
        _engineFactory = engineFactory;
        _contextFactory = contextFactory;
        _userInformationService = userInformationService;
    }

    public event EventHandler<AchievementsEarnedEventArgs> OnAchievementsEarned;

    public async Task LogTelemetryEvent(string eventName)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync();

        // get the logged user graph Id
        var userId = await _userInformationService.GetUserIdString();

        // retrieve the portal user
        var portalUser = await ctx.PortalUsers
            .Include(p => p.Achievements)
            .FirstOrDefaultAsync(p => p.GraphGuid == userId);

        // check the user exists
        if (portalUser is null)
        {
            _logger.LogWarning("Logging Telemetry without a Portal User. UserId: {0}", userId);
            return;
        }

        // grab engine (cached)
        var engine = _engineFactory.GetAchievementEngine();

        // collect the user current achievements
        var currentAchievements = new HashSet<string>(portalUser.Achievements.Select(a => a.AchievementId));

        // evaluate the changes
        var newAchievements = await engine.Evaluate(eventName, currentAchievements).ToListAsync();

        // add new achievements
        foreach (var id in newAchievements)
        {
            var newAchievement = new UserAchievement()
            {
                PortalUserId = portalUser.Id,
                AchievementId = id,
                Count = 1,
                UnlockedAt = DateTime.UtcNow
            };
            ctx.UserAchievements.Add(newAchievement);
        }

        ctx.TelemetryEvents.Add(new TelemetryEvent()
        {
            PortalUserId = portalUser.Id,
            EventName = eventName,
            EventDate = DateTime.UtcNow
        });

        await ctx.SaveChangesAsync();

        // report the new achievements
        if (newAchievements.Any())
        {
            OnAchievementsEarned.Invoke(this, new AchievementsEarnedEventArgs(newAchievements));
        }
    }
}
