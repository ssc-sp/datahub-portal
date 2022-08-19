using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Datahub.Achievements.Test;

public class AchievementValidationTests
{
    private static readonly string AchievementDirectoryPath =
        Path.Join(Directory.GetCurrentDirectory(), "../../../../Datahub.Achievements/Achievements");
    private static readonly string UserId = Guid.NewGuid().ToString();
    
    private static readonly IOptions<AchievementServiceOptions> Options = 
        new OptionsWrapper<AchievementServiceOptions>(new AchievementServiceOptions
    {
        AchievementDirectoryPath = AchievementDirectoryPath
    });

    private static readonly Dictionary<string, (DatahubUserTelemetry, DatahubUserTelemetry)> EarnedUserTelemetryDictionary = new()
    {
        { "EXP-006", (new DatahubUserTelemetry() { UserId = UserId, ViewedTheirProfile = true}, new DatahubUserTelemetry() { UserId = UserId, ViewedTheirProfile = false}) }
    };

    private static IEnumerable<object[]> EarnedAchievementParams()
    {
        var achievementFactory =
            AchievementFactory.CreateFromFilesAsync(AchievementDirectoryPath).GetAwaiter().GetResult();

        return achievementFactory.Achievements!.Select(kvp => new object[]
        {
            kvp.Key,
            kvp.Value,
            EarnedUserTelemetryDictionary[kvp.Key].Item1,
        });
    }
    
    private static IEnumerable<object[]> NotEarnedAchievementParams()
    {
        var achievementFactory =
            AchievementFactory.CreateFromFilesAsync(AchievementDirectoryPath).GetAwaiter().GetResult();

        return achievementFactory.Achievements!.Select(kvp => new object[]
        {
            kvp.Key,
            kvp.Value,
            EarnedUserTelemetryDictionary[kvp.Key].Item2,
        });
    }

    [Test]
    [TestCaseSource(nameof(EarnedAchievementParams))]
    [Parallelizable(ParallelScope.None)]
    public async Task EarnedIfConditionsMet(string code, Achievement achievement, DatahubUserTelemetry userTelemetry)
    {
        var mockLogger = new Mock<ILogger<AchievementService>>();
        var achievementService = new AchievementService(mockLogger.Object, Options);
        achievementService!.AchievementEarned += (_, e) =>
        {
            Assert.Multiple(() =>
            {
                Assert.That(e.Achievement, Is.TypeOf(typeof(Achievement)));
                Assert.That(e.UserId, Is.EqualTo(UserId));
            });

            if (e.Achievement?.Code != code) return;
            
            Assert.Pass();
        };
        
        await achievementService!.RunRulesEngine(userTelemetry);
    }
    
    [Test]
    [TestCaseSource(nameof(NotEarnedAchievementParams))]
    [Parallelizable(ParallelScope.None)]
    public async Task NotEarnedIfConditionsNotMet(string code, Achievement achievement, DatahubUserTelemetry userTelemetry)
    {
        var mockLogger = new Mock<ILogger<AchievementService>>();
        var achievementService = new AchievementService(mockLogger.Object, Options);
        achievementService!.AchievementEarned += (_, e) =>
        {
            Assert.Multiple(() =>
            {
                Assert.That(e.Achievement, Is.TypeOf(typeof(Achievement)));
                Assert.That(e.UserId, Is.EqualTo(UserId));
            });

            if (e.Achievement?.Code != code) return;
            
            Assert.Fail();
        };

        await achievementService!.RunRulesEngine(userTelemetry);
    }
}
