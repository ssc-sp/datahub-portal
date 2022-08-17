using Microsoft.Extensions.Logging;
using Moq;
using RulesEngine.Models;

namespace Datahub.Achievements.Test;

public class Tests
{
    private readonly string AchievementDirectoryPath =
        Path.Join(Directory.GetCurrentDirectory(), "../../../Achievements");


    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void SanityCheck()
    {
        Assert.Pass();
    }

    [Test]
    public async Task CanRunRulesEngineWithAchievementWorkflow()
    {
        var mockLogger = new Mock<ILogger<AchievementService>>();
        var achievementService = new AchievementService(mockLogger.Object);

        var input = new DatahubUserTelemetry();

        var result = await achievementService.RunRulesEngine(input, AchievementDirectoryPath);
        Assert.That(result, Is.TypeOf(typeof(List<RuleResultTree>)));
    }

    [Test]
    public async Task TriggersAchievementEvent()
    {
        var mockLogger = new Mock<ILogger<AchievementService>>();
        var achievementService = new AchievementService(mockLogger.Object);

        var userId = Guid.NewGuid().ToString();

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

        var result = await achievementService.RunRulesEngine(input, AchievementDirectoryPath);
        Assert.That(result, Is.TypeOf(typeof(List<RuleResultTree>)));
    }
}