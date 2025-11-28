namespace Datahub.Core.Model.Users;

/// <summary>
/// Represents a collection of user-specific settings and preferences.
/// </summary>
public class UserSettings
{
    /// <summary>
    /// Gets or sets the unique identifier for the portal user associated with these settings.
    /// </summary>
    public int PortalUserId { get; set; }

    /// <summary>
    /// Gets or sets the username for the portal user.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="PortalUser"/> entity tied to these settings.
    /// </summary>
    public PortalUser? User { get; set; }

    /// <summary>
    /// Gets or sets the date and time this user accepted the terms or conditions.
    /// </summary>
    public DateTime? AcceptedDate { get; set; }

    /// <summary>
    /// Gets or sets the preferred language for this user.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether notifications are enabled.
    /// </summary>
    public bool NotificationsEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether achievement badges and records are hidden.
    /// </summary>
    public bool HideAchievements { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether system alerts are hidden.
    /// </summary>
    public bool HideAlerts { get; set; }

    /// <summary>
    /// Gets or sets a collection of alert identifiers that the user has chosen to hide.
    /// </summary>
    public List<string>? HiddenAlerts { get; set; }

    /// <summary>
    /// Gets or sets the preferred theme for this user ("Light mode" or "Dark mode").
    /// </summary>
    public string? Theme { get; set; }
}