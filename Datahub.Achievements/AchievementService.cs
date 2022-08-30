using Blazored.LocalStorage;
using Datahub.Achievements.Models;
using Microsoft.EntityFrameworkCore;
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
    private const string AchievementContainerName = $"User Achievements v{AchievementVersion}";

    private readonly AchievementServiceOptions? _options;
    private readonly ILogger<AchievementService> _logger;
    private readonly ILocalStorageService _localStorage;
    private readonly IDbContextFactory<AchievementContext> _contextFactory;
    
    private string? _userId;

    public AchievementService(
        ILogger<AchievementService> logger,
        IDbContextFactory<AchievementContext> achievementContextFactory,
        ILocalStorageService localStorage,
        IOptions<AchievementServiceOptions> options)
    {
        _logger = logger;
        _localStorage = localStorage;
        _contextFactory = achievementContextFactory;
        _options = options.Value;
    }

    protected virtual void OnAchievementEarned(AchievementEarnedEventArgs args)
    {
        if (_options?.Enabled ?? false)
        {
            AchievementEarned?.Invoke(this, args);
        }
    }
    
    /// <summary>
    /// Ensures that the user has the specified record for their achievements and that the scoped service is tied to the user.
    /// </summary>
    /// <param name="userId">The user's ID</param>
    public async Task InitializeAchievementServiceForUser(string? userId)
    {
        _userId = userId ?? throw new ArgumentNullException(nameof(userId));
        
        UserObject? userObject;

        if (_options!.LocalAchievementsOnly)
        {
            userObject = await _localStorage.GetItemAsync<UserObject>(AchievementContainerName);
        }
        else
        {
            await using var ctx = await _contextFactory.CreateDbContextAsync();
            userObject = await ctx.UserObjects!.FirstOrDefaultAsync(u => u.UserId == userId);
        }

        if (userObject is null || !userObject.UserAchievements.Any())
        {
            await InitializeEmptyAchievements(userObject);
        }
    }

    /// <summary>
    /// Runs the rules engine off the user's telemetry data to determine if they have earned any achievements.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<List<RuleResultTree>> RunRulesEngine()
    {
        if (!_options?.Enabled ?? true)
        {
            return new List<RuleResultTree>();
        }
        
        var userObject = await GetUserObject();

        var achievementList = userObject.UserAchievements
            .Select(kvp => kvp.Value.Achievement)
            .ToList(); 

        var rulesEngineSettings = new ReSettings { CustomTypes = new[] { typeof(Utils) } };
        var rulesEngine = new RulesEngine.RulesEngine(new[] { AchievementFactory.CreateWorkflow(achievementList!) }, _logger, rulesEngineSettings);
        var response =
            await rulesEngine.ExecuteAllRulesAsync(AchievementFactory.AchievementWorkflowName, userObject.Telemetry );

        response.OnSuccess((successEvent) =>
        {
            // check for existing achievements
            if (userObject.UserAchievements[successEvent].Earned)
            {
                return;
            }

            // if it's a new one then save
            Task.Run(async () =>
            {
                userObject.UserAchievements[successEvent].Date = DateTime.UtcNow;
                // TODO - save cosmos db too
                await _localStorage.SetItemAsync(AchievementContainerName, userObject);
            });

            // and raise the event
            OnAchievementEarned(new AchievementEarnedEventArgs()
            {
                UserId = _userId,
                Achievement = achievementList.FirstOrDefault(a => a!.Code == successEvent)
            });
        });

        return response;
    }


    /// <summary>
    /// Reads the achievement files and creates a dictionary of UserAchievements for the user.
    /// </summary>
    /// <param name="userObject">(Optional) The user object</param>
    /// <returns>The root object that holds all the user's achievement information</returns>
    private async Task<UserObject> InitializeEmptyAchievements(UserObject? userObject = null)
    {
        var achievementFactory = await AchievementFactory.CreateFromFilesAsync(_options?.AchievementDirectoryPath);
        var emptyAchievements = achievementFactory!.Achievements!
            .ToDictionary(kvp => kvp.Key, kvp => new UserAchievement
            {
                UserId = _userId,
                Achievement = kvp.Value,
            });

        userObject ??= new UserObject
        {
            UserId = _userId,
            Telemetry = new DatahubUserTelemetry()
            {
                UserId = _userId
            },
        };
        userObject.UserAchievements = emptyAchievements;
        
        if (_options is { LocalAchievementsOnly: true })
        {
            var exists = await _localStorage.GetItemAsync<UserObject>(AchievementContainerName);
            if (exists is not null) return exists;
            
            await _localStorage.SetItemAsync(AchievementContainerName, userObject);
        }
        else
        {
            await using var ctx = await _contextFactory.CreateDbContextAsync();
            var exists = await ctx.UserObjects!.FirstOrDefaultAsync(u => u.UserId == _userId);
            if (exists is not null) return exists;
            
            await ctx.UserObjects!.AddAsync(userObject);
            await ctx.SaveChangesAsync();
        }

        return userObject;
    }
    
    /// <summary>
    /// Retrieves the user's object from storage.
    /// </summary>
    /// <returns>The root object that holds all the user's achievement information</returns>
    private async Task<UserObject> GetUserObject()
    {
        if(_userId is null)
        {
            throw new InvalidOperationException("User ID not set, please call InitializeAchievementServiceForUser first.");
        }
        
        UserObject? userObject;
        if (_options!.LocalAchievementsOnly)
        {
            userObject = await _localStorage.GetItemAsync<UserObject>(AchievementContainerName);
        }
        else
        {
            await using var ctx = await _contextFactory.CreateDbContextAsync();
            userObject = await ctx.UserObjects!.FirstOrDefaultAsync(u => u.UserId == _userId);
        }

        if (userObject is null || !userObject.UserAchievements.Any())
        {
            return await InitializeEmptyAchievements(userObject);
        }
        
        return userObject;
    }

    /// <summary>
    /// Retrieves the user's achievements from storage.
    /// </summary>
    /// <returns>The dictionary of achievements for the user where the key is the achievement's code</returns>
    public async Task<Dictionary<string, UserAchievement>> GetUserAchievements()
    {
        return (await GetUserObject()).UserAchievements;
    }
    
    public async Task<int> AddOrIncrementTelemetryEvent(string eventName, int value)
    {
        var userObject = await GetUserObject();
        userObject.Telemetry.AddOrIncrementEventMetric(eventName, value);

        await RunRulesEngine();

        return userObject.Telemetry.GetEventMetric(eventName);
    }
}