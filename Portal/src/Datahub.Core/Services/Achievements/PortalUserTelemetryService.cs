using Datahub.Core.Model.Achievements;
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
    private readonly IDatahubAuditingService _auditingService;

    public PortalUserTelemetryService(ILogger<PortalUserTelemetryService> logger,
        IAchievementEngineFactory engineFactory,
        IDbContextFactory<DatahubProjectDBContext> contextFactory,
        IUserInformationService userInformationService,
        IDatahubAuditingService auditingService)
    {
        _logger = logger;
        _engineFactory = engineFactory;
        _contextFactory = contextFactory;
        _userInformationService = userInformationService;
        _auditingService = auditingService;
    }

    public event EventHandler<AchievementsEarnedEventArgs> OnAchievementsEarned;

    public async Task LogTelemetryEvent(string eventName)
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();

        var portalUser = await _userInformationService.GetCurrentPortalUserWithAchievementsAsync();
        // check the user exists
        if (portalUser is null)
        {
            _logger.LogWarning("Logging Telemetry without a Portal User. Event: {eventName}", eventName);
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

        await ctx.TrackSaveChangesAsync(_auditingService);

        // report the new achievements
        if (newAchievements.Any())
        {
            OnAchievementsEarned?.Invoke(this, new AchievementsEarnedEventArgs(newAchievements, portalUser.UserSettings.HideAchievements));
            await _auditingService.TrackEvent("Achivements", ("Codes", string.Join(", ", newAchievements)));
        }        
    }
}
