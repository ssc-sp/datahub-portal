namespace Datahub.Core.Model.Achievements;

/// <summary>
/// Represents a user's progress or completion of a specific achievement.
/// </summary>
public class UserAchievement
{
    /// <summary>
    /// Gets or sets the unique identifier for this user achievement record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the portal user associated with this achievement.
    /// </summary>
    public int PortalUserId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the achievement.
    /// </summary>
    public string AchievementId { get; set; }

    /// <summary>
    /// Gets or sets the count or progress towards completing the achievement.
    /// For achievements that can be earned multiple times or have multiple steps.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the achievement was unlocked or earned.
    /// A default DateTime value indicates the achievement has not yet been earned.
    /// </summary>
    public DateTime UnlockedAt { get; set; }

    #region Navigation props

    /// <summary>
    /// Gets or sets the navigation property for the portal user associated with this achievement.
    /// </summary>
    public virtual PortalUser PortalUser { get; set; }

    /// <summary>
    /// Gets or sets the navigation property for the achievement definition.
    /// </summary>
    public virtual Achievement Achievement { get; set; }
    #endregion

    #region Utility functions

    /// <summary>
    /// Gets a value indicating whether the achievement has been earned (i.e., UnlockedAt date is set).
    /// </summary>
    public bool Earned => UnlockedAt != default;

    #endregion
}
