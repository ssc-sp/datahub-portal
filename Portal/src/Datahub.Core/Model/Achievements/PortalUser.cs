using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.UserTracking;

namespace Datahub.Core.Model.Achievements;

public class PortalUser
{
    public int Id { get; set; }
    public required string GraphGuid { get; set; }
    public string Email { get; set; }
    public string DisplayName { get; set; }
    public DateTime? FirstLoginDateTime { get; set; }
    public DateTime? LastLoginDateTime { get; set; }
    public string BannerPictureUrl { get; set; }
    public string ProfilePictureUrl { get; set; }

    public List<UserInactivityNotifications> InactivityNotifications { get; set; }

    #region Navigation props
    public ICollection<UserAchievement> Achievements { get; set; }
    public ICollection<TelemetryEvent> TelemetryEvents { get; set; }
    public ICollection<UserRecentLink> RecentLinks { get; set; }
    public UserSettings UserSettings { get; set; }

    public ICollection<OpenDataSubmission> OpenDataSubmissions { get; set; }
    #endregion

    #region Utility functions

    public IEnumerable<UserAchievement> GetUserAchievements()
    {
        return Achievements?
                   .OrderBy(a => a.Achievement?.Id)
                   .ThenBy(a => a.UnlockedAt)
                   .ToList()
               ?? new List<UserAchievement>();
    }

    public IEnumerable<Achievement> GetUnEarnedAchievements()
    {
        return Achievement.GetAll()
            .Where(a => Achievements?.All(ua => ua.Achievement.Id != a.Id) ?? false)
            .OrderBy(a => a.Id)
            .ToList();
    }
    #endregion
}
