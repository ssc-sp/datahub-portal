using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Datahub.Core.Model.Achievements;
using Newtonsoft.Json;

namespace Datahub.Core.Model.Projects;

public class ProjectResources2
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

    public DatahubProject Project { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedById { get; set; }
    public PortalUser UpdatedBy { get; set; }

    public string InputJsonContent { get; set; } = "{}";
}

public static class ProjectResourceConstants
{
    public const string STORAGETYPEBLOB = "blob";
    public const string STORAGETYPEGEN2 = "gen2";

    public const string SERVICETYPEPOSTGRES = "psql";
    public const string SERVICETYPESQLSERVER = "sql";
    public const string SERVICETYPESTORAGE = "storage";
    public const string SERVICETYPEDATABRICKS = "databricks";
    public const string SERVICETYPEPOWERBI = "powerbi";
    public const string SERVICETYPEVIRTUALMACHINE = "virtual-machine";

    public static readonly string[] ALLRESOURCETYPES = new[]
    {
        SERVICETYPEDATABRICKS,
        SERVICETYPESQLSERVER,
        SERVICETYPEPOSTGRES,
        SERVICETYPEPOWERBI,
        SERVICETYPESTORAGE
    };
}
