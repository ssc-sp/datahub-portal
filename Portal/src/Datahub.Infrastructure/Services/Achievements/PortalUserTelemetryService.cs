using Datahub.Application.Services;
using Datahub.Application.Services.Achievements;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services.Achievements;

public class PortalUserTelemetryService : IPortalUserTelemetryService
{
    private readonly IAchievementEngineFactory engineFactory;
    private readonly IDbContextFactory<DatahubProjectDBContext> contextFactory;
    private readonly IUserInformationService userInformationService;
    private readonly ILogger<PortalUserTelemetryService> logger;
    private readonly IDatahubAuditingService auditingService;

    public PortalUserTelemetryService(
        ILogger<PortalUserTelemetryService> logger,
        IAchievementEngineFactory engineFactory,
        IDbContextFactory<DatahubProjectDBContext> contextFactory,
        IUserInformationService userInformationService,
        IDatahubAuditingService auditingService)
    {
        this.logger = logger;
        this.engineFactory = engineFactory;
        this.contextFactory = contextFactory;
        this.userInformationService = userInformationService;
        this.auditingService = auditingService;
    }

    public event EventHandler<AchievementsEarnedEventArgs> OnAchievementsEarned;

    public async Task LogTelemetryEvent(string eventName)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        var portalUser = await userInformationService.GetCurrentPortalUserWithAchievementsAsync();
        // check the user exists
        if (portalUser is null)
        {
            logger.LogWarning("Logging Telemetry without a Portal User. Event: {eventName}", eventName);
            return;
        }

        // grab engine (cached)
        var engine = engineFactory.GetAchievementEngine();

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
            OnAchievementsEarned?.Invoke(this, new AchievementsEarnedEventArgs(newAchievements, portalUser.UserSettings.HideAchievements));
            await auditingService.TrackEvent("Achivements", ("Codes", string.Join(", ", newAchievements)));
        }
    }
}
