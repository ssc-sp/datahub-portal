using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Achievements;

namespace Datahub.Core.Model.Users;

/// <summary>
/// Represents a user within the portal, managing achievements, activity data, and related user information.
/// </summary>
public class PortalUser
{
    /// <summary>
    /// Gets or sets the unique identifier of this user.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the user's unique Graph identifier.
    /// </summary>
    public required string GraphGuid { get; set; }

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the user's display name.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the date and time of the user's first login.
    /// </summary>
    public DateTime? FirstLoginDateTime { get; set; }

    /// <summary>
    /// Gets or sets the date and time of the user's last login.
    /// </summary>
    public DateTime? LastLoginDateTime { get; set; }

    /// <summary>
    /// Gets or sets the URL of the user's banner picture.
    /// </summary>
    public string? BannerPictureUrl { get; set; }

    /// <summary>
    /// Gets or sets the URL of the user's profile picture.
    /// </summary>
    public string? ProfilePictureUrl { get; set; }

    /// <summary>
    /// Gets or sets the list of inactivity notifications for the user.
    /// </summary>
    public List<UserInactivityNotifications>? InactivityNotifications { get; set; }

    #region Navigation props

    /// <summary>
    /// Gets or sets the collection of achievements associated with this user.
    /// </summary>
    public ICollection<UserAchievement>? Achievements { get; set; }

    /// <summary>
    /// Gets or sets the collection of telemetry events performed by this user.
    /// </summary>
    public ICollection<TelemetryEvent>? TelemetryEvents { get; set; }

    /// <summary>
    /// Gets or sets the collection of recent links accessed by this user.
    /// </summary>
    public ICollection<UserRecentLink>? RecentLinks { get; set; }

    /// <summary>
    /// Gets or sets the user-defined settings for this user.
    /// </summary>
    public UserSettings? UserSettings { get; set; }

    /// <summary>
    /// Gets or sets the collection of Open Data submissions made by this user.
    /// </summary>
    public ICollection<OpenDataSubmission>? OpenDataSubmissions { get; set; }

    #endregion

    #region Utility functions

    /// <summary>
    /// Retrieves the list of achievements this user has earned, ordered by achievement ID and unlock date.
    /// </summary>
    /// <returns>An ordered collection of user achievements.</returns>
    public IEnumerable<UserAchievement> GetUserAchievements()
    {
        return Achievements?
                   .OrderBy(a => a.Achievement?.Id)
                   .ThenBy(a => a.UnlockedAt)
                   .ToList()
               ?? new List<UserAchievement>();
    }

    /// <summary>
    /// Retrieves the list of achievements this user has not yet earned.
    /// </summary>
    /// <returns>An ordered collection of unearned achievements.</returns>
    public IEnumerable<Achievement> GetUnEarnedAchievements()
    {
        return Achievement.GetAll()
            .Where(a => Achievements?.All(ua => ua.Achievement.Id != a.Id) ?? true)
            .OrderBy(a => a.Id)
            .ToList();
    }

    #endregion
}