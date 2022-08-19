using System.ComponentModel.DataAnnotations;

namespace Datahub.Achievements;

public class UserAchievements
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string? UserId { get; set; }
    
    [Required]
    public int AchievementId { get; set; }
    
    public DateTime Date { get; set; }
    
    public Achievement? Achievement { get; set; }
}