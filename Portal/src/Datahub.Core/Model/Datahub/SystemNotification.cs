using System.ComponentModel.DataAnnotations;

namespace Datahub.Core.Model.Datahub;

/// <summary>
/// Represents a system-generated notification with its associated metadata.
/// </summary>
public class SystemNotification
{
    /// <summary>
    /// Gets or sets a unique identifier for the notification record.
    /// </summary>
    [Key]
    public long Notification_ID { get; set; }

    /// <summary>
    /// Gets or sets the user who should receive the notification.
    /// </summary>
    [Required]
    [StringLength(200)]
    public required string ReceivingUser_ID { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the notification was generated.
    /// </summary>
    [Required]
    public DateTime Generated_TS { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the notification has been read.
    /// </summary>
    public bool Read_FLAG { get; set; }

    /// <summary>
    /// Gets or sets the text of the notification in English.
    /// </summary>
    [Required]
    public required string NotificationTextEn_TXT { get; set; }

    /// <summary>
    /// Gets or sets the text of the notification in French.
    /// </summary>
    [Required]
    public required string NotificationTextFr_TXT { get; set; }

    /// <summary>
    /// Gets or sets the URI to an action or resource related to this notification.
    /// </summary>
    [StringLength(512)]
    public string? ActionLink_URL { get; set; }

    /// <summary>
    /// Gets or sets a key representing the action or link context.
    /// </summary>
    [StringLength(128)]
    public string? ActionLink_Key { get; set; }
}
