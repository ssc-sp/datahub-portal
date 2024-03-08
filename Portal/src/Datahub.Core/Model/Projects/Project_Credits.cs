namespace Datahub.Core.Model.Projects;

public class Project_Credits
{
    /// <summary>
    /// Row key
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Project table (FK)
    /// </summary>
    public int ProjectId { get; set; }
    /// <summary>
    /// Project instance
    /// </summary>
    public virtual Datahub_Project Project { get; set; }
    /// <summary>
    /// Last updated (once a day)
    /// </summary>
    public DateTime? LastUpdate { get; set; }
    /// <summary>
    /// Current credits consumed.
    /// </summary>
    public double Current { get; set; }
    /// <summary>
    /// JSON serialized cost per service (ServiceName, Cost)
    /// </summary>
    public string CurrentPerService { get; set; }
    /// <summary>
    /// JSON serialized cost per day (Date, Cost)
    /// </summary>
    public string CurrentPerDay { get; set; }
    /// <summary>
    /// Current credits yesterday.
    /// </summary>
    public double YesterdayCredits { get; set; }
    /// <summary>
    /// JSON serialized cost per service yesterday (ServiceName, Cost)
    /// </summary>
    public string YesterdayPerService { get; set; }
    /// <summary>
    /// Consumed notification date
    /// </summary>
    public DateTime? LastNotified { get; set; }
    /// <summary>
    /// Consume notification percent
    /// </summary>
    public int? PercNotified { get; set; }
}
