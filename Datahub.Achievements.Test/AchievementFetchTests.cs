using Blazored.LocalStorage;
using Datahub.Achievements.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Datahub.Achievements.Test;

public class AchievementFetchTests
{
    private static readonly string AchievementDirectoryPath =
        Path.Join(Directory.GetCurrentDirectory(), "../../../../Datahub.Achievements/Achievements");

    private static readonly string UserId = Guid.NewGuid().ToString();

    private static readonly IOptions<AchievementServiceOptions> Options =
        new OptionsWrapper<AchievementServiceOptions>(new AchievementServiceOptions
        {
            AchievementDirectoryPath = AchievementDirectoryPath,
            LocalAchievementsOnly = true,
            Enabled = true
        });

    private const string NotEarnedAchievementCode = "not-earned";
    private const string EarnedAchievementCode1 = "earned1";
    private const string EarnedAchievementCode2 = "earned2";

    private static readonly UserObject UserObject = new()
    {
        Telemetry = new DatahubUserTelemetry()
        {
            UserId = UserId
        },
        UserAchievements = new List<UserAchievement>
        {
            new()
            {
                UserId = UserId,
                Achievement = new Achievement
                {
                    Name = "Earned 1",
                    Code = EarnedAchievementCode1
                },
                Date = DateTime.Now.Subtract(TimeSpan.FromDays(1)),
            },
            new()
            {
                UserId = UserId,
                Achievement = new Achievement
                {
                    Name = "Earned 2",
                    Code = EarnedAchievementCode2,
                },
                Date = DateTime.Now.Subtract(TimeSpan.FromDays(3)),
            },
            new()
            {
                UserId = UserId,
                Achievement = new Achievement
                {
                    Name = "Not Earned",
                    Code = NotEarnedAchievementCode
                },
                Date = DateTime.Now.Subtract(TimeSpan.FromDays(1)),
            }
        }
    };

    [Test]
    public async Task GetUsersAchievements()
    {
        var mockLogger = new Mock<ILogger<AchievementService>>();
        var mockStorage = new Mock<ILocalStorageService>();
        var mockCosmosDb = new Mock<IDbContextFactory<AchievementContext>>();

        mockStorage.Setup(s => s.GetItemAsync<UserObject>(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(UserObject);

        var mockAuth = Utils.CreateMockAuth(UserId);
        var achievementService =
            new AchievementService(mockLogger.Object, mockCosmosDb.Object, mockStorage.Object, mockAuth.Object, Options);

        var result = await achievementService.GetUserAchievements();

        Assert.That(result, Is.TypeOf<List<UserAchievement>>());
        Assert.That(result, Has.Count.EqualTo(UserObject.UserAchievements.Count));
    }

    [Test]
    public async Task GetEarnedUsersAchievements()
    {
        var mockLogger = new Mock<ILogger<AchievementService>>();
        var mockStorage = new Mock<ILocalStorageService>();
        var mockCosmosDb = new Mock<IDbContextFactory<AchievementContext>>();

        mockStorage.Setup(s => s.GetItemAsync<UserObject>(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(UserObject);

        var mockAuth = Utils.CreateMockAuth(UserId);
        var achievementService =
            new AchievementService(mockLogger.Object, mockCosmosDb.Object, mockStorage.Object, mockAuth.Object, Options);

        var result = await achievementService.GetUserAchievements();

        Assert.That(result, Is.TypeOf<List<UserAchievement>>());
        Assert.That(result.Count(r => r.Earned),
            Is.EqualTo(UserObject.UserAchievements.Count(r => r.Earned)));
    }
}