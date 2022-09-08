using System.Linq.Expressions;
using Blazored.LocalStorage;
using Datahub.Achievements.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Datahub.Achievements.Test;

public class AchievementSaveTests
{
    private static readonly string AchievementDirectoryPath =
        Path.Join(Directory.GetCurrentDirectory(), "../../../Achievements");

    private static readonly string UserId = Guid.NewGuid().ToString();

    private static readonly IOptions<AchievementServiceOptions> LocalOptions =
        new OptionsWrapper<AchievementServiceOptions>(new AchievementServiceOptions
        {
            AchievementDirectoryPath = AchievementDirectoryPath,
            LocalAchievementsOnly = true,
            Enabled = true
        });
    
    private static readonly IOptions<AchievementServiceOptions> NotLocalOptions =
        new OptionsWrapper<AchievementServiceOptions>(new AchievementServiceOptions
        {
            AchievementDirectoryPath = AchievementDirectoryPath,
            LocalAchievementsOnly = false,
            Enabled = true
        });

    [Test]
    public async Task LocalOptionsHitLocalStorageTest()
    {
        var mockLogger = new Mock<ILogger<AchievementService>>();
        var mockStorage = new Mock<ILocalStorageService>();
        var mockCosmosDb = new Mock<IDbContextFactory<AchievementContext>>();

        var optionsBuilder =
            new DbContextOptionsBuilder<AchievementContext>().UseInMemoryDatabase("LocalOptionsHitLocalStorageTest");
        var achievementContext = new AchievementContext(optionsBuilder.Options);

        var achievementService =
            new AchievementService(mockLogger.Object, mockCosmosDb.Object, mockStorage.Object, LocalOptions);
        await achievementService.InitializeAchievementServiceForUser(UserId);

        const string eventName = "right";
        const string incorrectEventName = "wrong";

        mockStorage.Setup(s => s.GetItemAsync<UserObject>(It.IsAny<string>(), null))
            .ReturnsAsync(new UserObject
            {
                UserId = UserId,
                Telemetry = new DatahubUserTelemetry { UserId = UserId },
                UserAchievements = new List<UserAchievement>
                {
                    new() { Achievement = new Achievement { Code = eventName } }
                }
            });

        mockCosmosDb.Setup(s => s.CreateDbContextAsync(CancellationToken.None))
            .ReturnsAsync(achievementContext);

        await achievementContext.UserObjects!.AddAsync(new UserObject
        {
            UserId = UserId,
            Telemetry = new DatahubUserTelemetry { UserId = UserId },
            UserAchievements = new List<UserAchievement>
            {
                new() { Achievement = new Achievement { Code = incorrectEventName } }
            }
        });
        await achievementContext.SaveChangesAsync();

        var userAchievements = await achievementService.GetUserAchievements();
        Assert.That(userAchievements, Is.Not.Empty);
        Assert.That(userAchievements.All(u => u.Code == eventName), Is.True);
    }

    [Test]
    public async Task NonLocalOptionsHitCosmosDbStorageTest()
    {
        const string eventName = "right";
        const string incorrectEventName = "wrong";
        
        var mockLogger = new Mock<ILogger<AchievementService>>();
        var mockStorage = new Mock<ILocalStorageService>();
        mockStorage.Setup(s => s.GetItemAsync<UserObject>(It.IsAny<string>(), null))
            .ReturnsAsync(new UserObject
            {
                UserId = UserId,
                Telemetry = new DatahubUserTelemetry { UserId = UserId },
                UserAchievements = new List<UserAchievement>
                {
                    new() { Achievement = new Achievement { Code = incorrectEventName } }
                }
            });

        var optionsBuilder =
            new DbContextOptionsBuilder<AchievementContext>().UseInMemoryDatabase("NonLocalOptionsHitCosmosDbStorageTest");
        var achievementContext = new AchievementContext(optionsBuilder.Options);
        await achievementContext.UserObjects!.AddAsync(new UserObject
        {
            UserId = UserId,
            Telemetry = new DatahubUserTelemetry { UserId = UserId },
            UserAchievements = new List<UserAchievement>
            {
                new() { Achievement = new Achievement { Code = eventName } }
            }
        });
        await achievementContext.SaveChangesAsync();
        
        var mockCosmosDb = new Mock<IDbContextFactory<AchievementContext>>();
        mockCosmosDb.Setup(s => s.CreateDbContextAsync(CancellationToken.None))
            .ReturnsAsync(() => new AchievementContext(optionsBuilder.Options));
        
        var achievementService =
            new AchievementService(mockLogger.Object, mockCosmosDb.Object, mockStorage.Object, NotLocalOptions);
        await achievementService.InitializeAchievementServiceForUser(UserId);

        var userAchievements = await achievementService.GetUserAchievements();
        Assert.That(userAchievements, Is.Not.Empty);
        Assert.That(userAchievements.All(u => u.Code == eventName), Is.True);
    }
}