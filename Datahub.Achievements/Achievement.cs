using System.ComponentModel.DataAnnotations;

namespace Datahub.Achievements;

public class Achievement
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(128)]
    public string? Name { get; set; }
    
    [Required]
    [StringLength(256)]
    public string? Description { get; set; }
    
    [Required]
    public string? Icon { get; set; }
}