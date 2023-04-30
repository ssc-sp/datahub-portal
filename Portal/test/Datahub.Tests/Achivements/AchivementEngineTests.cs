﻿using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Achievements.Configuration;
using Datahub.Core.Services.Achievements;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Datahub.Tests.Achivements;

public class AchivementEngineTests
{
    private readonly AchievementEngine _achivementEngine;

    public AchivementEngineTests()
    {
        _achivementEngine = new(Achievement.GetAll());
    }

    [Fact]
    public async Task EvaluateAchivements_WithNoPrevAchivements_ReturnsExpectedNewAchivement()
    {
        var emptySet = new HashSet<string>();

        var result = await _achivementEngine.Evaluate(TelemetryEvents.UserLogin, emptySet).ToListAsync();
        
        Assert.NotNull(result);
        Assert.Contains("DHA-001", result);
    }

    [Fact]
    public async Task EvaluateAchivements_WithExistingAchivements_ReturnsNone()
    {
        var achivements = new HashSet<string>() { "DHA-001", "STR-002" };

        var result = await _achivementEngine.Evaluate(TelemetryEvents.UserLogin, achivements).ToListAsync();

        Assert.NotNull(result);
        Assert.Empty(result);

        result = await _achivementEngine.Evaluate(TelemetryEvents.UserShareFile, achivements).ToListAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task EvaluateAchivements_GivenMissingChildren_ReturnsParentAchivement()
    {
        var achivements = new HashSet<string>() { "STR-001", "STR-002", "STR-003", "STR-004", "STR-005" };

        var result = await _achivementEngine.Evaluate(TelemetryEvents.UserDeletedFolder, achivements).ToListAsync();

        Assert.NotNull(result);
        Assert.Contains("STR-006", result);
        Assert.Contains("STR-000", result);
    }
}
