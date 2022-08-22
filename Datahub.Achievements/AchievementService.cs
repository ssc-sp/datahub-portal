using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MudBlazor;
using RulesEngine.Extensions;
using RulesEngine.Models;

namespace Datahub.Achievements;

public class AchievementService
{
    public event EventHandler<AchievementEarnedEventArgs>? AchievementEarned;
    
    private const string AchievementVersion = "1.0.4";
    private const string AchievementContainerName = $"Achievements v{AchievementVersion}";

    private readonly AchievementServiceOptions? _options;
    private readonly ILogger<AchievementService> _logger;
    private readonly ILocalStorageService _localStorage;

    public AchievementService(ILogger<AchievementService> logger, ILocalStorageService localStorage, IOptions<AchievementServiceOptions> options)
    {
        _logger = logger;
        _localStorage = localStorage;
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
        var userAchievements = await _localStorage.GetItemAsync<Dictionary<string, UserAchievement>>(AchievementContainerName) ??
                               await SetupEmptyAchievements(achievementFactory, input?.UserId);

        var rulesEngine = new RulesEngine.RulesEngine(new[] { achievementFactory.CreateWorkflow() }, _logger);
        var response =
            await rulesEngine.ExecuteAllRulesAsync(AchievementFactory.AchievementWorkflowName, input);

        response.OnSuccess((successEvent) =>
        {
            // check for existing achievements
            if (userAchievements[successEvent].Earned)
            {
                return;
            }

            // if it's a new one then save
            Task.Run(async () =>
            {
                userAchievements[successEvent].Date = DateTime.UtcNow;
                await _localStorage.SetItemAsync(AchievementContainerName, userAchievements);
            });
            
            // and raise the event
            OnAchievementEarned(new AchievementEarnedEventArgs()
            {
                UserId = input?.UserId ?? "UserId not found",
                Achievement = achievementFactory.FromCode(successEvent)
            });
        });
    
        return response;
    }
    
    private async Task<Dictionary<string, UserAchievement>> SetupEmptyAchievements(AchievementFactory achievementFactory, string? userId)
    {
        var emptyAchievements = achievementFactory!.Achievements!.ToDictionary(kvp => kvp.Key, kvp => new UserAchievement()
        {
            UserId = userId,
            Achievement = kvp.Value,
        });
        
        await _localStorage.SetItemAsync(AchievementContainerName, emptyAchievements);

        return emptyAchievements;
    }

    public async Task<Dictionary<string, UserAchievement>> GetUserAchievements(string userId)
    {
        return await _localStorage.GetItemAsync<Dictionary<string, UserAchievement>>(AchievementContainerName);
    }

    public async Task InitializeAchievements(string? userName)
    {
        var achievementFactory = await AchievementFactory.CreateFromFilesAsync(_options?.AchievementDirectoryPath);
        
        await SetupEmptyAchievements(achievementFactory, userName);
        
        var userAchievements = await _localStorage.GetItemAsync<Dictionary<string, UserAchievement>>(AchievementContainerName) ??
                               await SetupEmptyAchievements(achievementFactory, userName);
    }
}