using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Model.Users
{
    public class ExternalUser
    {
        [Key]
        public string ExternalUserID { get; set; }

        public string OID { get; set; }

        [Required]
        [StringLength(256)]
        public string Email { get; set; }

        [StringLength(128)]
        public string DisplayName { get; set; }

        public DateTime? FirstLogin_DT { get; set; }

        public DateTime? LastLogin_DT { get; set; }

        public DateTime? LastPermissionsUpdated_DT { get; set; }

        public ICollection<ExternalUserInvite> Requests { get; set; }
    }
}
