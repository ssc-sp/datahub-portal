using System.ComponentModel.DataAnnotations;

namespace Datahub.Core.Model.Users
{
    public class ExternalUser
    {
        [Key]
        public string? ExternalUserID { get; set; }

        public string? OID { get; set; }

        public DateTime? FirstLogin_DT { get; set; }

        public DateTime? LastLogin_DT { get; set; }

        public DateTime? LastPermissionsUpdated_DT { get; set; }

        public ICollection<ExternalUserInvite>? Requests { get; set; }

        public int? PortalUserId { get; set; }

        public required PortalUser PortalUser { get; set; } = null!;
    }
}