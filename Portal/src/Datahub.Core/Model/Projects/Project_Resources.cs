using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Datahub.Core.Model.Achievements;
using Newtonsoft.Json;

namespace Datahub.Core.Model.Projects;

public class Project_Resources2
{
    [Key]
    public Guid ResourceId { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(200)]
    public string ResourceType { get; set; }

    [Required]
    [StringLength(200)]
    public string ClassName { get; set; } = "legacy";

    public string JsonContent { get; set; } = "{}";

    [ForeignKey("Project")]
    public int ProjectId { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public int RequestedById { get; set; }
    public PortalUser RequestedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public Datahub_Project Project { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedById { get; set; }
    public PortalUser UpdatedBy { get; set; }

    public string InputJsonContent { get; set; } = "{}";
}



public static class ProjectResourceConstants
{
    public const string SERVICE_TYPE_POSTGRES = "psql";
    public const string SERVICE_TYPE_SQL_SERVER = "sql";
    public const string SERVICE_TYPE_STORAGE = "storage";
    public const string SERVICE_TYPE_DATABRICKS = "databricks";
    public const string SERVICE_TYPE_POWERBI = "powerbi";
    public const string SERVICE_TYPE_VIRTUAL_MACHINE = "virtual-machine";

    public static readonly string[] ALL_RESOURCE_TYPES = new[]
    {
        SERVICE_TYPE_DATABRICKS,
        SERVICE_TYPE_SQL_SERVER,
        SERVICE_TYPE_POSTGRES,
        SERVICE_TYPE_POWERBI,
        SERVICE_TYPE_STORAGE
    };

    public const string STORAGE_TYPE_BLOB = "blob";
    public const string STORAGE_TYPE_GEN2 = "gen2";
}
