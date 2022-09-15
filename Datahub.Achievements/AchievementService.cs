using Blazored.LocalStorage;
using Datahub.Achievements.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RulesEngine.Models;

namespace Datahub.Achievements;

public class AchievementService
{
    public event EventHandler<AchievementEarnedEventArgs>? AchievementEarned;

    private const string AchievementVersion = "1.0.4";
    public const string AchievementContainerName = $"User Achievements v{AchievementVersion}";

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

        await SynchronizeUserObjectWithLatest(userObject);
    }

    /// <summary>
    /// Runs the rules engine off the user's telemetry data to determine if they have earned any achievements.
    /// </summary>
    /// <returns></returns>
    public async Task<List<RuleResultTree>> RunRulesEngine()
    {
        if (!_options?.Enabled ?? true)
        {
            return new List<RuleResultTree>();
        }

        var userObject = await GetUserObject();

        var achievementList = userObject.UserAchievements
            .Select(u => u.Achievement)
            .ToList();

        var rulesEngineSettings = new ReSettings { CustomTypes = new[] { typeof(Utils) } };
        var workflows = AchievementFactory.CreateWorkflow(achievementList!);
        var rulesEngine = new RulesEngine.RulesEngine(new[] { workflows }, _logger, rulesEngineSettings);

        // var result = await rulesEngine.ExecuteActionWorkflowAsync().ExecuteAsync(userObject);
        var response =
            await rulesEngine.ExecuteAllRulesAsync(AchievementFactory.AchievementWorkflowName, userObject.Telemetry);

        foreach (var userAchievement in from ruleResultTree in response
                 where ruleResultTree.IsSuccess
                 select userObject.UserAchievements
                     .FirstOrDefault(u => u.Code == ruleResultTree.Rule.SuccessEvent && u.Earned == false)
                 into userAchievement
                 where userAchievement is not null
                 select userAchievement)
        {
            // if it's a new one then save
            userAchievement!.Date = DateTime.UtcNow;
            if (_options!.LocalAchievementsOnly)
            {
                await _localStorage.SetItemAsync(AchievementContainerName, userObject);
            }
            else
            {
                await using var ctx = await _contextFactory.CreateDbContextAsync();
                ctx.UserObjects!.Update(userObject);
                await ctx.SaveChangesAsync();
            }

            // and raise the event
            OnAchievementEarned(new AchievementEarnedEventArgs()
            {
                UserId = _userId,
                Achievement = userAchievement.Achievement
            });
        }

        return response;
    }


    /// <summary>
    /// Retrieves the user's object from storage.
    /// </summary>
    /// <returns>The root object that holds all the user's achievement information</returns>
    private async Task<UserObject> GetUserObject(string? userId = null)
    {
        if (_userId is null && userId is null)
        {
            throw new InvalidOperationException(
                "User ID not set, please call InitializeAchievementServiceForUser first or pass a User ID.");
        }

        UserObject? userObject;
        if (_options!.LocalAchievementsOnly)
        {
            userObject = await _localStorage.GetItemAsync<UserObject>(AchievementContainerName);
        }
        else
        {
            await using var ctx = await _contextFactory.CreateDbContextAsync();
            userObject = await ctx.UserObjects!.FirstOrDefaultAsync(u => u.UserId == (userId ?? _userId));
        }

        return await SynchronizeUserObjectWithLatest(userObject);
    }

    /// <summary>
    /// Updates and verifies that the user's achievements are in sync with the latest achievements from the achievement files.
    /// </summary>
    /// <description>
    /// Trying to keep it O(n) here instead of O(n^2) by using a dictionary.
    /// Converts the users achievements to a dictionary by code [O(n)]
    /// then clones the achievement file dictionary [O(n)]
    /// and within that iteration it looks up the earned date by code from the achievements that are already in the user's dictionary [O(1)]
    /// and finally converts the dictionary back to a list [O(n)]
    ///
    /// Also handles the case where the user has earned an achievement that is no longer in the achievement files and removes it from the user's list
    /// </description>
    /// <param name="userObject"></param>
    /// <returns>The user's object with all of the latest achievements included in it</returns>
    public async Task<UserObject> SynchronizeUserObjectWithLatest(UserObject? userObject)
    {
        userObject ??= new UserObject
        {
            UserId = _userId,
            Telemetry = new DatahubUserTelemetry()
            {
                UserId = _userId
            },
            UserAchievements = new List<UserAchievement>()
        };

        var achievementFactory = await AchievementFactory.CreateFromFilesAsync(_options?.AchievementDirectoryPath);

        var userAchievementsByCode = userObject.UserAchievements
            .ToDictionary(u => u.Code!, u => u);

        var updatedUserAchievements = achievementFactory.Achievements!
            .ToDictionary(pair => pair.Key, pair => new UserAchievement()
            {
                UserId = _userId,
                Achievement = pair.Value,
                Date = userAchievementsByCode.TryGetValue(pair.Key, out var userAchievement)
                    ? userAchievement.Date
                    : null
            });

        userObject.UserAchievements = updatedUserAchievements.Values.ToList();

        await SaveUserObject(userObject);

        return userObject;
    }

    /// <summary>
    /// Retrieves the user's achievements from storage.
    /// </summary>
    /// <returns>The dictionary of achievements for the user where the key is the achievement's code</returns>
    public async Task<List<UserAchievement>> GetUserAchievements(string? userId = null)
    {
        return (await GetUserObject(userId)).UserAchievements.ToList();
    }

    public async Task<int> AddOrIncrementTelemetryEvent(string eventName, int value, string? userId = null)
    {
        var userObject = await GetUserObject(userId);
        userObject.Telemetry.AddOrIncrementEventMetric(eventName, value);

        await SaveUserObject(userObject);
        await RunRulesEngine();

        return userObject.Telemetry.GetEventMetric(eventName);
    }

    /// <summary>
    /// Saves the user's object to storage (local or cosmos).
    /// </summary>
    /// <param name="userObject"></param>
    private async Task SaveUserObject(UserObject userObject)
    {
        if (_options is { LocalAchievementsOnly: true })
        {
            await _localStorage.SetItemAsync(AchievementContainerName, userObject, CancellationToken.None);
        }
        else
        {
            await using var ctx = await _contextFactory.CreateDbContextAsync();
            var exists = await ctx.UserObjects!
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userObject.UserId);
            if (exists is not null)
            {
                ctx.UserObjects!.Remove(exists);
                await ctx.SaveChangesAsync();
            }

            await ctx.UserObjects!.AddAsync(userObject);
            await ctx.SaveChangesAsync();
        }
    }
}