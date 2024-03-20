using System.ComponentModel.DataAnnotations;

namespace Datahub.Core.Model;

public class MiscStoredObject
{
    [Key]
    public Guid GeneratedId { get; set; }

    public string Id { get; set; }
    public string TypeName { get; set; }

    [Required]
    public string JsonContent { get; set; }
}