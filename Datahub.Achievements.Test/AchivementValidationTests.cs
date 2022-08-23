using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework.Constraints;

namespace Datahub.Achievements.Test;

public class AchievementValidationTests
{
    private static readonly string AchievementDirectoryPath =
        Path.Join(Directory.GetCurrentDirectory(), "../../../../Datahub.Achievements/Achievements");
    private static readonly string UserId = Guid.NewGuid().ToString();
    
    private static readonly IOptions<AchievementServiceOptions> Options = 
        new OptionsWrapper<AchievementServiceOptions>(new AchievementServiceOptions
    {
        AchievementDirectoryPath = AchievementDirectoryPath,
        Enabled = true
    });

    private static readonly Dictionary<string, (DatahubUserTelemetry, DatahubUserTelemetry)> ParameterizedUserTelemetryDictionary = new()
    {
        // Code, Earned, Not Earned
        { "PRJ-001", (new DatahubUserTelemetry() { UserId = UserId, NumberOfUsersInvited = 1}, new DatahubUserTelemetry() { UserId = UserId, NumberOfUsersInvited = 0}) },
        
        { "EXP-001", (new DatahubUserTelemetry() { UserId = UserId, NumberOfLogins = 1}, new DatahubUserTelemetry() { UserId = UserId, NumberOfLogins = 0}) },
        { "EXP-002", (new DatahubUserTelemetry() { UserId = UserId, VisitStorageExplorer = true}, new DatahubUserTelemetry() { UserId = UserId, VisitStorageExplorer = false}) },
        { "EXP-003", (new DatahubUserTelemetry() { UserId = UserId, VisitDatabricks = true}, new DatahubUserTelemetry() { UserId = UserId, VisitDatabricks = false}) },
        { "EXP-006", (new DatahubUserTelemetry() { UserId = UserId, VisitProfile = true}, new DatahubUserTelemetry() { UserId = UserId, VisitProfile = false}) },
        
    };

    private static IEnumerable<object[]> EarnedAchievementParams()
    {
        var achievementFactory =
            AchievementFactory.CreateFromFilesAsync(AchievementDirectoryPath).GetAwaiter().GetResult();

        return achievementFactory.Achievements!.Select(kvp => new object[]
        {
            kvp.Key,
            kvp.Value,
            ParameterizedUserTelemetryDictionary[kvp.Key].Item1,
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
            ParameterizedUserTelemetryDictionary[kvp.Key].Item2,
        });
    }

    [Test]
    [TestCaseSource(nameof(EarnedAchievementParams))]
    [Parallelizable(ParallelScope.All)]
    public async Task EarnedIfConditionsMet(string code, Achievement achievement, DatahubUserTelemetry userTelemetry)
    {
        var mockLogger = new Mock<ILogger<AchievementService>>();
        var mockStorage = new Mock<ILocalStorageService>();
        var achievementService = new AchievementService(mockLogger.Object, mockStorage.Object, Options);
        var passed = false;
        
        achievementService!.AchievementEarned += (_, e) =>
        {
            Assert.Multiple(() =>
            {
                Assert.That(e.Achievement, Is.TypeOf(typeof(Achievement)));
                Assert.That(e.UserId, Is.EqualTo(UserId));
            });

            if (e.Achievement?.Code != code) return;
            passed = true;
        };
        
        await achievementService!.RunRulesEngine(userTelemetry);
        Assert.That(passed, Is.True);
    }
    
    [Test]
    [TestCaseSource(nameof(NotEarnedAchievementParams))]
    [Parallelizable(ParallelScope.All)]
    public async Task NotEarnedIfConditionsNotMet(string code, Achievement achievement, DatahubUserTelemetry userTelemetry)
    {
        var mockLogger = new Mock<ILogger<AchievementService>>();
        var mockStorage = new Mock<ILocalStorageService>();
        
        var achievementService = new AchievementService(mockLogger.Object, mockStorage.Object, Options);
        
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
