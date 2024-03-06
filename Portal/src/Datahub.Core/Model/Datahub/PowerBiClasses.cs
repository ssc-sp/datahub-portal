using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Datahub.Core.Model.Projects;

namespace Datahub.Core.Model.Datahub;

public class PowerBiWorkspace
{
    [Key]
    public Guid WorkspaceID { get; set; }

    public string WorkspaceName { get; set; }

    public bool SandboxFlag { get; set; }

    public int? ProjectId { get; set; }

    public DatahubProject Project { get; set; }

    public IList<PowerBiReport> Reports { get; set; }

    public IList<PowerBiDataSet> Datasets { get; set; }
}

public record class PowerBiWorkspaceDefinition(Guid WorkspaceId, string WorkspaceName, bool SandboxFlag, int? ProjectId);

public class PowerBiReport
{
    [Key]
    public Guid ReportID { get; set; }

    public string ReportName { get; set; }

    public Guid WorkspaceId { get; set; }

    public PowerBiWorkspace Workspace { get; set; }

    [NotMapped]
    public bool IsExternalReportActive { get; set; }

    public bool InCatalog { get; set; }
}

public record class PowerBiReportDefinition(Guid ReportId, string ReportName, Guid WorkspaceId);

public class PowerBiDataSet
{
    [Key]
    public Guid DataSetID { get; set; }

    public string DataSetName { get; set; }

    public Guid WorkspaceId { get; set; }

    public PowerBiWorkspace Workspace { get; set; }
}

public class ExternalPowerBiReport
{
    [Key]
    public int ExternalPowerBiReportID { get; set; }
    [StringLength(200)]
    public string RequestingUser { get; set; }
    public bool IsCreated { get; set; }
    public DateTime EndDate { get; set; }
    public string Token { get; set; }
    public string Url { get; set; }
    public Guid ReportID { get; set; }

    public string ValidationCode { get; set; }
    public byte[] ValidationSalt { get; set; }

    [NotMapped]
    public string UnhashedValidationCode { get; set; }
    [NotMapped]
    public string ReportName { get; set; }
    public bool IsExpired => EndDate != DateTime.MinValue && DateTime.Now > EndDate;
}

public record class PowerBiDataSetDefinition(Guid DataSetId, string DataSetName, Guid WorkspaceId);