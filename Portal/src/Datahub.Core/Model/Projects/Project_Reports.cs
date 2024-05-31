namespace Datahub.Core.Model.Projects
{
    public class ProjectReports
    {
       public int Id { get; set; }
       public int ProjectId { get; set; }
       public virtual Datahub_Project Project { get; set; }
       public DateTime GeneratedDate { get; set; }
       public DateTime UpdatedDate { get; set; }
       public DateTime CoverageStartDate { get; set; }
       public DateTime CoverageEndDate { get; set; }
       public string GeneratedBy { get; set; }
       public int ReportType { get; set; }
       public int ReportStatus { get; set; }
       public string ReportName { get; set; }
       public string ReportUrl { get; set; }
    }
}