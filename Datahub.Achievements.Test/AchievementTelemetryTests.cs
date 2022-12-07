using Blazored.LocalStorage;
using Datahub.Achievements.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Datahub.Achievements.Test;

public class AchievementTelemetryTests
{
    private static readonly string AchievementDirectoryPath =
        Path.Join(Directory.GetCurrentDirectory(), "../../../Achievements");

    private static readonly string UserId = Guid.NewGuid().ToString();

    private static readonly IOptions<AchievementServiceOptions> Options =
        new OptionsWrapper<AchievementServiceOptions>(new AchievementServiceOptions
        {
            AchievementDirectoryPath = AchievementDirectoryPath,
            LocalAchievementsOnly = true,
            Enabled = true
        });

    [Test]
    public async Task CanAddTelemetryEvent()
    {
        var mockLogger = new Mock<ILogger<AchievementService>>();
        var mockStorage = new Mock<ILocalStorageService>();
        var mockCosmosDb = new Mock<IDbContextFactory<AchievementContext>>();

        const string eventName = "test";
        const int initialValue = 0;
        var mockAuth = Utils.CreateMockAuth(UserId);
        var achievementService =
            new AchievementService(mockLogger.Object, mockCosmosDb.Object, mockStorage.Object, mockAuth.Object, Options);

        mockStorage.Setup(s => s.GetItemAsync<UserObject>(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(new UserObject() { UserId = UserId });

        var result = await achievementService.AddOrIncrementTelemetryEvent(eventName, initialValue);
        Assert.That(result, Is.TypeOf<int>());
        Assert.That(result, Is.EqualTo(initialValue));
    }

    [Test]
    public async Task CanIncrementTelemetryEvent()
    {
        var mockLogger = new Mock<ILogger<AchievementService>>();
        var mockStorage = new Mock<ILocalStorageService>();
        var mockCosmosDb = new Mock<IDbContextFactory<AchievementContext>>();

        const string eventName = "test";
        var mockAuth = Utils.CreateMockAuth(UserId);
        var achievementService =
            new AchievementService(mockLogger.Object, mockCosmosDb.Object, mockStorage.Object, mockAuth.Object, Options);

        mockStorage.Setup(s => s.GetItemAsync<UserObject>(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(new UserObject() { UserId = UserId });


        var expectedSum = 0;
        for (var i = 1; i < 100; i += i)
        {
            expectedSum += i + i;
            var result = await achievementService.AddOrIncrementTelemetryEvent(eventName, i + i);
            Assert.That(result, Is.TypeOf<int>());
            Assert.That(result, Is.EqualTo(expectedSum));
        }
    }
    
    [Test]
    public async Task CanSetTelemetryEvent()
    {
        var mockLogger = new Mock<ILogger<AchievementService>>();
        var mockStorage = new Mock<ILocalStorageService>();
        var mockCosmosDb = new Mock<IDbContextFactory<AchievementContext>>();

        const string eventName = "test";
        const int initialValue = 21;
        var mockAuth = Utils.CreateMockAuth(UserId);
        var achievementService =
            new AchievementService(mockLogger.Object, mockCosmosDb.Object, mockStorage.Object, mockAuth.Object, Options);

        mockStorage.Setup(s => s.GetItemAsync<UserObject>(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(new UserObject() { UserId = UserId });

        var result = await achievementService.AddOrSetTelemetryEvent(eventName, initialValue);
        Assert.That(result, Is.TypeOf<int>());
        Assert.That(result, Is.EqualTo(initialValue));
        
        result = await achievementService.AddOrSetTelemetryEvent(eventName, initialValue * 2);
        Assert.That(result, Is.TypeOf<int>());
        Assert.That(result, Is.EqualTo(initialValue * 2));
        
        result = await achievementService.AddOrSetTelemetryEvent(eventName, initialValue);
        Assert.That(result, Is.TypeOf<int>());
        Assert.That(result, Is.EqualTo(initialValue));
    }
    
    [Test]
    public async Task CanSetButKeepMaxTelemetryEvent()
    {
        var mockLogger = new Mock<ILogger<AchievementService>>();
        var mockStorage = new Mock<ILocalStorageService>();
        var mockCosmosDb = new Mock<IDbContextFactory<AchievementContext>>();

        const string eventName = "test";
        const int initialValue = 21;
        var mockAuth = Utils.CreateMockAuth(UserId);
        var achievementService =
            new AchievementService(mockLogger.Object, mockCosmosDb.Object, mockStorage.Object, mockAuth.Object, Options);

        mockStorage.Setup(s => s.GetItemAsync<UserObject>(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(new UserObject() { UserId = UserId });

        var result = await achievementService.AddOrSetTelemetryEventKeepMax(eventName, initialValue);
        Assert.That(result, Is.TypeOf<int>());
        Assert.That(result, Is.EqualTo(initialValue));
        
        result = await achievementService.AddOrSetTelemetryEventKeepMax(eventName, initialValue * 2);
        Assert.That(result, Is.TypeOf<int>());
        Assert.That(result, Is.EqualTo(initialValue * 2));
        
        result = await achievementService.AddOrSetTelemetryEventKeepMax(eventName, initialValue);
        Assert.That(result, Is.TypeOf<int>());
        Assert.That(result, Is.EqualTo(initialValue * 2));
    }

    [Test]
    public async Task RunsRulesEngineAfterTelemetryEvent()
    {
        var mockLogger = new Mock<ILogger<AchievementService>>();
        var mockStorage = new Mock<ILocalStorageService>();
        var mockCosmosDb = new Mock<IDbContextFactory<AchievementContext>>();

        mockStorage.Setup(s => s.GetItemAsync<UserObject>(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(new UserObject { UserId = UserId });

        var mockAuth = Utils.CreateMockAuth(UserId);
        var achievementService =
            new AchievementService(mockLogger.Object, mockCosmosDb.Object, mockStorage.Object, mockAuth.Object, Options);
        
        var passed = false;

        const string code = "TST-003";
        const string eventName = "telemetry_test";
        const int value = 1;

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

        var result = await achievementService.AddOrIncrementTelemetryEvent(eventName, value);
        Assert.That(result, Is.TypeOf<int>());
        Assert.That(result, Is.EqualTo(value));
        Assert.That(passed, Is.True);
    }
}