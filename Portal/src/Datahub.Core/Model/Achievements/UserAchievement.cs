namespace Datahub.Core.Model.Achievements;

public class UserAchievement
{
    public int Id { get; set; }
    public int PortalUserId { get; set; }
    public string AchievementId { get; set; }
    public int Count { get; set; }
    public DateTime UnlockedAt { get; set; }

    public virtual PortalUser PortalUser { get; set; }
    public virtual Achievement Achievement { get; set; }

    public bool Earned => UnlockedAt != default;
}
