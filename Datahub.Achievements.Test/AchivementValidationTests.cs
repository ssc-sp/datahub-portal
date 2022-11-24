using Blazored.LocalStorage;
using Datahub.Achievements.Models;
using Microsoft.EntityFrameworkCore;
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
            AchievementDirectoryPath = AchievementDirectoryPath,
            Enabled = true,
            LocalAchievementsOnly = true
        });


    private const string STORAGE_EXPLORER_URL = "/projects/ABC/filelist";
    private const string PROJECT_URL = "/projects/ABC";
    private const string PROFILE_URL = "/profile";
    private const string RESOURCES_URL = "/resources";

    private static readonly List<(string code, string eventName, int validValue, int invalidValue)> Metrics = new()
    {
        ("EXP-001", DatahubUserTelemetry.TelemetryEvents.UserLogin, 1, 0),
        ("EXP-002", STORAGE_EXPLORER_URL, 1, 0),
        ("EXP-003", DatahubUserTelemetry.TelemetryEvents.UserOpenDatabricks, 1, 0),
        ("EXP-004", RESOURCES_URL, 1, 0),
        ("EXP-005", DatahubUserTelemetry.TelemetryEvents.UserViewProjectNotMemberOf, 1, 0),
        ("EXP-006", PROFILE_URL, 1, 0),
        ("EXP-007", DatahubUserTelemetry.TelemetryEvents.UserRecentLink, 1, 0),
        ("EXP-008", DatahubUserTelemetry.TelemetryEvents.UserToggleCulture, 1, 0),
        // ("EXP-009", ALL_EXPLORATION_PREFIX, 1, 0),
        ("EXP-010", PROJECT_URL, 1, 0),
        ("EXP-011", DatahubUserTelemetry.TelemetryEvents.UserViewOtherProfile, 1, 0),
        
        // include the events for unlocking all of the 01 exploration
        ("EXP-009", "garbage", 1, 0),
        ("EXP-009", DatahubUserTelemetry.TelemetryEvents.UserLogin, 1, 0),
        ("EXP-009", STORAGE_EXPLORER_URL, 1, 0),
        ("EXP-009", DatahubUserTelemetry.TelemetryEvents.UserOpenDatabricks, 1, 0),
        ("EXP-009", RESOURCES_URL, 1, 0),
        ("EXP-009", DatahubUserTelemetry.TelemetryEvents.UserViewProjectNotMemberOf, 1, 0),
        ("EXP-009", PROFILE_URL, 1, 0),
        ("EXP-009", DatahubUserTelemetry.TelemetryEvents.UserRecentLink, 1, 0),
        ("EXP-009", DatahubUserTelemetry.TelemetryEvents.UserToggleCulture, 1, 0),
        ("EXP-009", PROJECT_URL, 1, 0),
        ("EXP-009", DatahubUserTelemetry.TelemetryEvents.UserViewOtherProfile, 1, 0),

        ("PRJ-001", DatahubUserTelemetry.TelemetryEvents.UserSentInvite, 1, 0),
        ("PRJ-002", DatahubUserTelemetry.TelemetryEvents.UserJoinedProject, 1, 0),
        ("PRJ-003", DatahubUserTelemetry.TelemetryEvents.UserUploadFile, 1, 0),
        ("PRJ-004", DatahubUserTelemetry.TelemetryEvents.UserShareFile, 1, 0),
        ("PRJ-005", DatahubUserTelemetry.TelemetryEvents.UserDownloadFile, 1, 0),
        ("PRJ-006", DatahubUserTelemetry.TelemetryEvents.UserDeleteFile, 1, 0),
        ("PRJ-007", DatahubUserTelemetry.TelemetryEvents.UserCreateFolder, 1, 0),
        
        // include the events for unlocking all of the 01 project
        ("PRJ-008", "garbage", 1, 0),
        ("PRJ-008", DatahubUserTelemetry.TelemetryEvents.UserSentInvite, 1, 0),
        ("PRJ-008", DatahubUserTelemetry.TelemetryEvents.UserJoinedProject, 1, 0),
        ("PRJ-008", DatahubUserTelemetry.TelemetryEvents.UserUploadFile, 1, 0),
        ("PRJ-008", DatahubUserTelemetry.TelemetryEvents.UserShareFile, 1, 0),
        ("PRJ-008", DatahubUserTelemetry.TelemetryEvents.UserDownloadFile, 1, 0),
        ("PRJ-008", DatahubUserTelemetry.TelemetryEvents.UserDeleteFile, 1, 0),
        ("PRJ-008", DatahubUserTelemetry.TelemetryEvents.UserCreateFolder, 1, 0),
    };


    private static IEnumerable<object[]> EarnedAchievementTelemetryParams()
    {
        var achievementFactory =
            AchievementFactory.CreateFromFilesAsync(AchievementDirectoryPath).GetAwaiter().GetResult();

        return achievementFactory.Achievements!.Select(kvp => new object[]
        {
            kvp.Key,
            Metrics
                .Where(m => m.code == kvp.Key)
                .Select(m => (m.eventName, m.validValue))
                .ToList()
        });
    }
    
    private static IEnumerable<object[]> NotEarnedAchievementTelemetryParams()
    {
        var achievementFactory =
            AchievementFactory.CreateFromFilesAsync(AchievementDirectoryPath).GetAwaiter().GetResult();

        return achievementFactory.Achievements!.Select(kvp => new object[]
        {
            kvp.Key,
            Metrics
                .Where(m => m.code == kvp.Key)
                .Select(m => (m.eventName, m.invalidValue))
                .ToList()
        });
    }

    [Test]
    [TestCaseSource(nameof(EarnedAchievementTelemetryParams))]
    [Parallelizable(ParallelScope.All)]
    public async Task EarnedIfConditionsMet(string code, List<(string, int)> telemetryEvents)
    {
        var userObject = new UserObject
        {
            UserId = UserId,
            Telemetry = new DatahubUserTelemetry(),
        };
        
        
        var mockLogger = new Mock<ILogger<AchievementService>>();
        var mockStorage = new Mock<ILocalStorageService>();
        mockStorage.Setup(s => s.GetItemAsync<UserObject>(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(userObject);

        var mockCosmosDb = new Mock<IDbContextFactory<AchievementContext>>();
        
        var mockAuth = Utils.CreateMockAuth(UserId);
        var achievementService =
            new AchievementService(mockLogger.Object, mockCosmosDb.Object, mockStorage.Object, mockAuth.Object, Options);
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

        foreach (var (eventName, value) in telemetryEvents)
        {
            await achievementService.AddOrIncrementTelemetryEvent(eventName, value);
        }
        Assert.That(passed, Is.True);
    }

    [Test]
    [TestCaseSource(nameof(NotEarnedAchievementTelemetryParams))]
    [Parallelizable(ParallelScope.All)]
    public async Task NotEarnedIfConditionsNotMet(string code, List<(string, int)> telemetryEvents)
    {
        var userObject = new UserObject
        {
            UserId = UserId,
            Telemetry = new DatahubUserTelemetry(),
        };
        
        var mockLogger = new Mock<ILogger<AchievementService>>();
        var mockStorage = new Mock<ILocalStorageService>();
        var mockCosmosDb = new Mock<IDbContextFactory<AchievementContext>>();
        var mockAuth = Utils.CreateMockAuth(UserId);
        var achievementService =
            new AchievementService(mockLogger.Object, mockCosmosDb.Object, mockStorage.Object, mockAuth.Object, Options);

        mockStorage.Setup(s => s.GetItemAsync<UserObject>(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(userObject);


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
        
        foreach (var (eventName, value) in telemetryEvents)
        {
            await achievementService.AddOrIncrementTelemetryEvent(eventName, value);
        }
    }
}