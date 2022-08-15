using Microsoft.Extensions.Logging;
using Moq;
using RulesEngine.Models;

namespace Datahub.Achievements.Test;

public class Tests
{
    
    
    
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        Assert.Pass();
    }

    [Test]
    public async Task CanRunRulesEngine()
    {
        var mockLogger = new Mock<ILogger<AchievementService>>();
        var achievementService = new AchievementService(mockLogger.Object);

        var input = new UserAchievementInput()
        {
            JoinedAProject = false
        };
        
        var result = await achievementService.RunRulesEngine(input);
        Assert.That(result, Is.TypeOf(typeof(List<RuleResultTree>)));
        
        Assert.That(result.First().IsSuccess, Is.False);
    }
}