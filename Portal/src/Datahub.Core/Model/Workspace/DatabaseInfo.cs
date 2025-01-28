namespace Datahub.Core.Model.Workspace;

public class DatabaseInfo
{
    /// <summary>
    /// Gets or sets the connection string with no credentials.
    /// </summary>
    public string Connection { get; set; }

    /// <summary>
    /// Gets or sets the database edition.
    ///
    /// Example: Basic, Standard, Premium or Data Warehouse.
    /// </summary>
    public string Edition { get; set; }

    /// <summary>
    /// Gets or sets the pricing tier of the database.
    ///
    /// Example: Basic or S0, S1, S2, S3, S4, S6, S7, S9, P1, P2, P4, P6, P11 or P15 etc.
    /// </summary>
    public string ServiceObjective { get; set; }

    /// <summary>
    /// Gets or sets the name of the database
    ///
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the name of the elastic pool that the database belongs to
    /// return null if the database is a single database or a dedicated SQL pool.
    ///
    /// </summary>
    public string ElasticPoolName { get; set; }

    /// <summary>
    /// Gets or sets PostgreSQL version.
    /// </summary>
    public string PSQLVersion { get; set; }

    /// <summary>
    /// Gets or sets the size of the database
    ///
    /// </summary>
    public string Size { get; set; }

    /// <summary>
    /// Gets or sets the location of the database
    ///
    /// </summary>
    public string Location { get; set; }
}
