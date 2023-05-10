using System;

namespace Datahub.Core.Model.Announcements;

public class Announcement
{
    public int Id { get; set; }
    public required string TitleEn { get; set; }
    public required string TitleFr { get; set; }
    public required string PreviewEn { get; set; }
    public required string PreviewFr { get; set; }
    public required string BodyEn { get; set; }
    public required string BodyFr { get; set; }

    public bool ForceHidden { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }

    public bool IsVisible() => !IsDeleted && !ForceHidden;
}
