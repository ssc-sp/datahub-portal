using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MudBlazor;
using RulesEngine.Extensions;
using RulesEngine.Models;

namespace Datahub.Achievements;

public class AchievementService
{
    public event EventHandler<AchievementEarnedEventArgs>? AchievementEarned;

    private readonly AchievementServiceOptions? _options;
    private readonly ILogger<AchievementService> _logger;

    public AchievementService(ILogger<AchievementService> logger, IOptions<AchievementServiceOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    protected virtual void OnAchievementEarned(AchievementEarnedEventArgs args)
    {
        if (_options?.Enabled ?? false)
        {
            AchievementEarned?.Invoke(this, args);
        }
    }

    public async Task<List<RuleResultTree>> RunRulesEngine(DatahubUserTelemetry? input)
    {
        if (!_options?.Enabled ?? true)
        {
            return new List<RuleResultTree>();
        }
        
        var achievementFactory = await AchievementFactory.CreateFromFilesAsync(_options?.AchievementDirectoryPath);

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