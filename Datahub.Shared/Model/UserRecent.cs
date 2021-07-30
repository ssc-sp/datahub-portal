using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using NRCan.Datahub.Portal.Components;
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
        public ICollection<UserRecentLink> UserRecentActions{ get; set; } = new List<UserRecentLink>();
        
    }
    [Owned]
    public record UserRecentLink
    { 
        [Key]
        public Guid UserRecentActionId { get; set; }

        public DatahubLinkType LinkType { get; set; }

        public string PowerBIURL { get; set; }

        public string Variant { get; set; }

        public string DatabricksURL { get; set; }

        public string WebFormsURL { get; set; }

        public string DataProject { get; set; }
        public DateTimeOffset accessedTime{ get; set; }
    }
}
