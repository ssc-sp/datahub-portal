namespace Datahub.Portal.Pages.Tools.Statistics;

public class DatahubProjectStatsRow
{

    public int ProjectId { get; set; }
    public string Name { get; set; }
    public string Acronym { get; set; }
    
    public List<string> Leads { get; set; }
    public List<string> Admins { get; set; }
    public List<string> Collaborators { get; set; }
    
    public List<string> UniqueDepartments => 
        Leads
            .Select(x => x?.Split('@')[1] ?? string.Empty)
            .Union(Admins
                .Select(x => x?.Split('@')[1] ?? string.Empty))
            .Union(Collaborators
                .Select(x => x?.Split('@')[1] ?? string.Empty))
            .ToList();
    
    public decimal BudgetLimit { get; set; }
    public double BudgetSpent { get; set; }
    
    public decimal BudgetRemaining => BudgetLimit - (decimal) BudgetSpent;
    
    public decimal CostOfLastXDays { get; set; }
    
    public bool MetadataComplete { get; set; }
    
    public ResourceStatus StorageStatus { get; set; }
    public ResourceStatus DatabricksStatus { get; set; }
    
    public bool ShowUserDetails { get; set; }
    
    
    public enum ResourceStatus
    {
        None,
        Requested,
        Complete
    }
}