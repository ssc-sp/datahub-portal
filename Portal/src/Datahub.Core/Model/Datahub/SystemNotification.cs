using System.ComponentModel.DataAnnotations;

namespace Datahub.Core.Model.Datahub;

public class SystemNotification
{
    [Key]
    public long NotificationID { get; set; }

    [Required]
    [StringLength(200)]
    public string ReceivingUserID { get; set; }

    [Required]
    public DateTime GeneratedTS { get; set; }

    public bool ReadFLAG { get; set; }

    [Required]
    public string NotificationTextEnTXT { get; set; }

    [Required]
    public string NotificationTextFrTXT { get; set; }

    [StringLength(512)]
    public string ActionLinkURL { get; set; }

    [StringLength(128)]
    public string ActionLinkKey { get; set; }
}