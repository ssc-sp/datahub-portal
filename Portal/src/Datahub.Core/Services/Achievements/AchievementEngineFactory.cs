using Datahub.Core.Model.Achievements;

namespace Datahub.Core.Services.Achievements;

public class AchievementEngineFactory : IAchievementEngineFactory
{
    private readonly Lazy<AchievementEngine> _engine;

    public AchievementEngineFactory()
    {
        _engine = new(() => new(Achievement.GetAll()));
    }

    public AchievementEngine GetAchievementEngine() => _engine.Value;
}
