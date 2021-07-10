using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRCan.Datahub.Shared.EFCore
{
    public class UserRecent
    {
        [Key]
        public string UserId { get; set; }
        public ICollection<UserRecentActions> UserRecentActions{ get; set; } = new List<UserRecentActions>();
        
    }
    [Owned]
    public class UserRecentActions
    { 
        [Key]
        public Guid UserRecentActionId { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string icon { get; set; }
        public DateTimeOffset accessedTime{ get; set; }
    }
}
