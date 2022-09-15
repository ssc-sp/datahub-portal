using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Datahub.Achievements.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RulesEngine.Models;

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
        var achievementService =
            new AchievementService(mockLogger.Object, mockCosmosDb.Object, mockStorage.Object, Options);
        await achievementService.InitializeAchievementServiceForUser(userId);

        mockStorage.Setup(s => s.GetItemAsync<UserObject>(It.IsAny<string>(), null))
            .ReturnsAsync(new UserObject
            {
                UserId = userId,
                Telemetry = new DatahubUserTelemetry()
            });

        var result = await achievementService.RunRulesEngine();
        Assert.That(result, Is.TypeOf(typeof(List<RuleResultTree>)));
    }

    [Test]
    public async Task TriggersAchievementEvent()
    {
        var userId = Guid.NewGuid().ToString();

        var mockLogger = new Mock<ILogger<AchievementService>>();
        var mockStorage = new Mock<ILocalStorageService>();
        var mockCosmosDb = new Mock<IDbContextFactory<AchievementContext>>();
        var achievementService =
            new AchievementService(mockLogger.Object, mockCosmosDb.Object, mockStorage.Object, Options);
        await achievementService.InitializeAchievementServiceForUser(userId);

        mockStorage.Setup(s => s.GetItemAsync<UserObject>(It.IsAny<string>(), null))
            .ReturnsAsync(new UserObject
            {
                UserId = userId,
                Telemetry = new DatahubUserTelemetry()
            });

        achievementService.AchievementEarned += (_, e) =>
        {
            Assert.Multiple(() =>
            {
                Assert.That(e.Achievement, Is.TypeOf(typeof(Achievement)));
                Assert.That(e.UserId, Is.EqualTo(userId));
            });
        };

        var input = new DatahubUserTelemetry()
        {
            UserId = userId,
        };

        var result = await achievementService.RunRulesEngine();
        Assert.That(result, Is.TypeOf(typeof(List<RuleResultTree>)));
    }
}