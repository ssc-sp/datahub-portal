using System.ComponentModel.DataAnnotations;

namespace Datahub.Achievements.Models;

public record UserObject
{
    [Key]
    public string? UserId { get; set; }
    
    public DatahubUserTelemetry Telemetry { get; set; } = new();
    
    public ICollection<UserAchievement> UserAchievements { get; set; } =  new List<UserAchievement>();
    
}