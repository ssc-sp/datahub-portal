using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Datahub.Portal.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datahub.Core.Components;

namespace Datahub.Core.UserTracking
{
    public record UserRecent
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

        public string Name { get; set; }

        public string Variant { get; set; }

        public string DatabricksURL { get; set; }

        public string WebFormsURL { get; set; }

        public string DataProject { get; set; }

        public string PBIReportId { get; set; }

        public DateTimeOffset accessedTime{ get; set; }
    }
}
