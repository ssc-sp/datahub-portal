using System;

namespace Datahub.Core.Model.UserTracking;

public class UserSettings
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public DateTime? AcceptedDate { get; set; } 
    public string Language { get; set; }

}