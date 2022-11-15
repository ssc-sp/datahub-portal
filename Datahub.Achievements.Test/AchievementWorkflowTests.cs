using Blazored.LocalStorage;
using Datahub.Achievements.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Datahub.Achievements.Test;

public class AchievementWorkflowTests
{
    private static readonly string AchievementDirectoryPath =
        Path.Join(Directory.GetCurrentDirectory(), "../../../Achievements");

    private static readonly IOptions<AchievementServiceOptions> Options =
        new OptionsWrapper<AchievementServiceOptions>(new AchievementServiceOptions
        {
            AchievementDirectoryPath = AchievementDirectoryPath,
            LocalAchievementsOnly = true,
            Enabled = true
        });

    [Test]
    public void SanityCheck()
    {
        Assert.Pass();
    }

    [Test]
    public async Task CanRunRulesEngineWithAchievementWorkflow()
    {
        var userId = Guid.NewGuid().ToString();

        var mockLogger = new Mock<ILogger<AchievementService>>();
        var mockStorage = new Mock<ILocalStorageService>();
        var mockCosmosDb = new Mock<IDbContextFactory<AchievementContext>>();
        var mockAuth = Utils.CreateMockAuth(userId);
        var achievementService =
            new AchievementService(mockLogger.Object, mockCosmosDb.Object, mockStorage.Object, mockAuth.Object, Options);

        var userObject = new UserObject
        {
            UserId = userId,
            Telemetry = new DatahubUserTelemetry(),
        };
        mockStorage.Setup(s => s.GetItemAsync<UserObject>(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(userObject);

        await achievementService.AddOrIncrementTelemetryEvent("test", 1);

        var result = await achievementService.RunRulesEngine(userObject);
        Assert.That(result, Is.TypeOf(typeof(bool)));
    }

    [Test]
    public async Task TriggersAchievementEvent()
    {
        var userId = Guid.NewGuid().ToString();

        var mockLogger = new Mock<ILogger<AchievementService>>();
        var mockStorage = new Mock<ILocalStorageService>();
        var mockCosmosDb = new Mock<IDbContextFactory<AchievementContext>>();
        var mockAuth = Utils.CreateMockAuth(userId);
        var achievementService =
            new AchievementService(mockLogger.Object, mockCosmosDb.Object, mockStorage.Object, mockAuth.Object, Options);

        mockStorage.Setup(s => s.GetItemAsync<UserObject>(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(new UserObject
            {
                UserId = userId,
                Telemetry = new DatahubUserTelemetry()
                {
                    UserId = userId
                }
            });

        var triggeredEvent = false;
        
        achievementService.AchievementEarned += (_, e) =>
        {
            Assert.Multiple(() =>
            {
                Assert.That(e.Achievement, Is.TypeOf(typeof(Achievement)));
                Assert.That(e.UserId, Is.EqualTo(userId));
            });

            triggeredEvent = true;
        };

        var result = await achievementService.AddOrIncrementTelemetryEvent("test", 1);
        
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf(typeof(int)));
            Assert.That(triggeredEvent, Is.True);
        });
    }
}