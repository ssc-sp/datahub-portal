using System.ComponentModel.DataAnnotations;

namespace Datahub.Achievements;

public class UserAchievement
{
    
    public string? UserId { get; set; }
    
    public DateTime? Date { get; set; }
    
    public Achievement? Achievement { get; set; }
    
    public bool Earned => Date != null && Date < DateTime.UtcNow;
}