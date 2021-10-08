using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Datahub.Core.EFCore
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
