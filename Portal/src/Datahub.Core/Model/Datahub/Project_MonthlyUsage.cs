using System;
using System.ComponentModel.DataAnnotations;

namespace Datahub.Core.Model.Datahub;

public class Project_MonthlyUsage
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
    /// Last updated (expected once per day)
    /// </summary>
    public DateTime? LastUpdate { get; set; }
    /// <summary>
    /// Current credits consumed.
    /// </summary>
    public double CurrentCost { get; set; }
    /// <summary>
    /// JSON serialized cost per service (ServiceName, Cost)
    /// </summary>
    public string CurrentCostPerService { get; set; }
    /// <summary>
    /// JSON serialized cost per day (Date, Cost)
    /// </summary>
    public string CurrentCostPerDay { get; set; }
    /// <summary>
    /// Current credits yesterday.
    /// </summary>
    public double YesterdayCost { get; set; }
    /// <summary>
    /// JSON serialized cost per service yesterday (ServiceName, Cost)
    /// </summary>
    public string YesterdayCostPerService { get; set; }

    #region deprecated 
    
    /// <summary>
    /// Total storage capacity currently in used
    /// </summary>
    public long CurrentStorageUsage { get; set; }
    /// <summary>
    /// Storage capacity notification date
    /// </summary>
    public DateTime? StorageLastNotified { get; set; }
    /// <summary>
    /// Storage percent notified
    /// </summary>
    public int? StoragePercNotified { get; set; }

    #endregion

    /// <summary>
    /// Consume notification date
    /// </summary>
    public DateTime? ConsumeLastNotified { get; set; }

    /// <summary>
    /// Consume notification percent
    /// </summary>
    public int? ConsumePercNotified { get; set; }
}
