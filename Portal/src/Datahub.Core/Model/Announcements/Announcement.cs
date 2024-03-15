using Datahub.Core.Model.Achievements;

namespace Datahub.Core.Model.Announcements;

public class Announcement
{
    public Announcement()
    {
        StartDateTime = DateTime.Now;
    }

    public int Id { get; set; }
    public string PreviewEn { get; set; }
    public string PreviewFr { get; set; }
    public string BodyEn { get; set; }
    public string BodyFr { get; set; }

    public bool ForceHidden { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }

    public DateTime CreatedAt { get; set; }
    public int CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedById { get; set; }

    public bool IsNew() => Id == 0;

    /// <summary>
    /// Check whether the announcement is visible to regular users.
    /// </summary>
    /// <returns>True if announcement is visible</returns>
    public bool IsVisible() => !IsDeleted && !ForceHidden && !IsScheduled();

    /// <summary>
    /// Check whether the announcement is visible in the carousel.
    /// </summary>
    /// <returns>True if Announcement in Carousel</returns>
    public bool IsInCarousel() => IsVisible() && StartDateTime <= DateTime.UtcNow && (EndDateTime == null || EndDateTime >= DateTime.UtcNow);

    /// <summary>
    /// Check whether the announcement is scheduled to be visible in the future.
    /// </summary>
    /// <returns>True if Announcement is scheduled</returns>
    public bool IsScheduled() => StartDateTime > DateTime.UtcNow;

    public PortalUser CreatedBy { get; set; }
    public PortalUser UpdatedBy { get; set; }
}
