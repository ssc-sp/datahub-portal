using Microsoft.Extensions.Logging;
using MudBlazor;
using RulesEngine.Extensions;
using RulesEngine.Models;

namespace Datahub.Achievements;

public class AchievementService
{
    public event EventHandler<AchievementEarnedEventArgs>? AchievementEarned;

    private readonly ILogger<AchievementService> _logger;

    public AchievementService(ILogger<AchievementService> logger)
    {
        _logger = logger;
    }

    protected virtual void OnAchievementEarned(AchievementEarnedEventArgs args)
    {
        AchievementEarned?.Invoke(this, args);
    }

    public async Task<List<RuleResultTree>> RunRulesEngine(DatahubUserTelemetry? input,
        string? achievementDirectoryPath = null)
    {
        var achievementFactory = await AchievementFactory.CreateFromFilesAsync(achievementDirectoryPath);

        var rulesEngine = new RulesEngine.RulesEngine(new[] { achievementFactory.CreateWorkflow() }, _logger);
        var response =
            await rulesEngine.ExecuteAllRulesAsync(AchievementFactory.AchievementWorkflowName, input);

        response.OnSuccess((successEvent) =>
        {
            // check for existing achievements

            // if it's a new one then raise event
            OnAchievementEarned(new AchievementEarnedEventArgs()
            {
                UserId = input?.UserId ?? "UserId not found",
                Achievement = achievementFactory.FromCode(successEvent)
            });
        });

        return response;
    }
}