using System;
using System.ComponentModel.DataAnnotations;

namespace Datahub.Core.UserTracking
{
    public class UserSettings
    {
        [Key]
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime? AcceptedDate { get; set; } 
        public string Language { get; set; }

    }
}
