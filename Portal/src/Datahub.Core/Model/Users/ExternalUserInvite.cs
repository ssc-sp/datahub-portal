using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Datahub.Core.Model.Users
{
    public class ExternalUserInvite
    {
        [Key]
        public int RequestID { get; set; }

        [Required]
        public string UserOID { get; set; }

        [ForeignKey("UserOID")]
        public ExternalUser User { get; set; }

        public DateTimeOffset Request_DT { get; set; }

        [Required]
        public string RequestContent { get; set; }
    }
}
