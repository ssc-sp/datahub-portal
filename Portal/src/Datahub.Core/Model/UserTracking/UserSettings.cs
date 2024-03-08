using Datahub.Core.Model.Achievements;
namespace Datahub.Core.Model.UserTracking;

public class UserSettings
{
    public int PortalUserId { get; set; }
    public string UserName { get; set; }
    public PortalUser User { get; set; }
    public DateTime? AcceptedDate { get; set; }
    public string Language { get; set; }

    public bool NotificationsEnabled { get; set; }
    public bool HideAchievements { get; set; }
    public bool HideAlerts { get; set; }
    public List<string> HiddenAlerts { get; set; }
}