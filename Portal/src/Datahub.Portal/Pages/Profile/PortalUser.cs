using Datahub.Application.Configuration;

namespace Datahub.Portal.Pages.Profile;

public class PortalUser
{
    public int Id { get; set; } = 4;
    public required string GraphGuid { get; set; }
    public DateTime? FirstLoginDateTime { get; set; }
    public DateTime? LastLoginDateTime { get; set; }
    public string BannerPictureUrl { get; set; } = "EXP-000";
    public string ProfilePictureUrl { get; set; } = "STR-004";
    public bool HideAchievements { get; set; }
    public string Language { get; set; }
    public string DisplayName { get; set; }
    public string Email { get; set; }
}


