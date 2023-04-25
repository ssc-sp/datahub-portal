using Datahub.Achievements.Models;
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
        _achivementEngine = new(AchivementConfiguration.GetAchievements());
    }

    // todo: fix seeding...

    [Fact]
    public async Task TestBasicAchievements()
    {
        HashSet<string> emptySet = new();

        var result = await _achivementEngine.Evaluate(TelemetryEvents.UserLogin, emptySet).ToListAsync();
        
        Assert.NotNull(result);
        Assert.Contains("DHA-001", result);
    }

}
