using System.ComponentModel.DataAnnotations;

namespace Datahub.Portal.Model;

public class Notification
{
    [Key]
    public int NotificationId { get; set; }
    public int UserId { get; set; }
    public string Message { get; set; }
    public bool IsNew { get; set; }

}