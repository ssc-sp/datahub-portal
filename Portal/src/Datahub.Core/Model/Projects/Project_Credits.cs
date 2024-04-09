namespace Datahub.Core.Model.Projects;

public class Project_Credits
{
    /// <summary>
    /// Gets or sets row key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets project table (FK)
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Gets or sets project instance
    /// </summary>
    public virtual Datahub_Project Project { get; set; }

    /// <summary>
    /// Gets or sets last updated (once a day)
    /// </summary>
    public DateTime? LastUpdate { get; set; }

    /// <summary>
    /// Gets or sets last rollover date
    /// </summary>
    public DateTime? LastRollover { get; set; }

    /// <summary>
    /// Gets or sets current credits consumed.
    /// </summary>
    public double Current { get; set; }

    /// <summary>
    /// Gets or sets current credits consumed, according to azure budget
    /// </summary>
    public double BudgetCurrentSpent { get; set; }

    /// <summary>
    /// Gets or sets jSON serialized cost per service (ServiceName, Cost)
    /// </summary>
    public string CurrentPerService { get; set; }

    /// <summary>
    /// Gets or sets jSON serialized cost per day (Date, Cost)
    /// </summary>
    public string CurrentPerDay { get; set; }

    /// <summary>
    /// Gets or sets current credits yesterday.
    /// </summary>
    public double YesterdayCredits { get; set; }

    /// <summary>
    /// Gets or sets jSON serialized cost per service yesterday (ServiceName, Cost)
    /// </summary>
    public string YesterdayPerService { get; set; }

    /// <summary>
    /// Gets or sets consumed notification date
    /// </summary>
    public DateTime? LastNotified { get; set; }

    /// <summary>
    /// Gets or sets consume notification percent
    /// </summary>
    public int? PercNotified { get; set; }
}
