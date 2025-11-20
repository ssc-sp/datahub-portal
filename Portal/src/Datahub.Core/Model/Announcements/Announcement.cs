using Datahub.Core.Model.Users;

namespace Datahub.Core.Model.Announcements;

/// <summary>
/// Represents an announcement to be displayed within the portal.
/// </summary>
public class Announcement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Announcement"/> class,
    /// setting the <see cref="StartDateTime"/> to the current time by default.
    /// </summary>
    public Announcement()
    {
        StartDateTime = DateTime.Now;
    }

    /// <summary>
    /// Gets or sets the unique identifier for the announcement.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the English preview text for the announcement.
    /// </summary>
    public string PreviewEn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the French preview text for the announcement.
    /// </summary>
    public string PreviewFr { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the English body content of the announcement.
    /// </summary>
    public string BodyEn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the French body content of the announcement.
    /// </summary>
    public string BodyFr { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an int representing the alert level of the announcement.
    /// </summary>
    public int Severity { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the announcement is forcibly hidden, regardless of other settings.
    /// </summary>
    public bool ForceHidden { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the announcement has been marked as deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the announcement should start being visible.
    /// If null, the announcement may be considered active immediately or based on other criteria.
    /// </summary>
    public DateTime? StartDateTime { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the announcement should stop being visible.
    /// If null, the announcement may be considered active indefinitely or until manually hidden/deleted.
    /// </summary>
    public DateTime? EndDateTime { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the announcement was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created the announcement.
    /// </summary>
    public int CreatedById { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the announcement was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last updated the announcement.
    /// </summary>
    public int? UpdatedById { get; set; }

    /// <summary>
    /// Checks if the announcement is a new, unsaved entity.
    /// </summary>
    /// <returns>True if the announcement is new (Id is 0); otherwise, false.</returns>
    public bool IsNew() => Id == 0;

    /// <summary>
    /// Check whether the announcement is visible to regular users.
    /// An announcement is visible if it's not deleted, not forcibly hidden, and not scheduled for a future date.
    /// </summary>
    /// <returns>True if announcement is visible</returns>
    public bool IsVisible() => !IsDeleted && !ForceHidden && !IsScheduled();

    /// <summary>
    /// Check whether the announcement is visible in the carousel.
    /// An announcement is in the carousel if it's visible, its start time is in the past or current,
    /// and its end time is null or in the future.
    /// </summary>
    /// <returns>True if Announcement in Carousel</returns>
    public bool IsInCarousel() => IsVisible() && StartDateTime <= DateTime.UtcNow && (EndDateTime == null || EndDateTime >= DateTime.UtcNow);

    /// <summary>
    /// Check whether the announcement is scheduled to be visible in the future.
    /// </summary>
    /// <returns>True if Announcement is scheduled (StartDateTime is in the future)</returns>
    public bool IsScheduled() => StartDateTime > DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the navigation property for the user who created the announcement.
    /// </summary>
    public PortalUser CreatedBy { get; set; } = null!;

    /// <summary>
    /// Gets or sets the navigation property for the user who last updated the announcement.
    /// </summary>
    public PortalUser? UpdatedBy { get; set; }
}
