using Blazored.LocalStorage;
using Datahub.Achievements.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Datahub.Achievements.Test;

public class AchievementUserObjectTests
{
    private static readonly string AchievementDirectoryPath =
        Path.Join(Directory.GetCurrentDirectory(), "../../../Achievements");

    private static readonly string UserId = Guid.NewGuid().ToString();

    
    private static readonly IOptions<AchievementServiceOptions> NotLocalOptions =
        new OptionsWrapper<AchievementServiceOptions>(new AchievementServiceOptions
        {
            AchievementDirectoryPath = AchievementDirectoryPath,
            LocalAchievementsOnly = false,
            Enabled = true
        });

    [Test]
    public async Task UserObjectSaveTest()
    {
       var mockLogger = new Mock<ILogger<AchievementService>>();
       var mockStorage = new Mock<ILocalStorageService>();
       mockStorage.Setup(s =>
               s.SetItemAsync(It.IsAny<string>(), It.IsAny<UserObject>(), CancellationToken.None))
           .Callback<string, UserObject, CancellationToken?>((_, _, _) =>
           {
               Assert.Fail();
           });

       var optionsBuilder =
           new DbContextOptionsBuilder<AchievementContext>().UseInMemoryDatabase("UserObjectSaveTest");
       
       var mockCosmosDb = new Mock<IDbContextFactory<AchievementContext>>();
       mockCosmosDb.Setup(s => s.CreateDbContextAsync(CancellationToken.None))
           .ReturnsAsync(() => new AchievementContext(optionsBuilder.Options));

       var mockAuth = Utils.CreateMockAuth(UserId);
       var achievementService =
           new AchievementService(mockLogger.Object, mockCosmosDb.Object, mockStorage.Object, mockAuth.Object, NotLocalOptions);

       await achievementService.AddOrIncrementTelemetryEvent("user_object_save_test", 1);
       
       var achievementFactory = await AchievementFactory.CreateFromFilesAsync(NotLocalOptions.Value.AchievementDirectoryPath);
       var result = await achievementService.GetUserAchievements();
       
       Assert.That(result, Has.Count.EqualTo(achievementFactory.Achievements!.Count));
       Assert.That(result.Where(a => a.Earned && a.Code == "TST-004").ToList(), Has.Count.EqualTo(1));
    }
}