using Datahub.Core.Model.Datahub;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Datahub.Core.Services.Achievements;

public class AchievementEngineFactory : IAchievementEngineFactory
{
    private readonly AchievementEngine _engine;

    public AchievementEngineFactory(IDbContextFactory<DatahubProjectDBContext> contextFactory)
    {
        _engine = CreateAchievementEngine(contextFactory);
    }

    public AchievementEngine GetAchievementEngine() => _engine;

    private AchievementEngine CreateAchievementEngine(IDbContextFactory<DatahubProjectDBContext> contextFactory)
    {
        using var ctx = contextFactory.CreateDbContext();
        var achievements = ctx.Achievements.ToList();
        return new(achievements);
    }
}
