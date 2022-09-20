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
        
        var mockAuth = Utils.CreateMockAuth(UserId);
        var achievementService =
            new AchievementService(mockLogger.Object, mockCosmosDb.Object, mockStorage.Object, mockAuth.Object, LocalOptions);

        mockStorage.Setup(s =>
                s.SetItemAsync(It.IsAny<string>(), It.IsAny<UserObject>(), CancellationToken.None))
            .Callback<string, UserObject, CancellationToken?>((_, _, _) =>
            {
                Assert.Pass();
            });
        
        mockCosmosDb.Setup(s => s.CreateDbContextAsync(CancellationToken.None))
            .Callback<CancellationToken?>(_ =>
            {
                Assert.Fail();
            });
        


        await achievementService.GetUserAchievements();
    }

    [Test]
    public async Task NonLocalOptionsHitCosmosDbStorageTest()
    {
        var mockLogger = new Mock<ILogger<AchievementService>>();
        var mockStorage = new Mock<ILocalStorageService>();
        mockStorage.Setup(s =>
                s.SetItemAsync(It.IsAny<string>(), It.IsAny<UserObject>(), CancellationToken.None))
            .Callback<string, UserObject, CancellationToken?>((_, _, _) =>
            {
                Assert.Fail();
            });
        
        var mockAuth = Utils.CreateMockAuth(UserId);

        var optionsBuilder =
            new DbContextOptionsBuilder<AchievementContext>().UseInMemoryDatabase("NonLocalOptionsHitCosmosDbStorageTest");
        var achievementContext = new AchievementContext(optionsBuilder.Options);
        await achievementContext.UserObjects!.AddAsync(new UserObject
        {
            UserId = UserId,
            Telemetry = new DatahubUserTelemetry { UserId = UserId },
            UserAchievements = new List<UserAchievement>()
        });
        await achievementContext.SaveChangesAsync();
        
        var mockCosmosDb = new Mock<IDbContextFactory<AchievementContext>>();
        mockCosmosDb.Setup(s => s.CreateDbContextAsync(CancellationToken.None))
            .Callback<CancellationToken?>(_ =>
            {
                Assert.Pass();
            });
        
        var achievementService =
            new AchievementService(mockLogger.Object, mockCosmosDb.Object, mockStorage.Object, mockAuth.Object, NotLocalOptions);

        await achievementService.GetUserAchievements();
    }


}