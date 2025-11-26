using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Datahub.Core.Model.Users;
using Datahub.Shared;
using Datahub.Shared.Entities;
using Newtonsoft.Json;

namespace Datahub.Core.Model.Projects;

/// <summary>
/// Represents a resource associated with a Datahub project.
/// </summary>
public class Project_Resources2
{
    /// <summary>
    /// Gets or sets the unique identifier for the project resource.
    /// </summary>
    [Key]
    public Guid ResourceId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the type of the resource.
    ///
    /// Prefixed with "terraform:resource_name".
    /// </summary>
    [Required]
    [StringLength(200)]
    public string ResourceType { get; set; } = null!;

    /// <summary>
    /// Gets or sets the class name of the resource, defaulting to "legacy".
    /// </summary>
    [Required]
    [StringLength(200)]
    public string ClassName { get; set; } = "legacy";

    /// <summary>
    /// Gets or sets the JSON content representing the resource's configuration or metadata. Defaults to an empty JSON object.
    /// </summary>
    public string? JsonContent { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the foreign key referencing the Datahub_Project this resource belongs to.
    /// </summary>
    [ForeignKey("Project")]
    public int ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the resource was requested. Defaults to the current UTC time.
    /// </summary>
    public DateTime? RequestedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the ID of the user who requested the resource.
    /// </summary>
    public int RequestedById { get; set; }

    /// <summary>
    /// Gets or sets the PortalUser who requested the resource.
    /// </summary>
    public PortalUser? RequestedBy { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the resource was created.
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the Datahub_Project this resource is associated with.
    /// </summary>
    public Datahub_Project? Project { get; set; }

    /// <summary>
    /// Gets or sets the status of the project resource.
    /// The status represents the current state of the terraform for the resource within a workspace.
    ///
    /// See <see cref="TerraformStatus"/> for possible values.
    /// </summary>
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string? Status { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the resource was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who last updated the resource.
    /// </summary>
    public int? UpdatedById { get; set; }

    /// <summary>
    /// Gets or sets the ID of the pipeline run linked to the resource.
    /// </summary>
    public int? PipelineId { get; set; }

    /// <summary>
    /// Gets or sets the PortalUser who last updated the resource.
    /// </summary>
    public PortalUser? UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets the input JSON content for the resource. Defaults to an empty JSON object.
    /// </summary>
    public string? InputJsonContent { get; set; } = "{}";

    /// <summary>
    /// Converts the current instance of Project_Resource to a TerraformTemplate object.
    /// </summary>
    /// <returns>A new instance of TerraformTemplate with the ResourceType, Status, and RequestedAt properties set.</returns>
    public TerraformTemplate ToTerraformTemplate()
    {
        return new TerraformTemplate(TerraformTemplate.NormalizeTemplateName(ResourceType), Status ?? string.Empty, DateTime.UtcNow);
    }
}

/// <summary>
/// Defines constants related to project resources, such as service types and storage types.
/// </summary>
public static class ProjectResourceConstants
{
    /// <summary>
    /// Represents the PostgreSQL service type.
    /// </summary>
    public const string SERVICE_TYPE_POSTGRES = "psql";

    /// <summary>
    /// Represents the SQL Server service type.
    /// </summary>
    public const string SERVICE_TYPE_SQL_SERVER = "sql";

    /// <summary>
    /// Represents the Azure Storage service type.
    /// </summary>
    public const string SERVICE_TYPE_STORAGE = "storage";

    /// <summary>
    /// Represents the Azure Databricks service type.
    /// </summary>
    public const string SERVICE_TYPE_DATABRICKS = "databricks";

    /// <summary>
    /// Represents the Power BI service type.
    /// </summary>
    public const string SERVICE_TYPE_POWERBI = "powerbi";

    /// <summary>
    /// Represents the Virtual Machine service type.
    /// </summary>
    public const string SERVICE_TYPE_VIRTUAL_MACHINE = "virtual-machine";

    /// <summary>
    /// Represents the Azure Blob Storage type.
    /// </summary>
    public const string STORAGE_TYPE_BLOB = "blob";

    /// <summary>
    /// Represents the Azure Data Lake Storage Gen2 type.
    /// </summary>
    public const string STORAGE_TYPE_GEN2 = "gen2";

    /// <summary>
    /// An array containing all supported project resource types.
    /// </summary>
    public static readonly string[] ALL_RESOURCE_TYPES = new[]
    {
        SERVICE_TYPE_DATABRICKS,
        SERVICE_TYPE_SQL_SERVER,
        SERVICE_TYPE_POSTGRES,
        SERVICE_TYPE_POWERBI,
        SERVICE_TYPE_STORAGE
    };
}