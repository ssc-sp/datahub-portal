namespace Datahub.Core.Model.Achievements;

public class UserAchievement
{
    public int Id { get; set; }
    public int PortalUserId { get; set; }
    public string AchievementId { get; set; }
    public int Count { get; set; }
    public DateTime UnlockedAt { get; set; }

    #region Navigation props
    public virtual PortalUser PortalUser { get; set; }
    public virtual Achievement Achievement { get; set; }
    #endregion

    #region Utility functions

    public bool Earned => UnlockedAt != default;

    #endregion
}
