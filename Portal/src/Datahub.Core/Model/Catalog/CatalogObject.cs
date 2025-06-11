namespace Datahub.Core.Model.Catalog;

/// <summary>
/// Represents a generic object in the data catalog.
/// </summary>
public class CatalogObject
{
    /// <summary>
    /// Gets or sets the unique identifier for the catalog object.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the type of the catalog object.
    /// </summary>
    public CatalogObjectType ObjectType { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the actual object being cataloged (e.g., user ID, workspace ID).
    /// </summary>
    public string ObjectId { get; set; }

    /// <summary>
    /// Gets or sets the English name of the catalog object.
    /// </summary>
    public string Name_English { get; set; }

    /// <summary>
    /// Gets or sets the French name of the catalog object.
    /// </summary>
    public string Name_French { get; set; }

    /// <summary>
    /// Gets or sets the English description of the catalog object.
    /// </summary>
    public string Desc_English { get; set; }

    /// <summary>
    /// Gets or sets the French description of the catalog object.
    /// </summary>
    public string Desc_French { get; set; }

    /// <summary>
    /// Gets or sets the location or source of the catalog object (e.g., URL, path).
    /// </summary>
    public string Location { get; set; }
}

/// <summary>
/// Defines the types of objects that can be represented in the catalog.
/// </summary>
public enum CatalogObjectType
{
    /// <summary>
    /// Represents a user object.
    /// </summary>
    User,

    /// <summary>
    /// Represents a workspace object.
    /// </summary>
    Workspace,

    /// <summary>
    /// Represents a repository object.
    /// </summary>
    Repository
}