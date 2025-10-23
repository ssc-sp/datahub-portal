using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Datahub.Core.Model.Achievements;

namespace Datahub.Core.Model.Datahub;

/// <summary>
/// Represents the status of a blocklist entry
/// </summary>
public enum BlocklistStatus
{
    /// <summary>
    /// Entry is active and blocks publishing access
    /// </summary>
    Active = 1,

    /// <summary>
    /// Entry has been deleted/removed and no longer blocks access
    /// </summary>
    Deleted = -1
}

/// <summary>
/// Represents a blocklist entry that restricts access to the Open Government Publishing feature.
/// Entries can block based on department name or Email Domain.
/// </summary>
public class OpenGovPublishingBlocklist
{
    /// <summary>
    /// Gets or sets the unique identifier for the blocklist entry
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the department name that is blocked (optional, if blocking by department)
    /// </summary>
    [MaxLength(500)]
    public string DepartmentName { get; set; }

    /// <summary>
    /// Gets or sets the Email Domain that is blocked (e.g., "example.gc.ca")
    /// Used to block users whose email domain matches this hostname
    /// </summary>
    [MaxLength(200)]
    public string EmailHostname { get; set; }

    /// <summary>
    /// Gets or sets the status of this blocklist entry
    /// </summary>
    [Required]
    public BlocklistStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this entry was added to the blocklist
    /// </summary>
    [Required]
    public DateTime DateAdded { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this entry was removed/deleted from the blocklist (nullable)
    /// </summary>
    public DateTime? DateRemoved { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who added this blocklist entry
    /// </summary>
    [Required]
    public int AddedByUserId { get; set; }

    /// <summary>
    /// Gets or sets the user who added this blocklist entry
    /// </summary>
    [ForeignKey(nameof(AddedByUserId))]
    public PortalUser AddedByUser { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who removed this blocklist entry (nullable)
    /// </summary>
    public int? RemovedByUserId { get; set; }

    /// <summary>
    /// Gets or sets the user who removed this blocklist entry (nullable)
    /// </summary>
    [ForeignKey(nameof(RemovedByUserId))]
    public PortalUser RemovedByUser { get; set; }

    /// <summary>
    /// Gets or sets optional notes or reason for blocking
    /// </summary>
    [MaxLength(2000)]
    public string Notes { get; set; }
}
