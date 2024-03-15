﻿using Datahub.Core.Model.Achievements;

namespace Datahub.Core.Services.Achievements;

public class AchievementEngineFactory : IAchievementEngineFactory
{
    private readonly Lazy<AchievementEngine> engine;

    public AchievementEngineFactory()
    {
        engine = new(() => new(Achievement.GetAll()));
    }

    public AchievementEngine GetAchievementEngine() => engine.Value;
}
