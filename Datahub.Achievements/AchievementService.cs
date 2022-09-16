using System.Security.Authentication;
using Blazored.LocalStorage;
using Datahub.Achievements.Models;
using Microsoft.AspNetCore.Components.Authorization;
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
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public AchievementService(
        ILogger<AchievementService> logger,
        IDbContextFactory<AchievementContext> achievementContextFactory,
        ILocalStorageService localStorage,
        AuthenticationStateProvider authenticationStateProvider,
        IOptions<AchievementServiceOptions> options)
    {
        _logger = logger;
        _localStorage = localStorage;
        _contextFactory = achievementContextFactory;
        _authenticationStateProvider = authenticationStateProvider;
        _options = options.Value;
    }

    protected virtual void OnAchievementEarned(AchievementEarnedEventArgs args)
    {
        if (_options?.Enabled ?? false)
        {
            AchievementEarned?.Invoke(this, args);
        }
    }

    public async Task<bool> RunRulesEngine(UserObject userObject)
    {
        if (!_options?.Enabled ?? true)
        {
            return false;
        }

        var achievementList = userObject.UserAchievements
            .Select(u => u.Achievement)
            .ToList();

        var rulesEngineSettings = new ReSettings { CustomTypes = new[] { typeof(Utils) } };
        var workflows = AchievementFactory.CreateWorkflow(achievementList!);
        var rulesEngine = new RulesEngine.RulesEngine(new[] { workflows }, _logger, rulesEngineSettings);

        var response =
            await rulesEngine.ExecuteAllRulesAsync(AchievementFactory.AchievementWorkflowName, userObject.Telemetry);

        var hasEarnedNewAchievement = false;

        foreach (var userAchievement in from ruleResultTree in response
                 where ruleResultTree.IsSuccess
                 select userObject.UserAchievements
                     .FirstOrDefault(u => u.Code == ruleResultTree.Rule.SuccessEvent && u.Earned == false)
                 into userAchievement
                 where userAchievement is not null
                 select userAchievement)
        {
            // if it's a new one then record and flag for save
            userAchievement!.Date = DateTime.UtcNow;
            hasEarnedNewAchievement = true;

            // and raise the event
            OnAchievementEarned(new AchievementEarnedEventArgs()
            {
                UserId = userObject.UserId,
                Achievement = userAchievement.Achievement
            });
        }

        return hasEarnedNewAchievement;
    }

    private async Task<UserObject> GetUserObject(string? userId = null)
    {
        if (userId is null)
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            userId = authState.User.Identity?.Name;

            if (userId is null)
            {
                throw new AuthenticationException("Self fetching user is not authenticated");
            }
        }

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
        
        userObject ??= new UserObject
        {
            UserId = userId,
            Telemetry = new DatahubUserTelemetry()
            {
                UserId = userId
            },
            UserAchievements = new List<UserAchievement>()
        };

        userObject.UserAchievements = await SynchronizeUserAchievements(userObject);
        return userObject;
    }

    private async Task<List<UserAchievement>> SynchronizeUserAchievements(UserObject userObject)
    {
        var achievementFactory = await AchievementFactory.CreateFromFilesAsync(_options?.AchievementDirectoryPath);
        var userAchievementsByCode = userObject.UserAchievements
            .ToDictionary(u => u.Code!, u => u);

        var updatedUserAchievements = achievementFactory.Achievements!
            .ToDictionary(pair => pair.Key, pair => new UserAchievement
            {
                UserId = userObject.UserId,
                Achievement = pair.Value,
                Date = userAchievementsByCode.TryGetValue(pair.Key, out var userAchievement)
                    ? userAchievement.Date
                    : null
            });

        return updatedUserAchievements.Values.ToList();
    }

    public async Task<List<UserAchievement>> GetUserAchievements(string? userId = null)
    {
        return (await GetUserObject(userId)).UserAchievements.ToList();
    }

    public async Task<int> AddOrIncrementTelemetryEvent(string eventName, int value, string? userId = null)
    {
        var userObject = await GetUserObject(userId);
        
        userObject.Telemetry.AddOrIncrementEventMetric(eventName, value);
        
        var hasEarnedNewAchievement = await RunRulesEngine(userObject);
        if (hasEarnedNewAchievement)
        {
            await SaveUserObject(userObject);
        }

        return userObject.Telemetry.GetEventMetric(eventName);
    }

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
                ctx.UserObjects!.Update(userObject);
            }
            else
            {
                await ctx.UserObjects!.AddAsync(userObject);
            }

            await ctx.SaveChangesAsync();
        }
    }
}