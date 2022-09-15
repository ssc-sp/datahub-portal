using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Datahub.Achievements.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
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
    private const string PROJECT_URL = "/projects/ABC";
    private const string PROFILE_URL = "/profile";
    private const string FRIEND_PROFILE_URL = "/profile/ABC";
    private const string RESOURCES_URL = "/resources";
    private const string ALL_EXPLORATION_PREFIX = "EXP";

    private static readonly List<(string code, string eventName, int validValue, int invalidValue)> Metrics = new()
    {
        ("PRJ-001", DatahubUserTelemetry.TelemetryEvents.UserSentInvite, 1, 0),
        ("EXP-001", DatahubUserTelemetry.TelemetryEvents.UserLogin, 1, 0),
        ("EXP-002", STORAGE_EXPLORER_URL, 1, 0),
        ("EXP-003", DatahubUserTelemetry.TelemetryEvents.UserOpenDatabricks, 1, 0),
        ("EXP-004", RESOURCES_URL, 1, 0),
        ("EXP-005", DatahubUserTelemetry.TelemetryEvents.UserViewProjectNotMemberOf, 1, 0),
        ("EXP-006", PROFILE_URL, 1, 0),
        ("EXP-007", DatahubUserTelemetry.TelemetryEvents.UserRecentLink, 1, 0),
        ("EXP-008", DatahubUserTelemetry.TelemetryEvents.UserToggleCulture, 1, 0),
        ("EXP-009", ALL_EXPLORATION_PREFIX, 1, 0),
        ("EXP-010", PROJECT_URL, 1, 0),
        ("EXP-011", FRIEND_PROFILE_URL, 1, 0),

        ("PRJ-001", DatahubUserTelemetry.TelemetryEvents.UserSentInvite, 1, 0),
        ("PRJ-002", DatahubUserTelemetry.TelemetryEvents.UserAcceptedInvite, 1, 0),
        ("PRJ-003", DatahubUserTelemetry.TelemetryEvents.UserUploadFile, 1, 0),
        ("PRJ-004", DatahubUserTelemetry.TelemetryEvents.UserShareFile, 1, 0),
        ("PRJ-005", DatahubUserTelemetry.TelemetryEvents.UserDownloadFile, 1, 0),
        ("PRJ-006", DatahubUserTelemetry.TelemetryEvents.UserDeleteFile, 1, 0),
        ("PRJ-007", DatahubUserTelemetry.TelemetryEvents.UserCreateFolder, 1, 0),
    };

    private static Dictionary<string, (DatahubUserTelemetry, DatahubUserTelemetry)> GenerateTelemetry()
    {
        var telemetry = new Dictionary<string, (DatahubUserTelemetry, DatahubUserTelemetry)>();

        foreach (var (code, eventName, validValue, invalidValue) in Metrics)
        {
            if (eventName == ALL_EXPLORATION_PREFIX)
            {
                AddTelemetry(telemetry, code, Metrics
                    .Where(m => m.Item1.StartsWith(ALL_EXPLORATION_PREFIX))
                    .Select(m => (m.eventName, m.validValue, m.invalidValue))
                    .ToList());

                continue;
            }

            AddTelemetry(telemetry, code, new List<(string, int, int)> { (eventName, validValue, invalidValue)});
        }

        return telemetry;
    }

    private static void AddTelemetry(IDictionary<string, (DatahubUserTelemetry, DatahubUserTelemetry)> telemetry,
        string code, IReadOnlyCollection<(string eventName, int validValue, int invalidValue)> events)
    {
        if (!telemetry.ContainsKey(code))
        {
            telemetry.Add(code, (
                new DatahubUserTelemetry
                {
                    UserId = UserId,
                    EventMetrics = new List<DatahubTelemetryEventMetric>()
                },
                new DatahubUserTelemetry
                {
                    UserId = UserId,
                    EventMetrics = new List<DatahubTelemetryEventMetric>()
                }));
        }

        telemetry[code].Item1.EventMetrics.AddRange(
            events.Select(e =>
                new DatahubTelemetryEventMetric
                {
                    Name = e.eventName,
                    Value = e.validValue
                }));
        telemetry[code].Item2.EventMetrics.AddRange(
            events.Select(e =>
                new DatahubTelemetryEventMetric
                {
                    Name = e.eventName,
                    Value = e.invalidValue
                }));
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