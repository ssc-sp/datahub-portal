using System;
using System.Collections.Generic;

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
    public string ProfilePictureUrl { get; set;}
    public bool HideAchievements { get; set; }
    public string Language { get; set; }

    #region Navigation props
    public ICollection<UserAchievement> Achievements { get; set; }
    public ICollection<TelemetryEvent> TelemetryEvents { get; set; }
    #endregion
}
