using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Datahub.Core.Model.Projects;

namespace Datahub.Core.Model.Datahub;

public class PowerBi_Workspace
{
    [Key]
    public Guid Workspace_ID { get; set; }

    public string Workspace_Name { get; set; }

    public bool Sandbox_Flag { get; set; }

    public int? Project_Id { get; set; }

    public Datahub_Project Project { get; set; }

    public IList<PowerBi_Report> Reports { get; set; }

    public IList<PowerBi_DataSet> Datasets { get; set; }
}

public record class PowerBi_WorkspaceDefinition(Guid WorkspaceId, string WorkspaceName, bool SandboxFlag, int? ProjectId);

public class PowerBi_Report
{
    [Key]
    public Guid Report_ID { get; set; }

    public string Report_Name { get; set; }

    public Guid Workspace_Id { get; set; }

    public PowerBi_Workspace Workspace { get; set; }

    [NotMapped]
    public bool IsExternalReportActive { get; set; }

    public bool InCatalog { get; set; }
}

public record class PowerBi_ReportDefinition(Guid ReportId, string ReportName, Guid WorkspaceId);

public class PowerBi_DataSet
{
    [Key]
    public Guid DataSet_ID { get; set; }

    public string DataSet_Name { get; set; }

    public Guid Workspace_Id { get; set; }

    public PowerBi_Workspace Workspace { get; set; }
}

public class ExternalPowerBiReport
{
    [Key]
    public int ExternalPowerBiReport_ID { get; set; }
    [StringLength(200)]
    public string RequestingUser { get; set; }
    public bool Is_Created { get; set; }
    public DateTime End_Date { get; set; }
    public string Token { get; set; }
    public string Url { get; set; }
    public Guid Report_ID { get; set; }

    public string Validation_Code { get; set; }
    public byte[] ValidationSalt { get; set; }

    [NotMapped]
    public string UnhashedValidationCode { get; set; }
    [NotMapped]
    public string ReportName { get; set; }
    public bool IsExpired => End_Date != DateTime.MinValue && DateTime.Now > End_Date;
}

public record class PowerBi_DataSetDefinition(Guid DataSetId, string DataSetName, Guid WorkspaceId);