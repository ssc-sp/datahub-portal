namespace Datahub.Core.Model.Documentation;

/// <summary>
/// Represents a documentation resource and tracks its usage statistics.
/// </summary>
public class DocumentationResource
{
    /// <summary>
    /// Gets or sets the unique identifier for the documentation resource.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the documentation resource was last updated.
    /// </summary>
    public DateTime LastUpdated { get; set; }

    /// <summary>
    /// Gets or sets the number of times the documentation resource has been accessed or viewed.
    /// </summary>
    public int Hits { get; set; }
}
