using Datahub.Core.Model.Projects;

namespace Datahub.Portal.Pages.Tools.Statistics;

public class DatahubProjectStatsRow
{

    public int ProjectId { get; set; }
    public string Name { get; set; }
    public string Acronym { get; set; }
    
    public List<Datahub_Project_User> AllUsers { get; set; }
    public List<Project_Resources2> ProvisionedResources { get; set; }
    public string Department { get; set; }
    public DateTime? LastLogin { get; set; }

    public bool IsDeleted { get; set; }
    
    public decimal BudgetLimit { get; set; }
    public double BudgetSpent { get; set; }
    
    public decimal BudgetRemaining => BudgetLimit - (decimal) BudgetSpent;
    
    public decimal CostOfLastXDays { get; set; }
    
    public bool MetadataComplete { get; set; }
    
    public bool ShowUserDetails { get; set; }
    
    
    public enum ResourceStatus
    {
        None,
        Requested,
        Complete
    }
}