using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.EFCore
{
    public class MiscStoredObject
    {
        [Key]
        public Guid GeneratedId { get; set; }

        public string Id { get; set; }
        public string TypeName { get; set; }
        
        [Required]
        public string JsonContent { get; set; }
    }
}
