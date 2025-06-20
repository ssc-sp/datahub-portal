using System.ComponentModel.DataAnnotations;

namespace Datahub.Core.Model;

/// <summary>
/// Stores miscellaneous objects in JSON format, such as Azure pricing data.
/// </summary>
public class MiscStoredObject
{
    /// <summary>
    /// Gets or sets the unique generated identifier.
    /// </summary>
    [Key]
    public Guid GeneratedId { get; set; }

    /// <summary>
    /// Gets or sets the textual identifier for this stored object.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the type name or category of the stored data.
    /// </summary>
    public string TypeName { get; set; }

    /// <summary>
    /// Gets or sets the JSON data content representing this object.
    /// </summary>
    [Required]
    public string JsonContent { get; set; }
}