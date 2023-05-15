using System;
using Datahub.Core.Model.Achievements;

namespace Datahub.Core.Model.Announcements;

public class Announcement
{
    public Announcement()
    {
        StartDateTime = DateTime.Now;
    }

    public int Id { get; set; }
    public string TitleEn { get; set; }
    public string TitleFr { get; set; }
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
    public bool IsVisible() => !IsDeleted && !ForceHidden;
    
    public PortalUser CreatedBy { get; set; }
    public PortalUser UpdatedBy { get; set; }
}
