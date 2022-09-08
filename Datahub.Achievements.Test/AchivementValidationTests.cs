using Blazored.LocalStorage;
using Datahub.Achievements.Models;
using Microsoft.EntityFrameworkCore;
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
            Enabled = true,
            LocalAchievementsOnly = true
        });


    private const string STORAGE_EXPLORER_URL = "/projects/ABC/filelist";
    private const string DATABRICKS_URL = "/projects/ABC/databricks";
    private const string PROFILE_URL = "/profile";
    private const string RESOURCES_URL = "/resources";

    private static Dictionary<string, (DatahubUserTelemetry, DatahubUserTelemetry)> GenerateTelemetry()
    {
        var telemetry = new Dictionary<string, (DatahubUserTelemetry, DatahubUserTelemetry)>();

        foreach (var (code, eventName, validValue, invalidValue) in new List<(string, string, int, int)>
                 {
                     ("PRJ-001", DatahubUserTelemetry.TelemetryEvents.UserSentInvite, 1, 0),
                     ("EXP-001", DatahubUserTelemetry.TelemetryEvents.UserLogin, 1, 0),
                     ("EXP-002", STORAGE_EXPLORER_URL, 1, 0),
                     ("EXP-003", DATABRICKS_URL, 1, 0),
                     ("EXP-004", RESOURCES_URL, 1, 0),
                     // ("EXP-005", DatahubUserTelemetry.TelemetryEvents.UserLogin, 1, 0),
                     ("EXP-006", PROFILE_URL, 1, 0),
                 })
        {
            telemetry.Add(code, (
                new DatahubUserTelemetry
                {
                    UserId = UserId,
                    EventMetrics = new List<DatahubTelemetryEventMetric>
                    {
                        new()
                        {
                            Name = eventName,
                            Value = validValue
                        }
                    }
                },
                new DatahubUserTelemetry
                {
                    UserId = UserId,
                    EventMetrics = new List<DatahubTelemetryEventMetric>
                    {
                        new()
                        {
                            Name = eventName, Value = invalidValue
                        }
                    }
                }));
        }

        return telemetry;
    }

    private static IEnumerable<object[]> EarnedAchievementParams()
    {
        var achievementFactory =
            AchievementFactory.CreateFromFilesAsync(AchievementDirectoryPath).GetAwaiter().GetResult();

        var parameterizedUserTelemetryDictionary = GenerateTelemetry();

        return achievementFactory.Achievements!.Select(kvp => new object[]
        {
            kvp.Key,
            kvp.Value,
            parameterizedUserTelemetryDictionary[kvp.Key].Item1,
        });
    }

    private static IEnumerable<object[]> NotEarnedAchievementParams()
    {
        var achievementFactory =
            AchievementFactory.CreateFromFilesAsync(AchievementDirectoryPath).GetAwaiter().GetResult();

        var parameterizedUserTelemetryDictionary = GenerateTelemetry();

        return achievementFactory.Achievements!.Select(kvp => new object[]
        {
            kvp.Key,
            kvp.Value,
            parameterizedUserTelemetryDictionary[kvp.Key].Item2,
        });
    }

    [Test]
    [TestCaseSource(nameof(EarnedAchievementParams))]
    [Parallelizable(ParallelScope.All)]
    public async Task EarnedIfConditionsMet(string code, Achievement achievement, DatahubUserTelemetry userTelemetry)
    {
        var mockLogger = new Mock<ILogger<AchievementService>>();
        var mockStorage = new Mock<ILocalStorageService>();
        mockStorage.Setup(s => s.GetItemAsync<UserObject>(It.IsAny<string>(), null))
            .ReturnsAsync(new UserObject
            {
                UserId = UserId,
                Telemetry = userTelemetry,
            });

        var mockCosmosDb = new Mock<IDbContextFactory<AchievementContext>>();
        var achievementService =
            new AchievementService(mockLogger.Object, mockCosmosDb.Object, mockStorage.Object, Options);
        await achievementService.InitializeAchievementServiceForUser(UserId);
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

        await achievementService.RunRulesEngine();
        Assert.That(passed, Is.True);
    }

    [Test]
    [TestCaseSource(nameof(NotEarnedAchievementParams))]
    [Parallelizable(ParallelScope.All)]
    public async Task NotEarnedIfConditionsNotMet(string code, Achievement achievement,
        DatahubUserTelemetry userTelemetry)
    {
        var mockLogger = new Mock<ILogger<AchievementService>>();
        var mockStorage = new Mock<ILocalStorageService>();
        var mockCosmosDb = new Mock<IDbContextFactory<AchievementContext>>();
        var achievementService =
            new AchievementService(mockLogger.Object, mockCosmosDb.Object, mockStorage.Object, Options);
        await achievementService.InitializeAchievementServiceForUser(UserId);

        mockStorage.Setup(s => s.GetItemAsync<UserObject>(It.IsAny<string>(), null))
            .ReturnsAsync(new UserObject
            {
                UserId = UserId,
                Telemetry = new DatahubUserTelemetry()
            });


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

        await achievementService!.RunRulesEngine();
    }
}