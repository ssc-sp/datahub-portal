using System.ComponentModel.DataAnnotations;

namespace Datahub.Achievements.Models;

public record UserAchievement
{
    [Key] 
    public Guid Id { get; set; }
    public string? UserId { get; set; }
    
    public DateTime? Date { get; set; }
    
    public Achievement? Achievement { get; set; }
    
    public bool Earned => Date != null && Date < DateTime.UtcNow;
    public string? Code => Achievement?.Code;
}