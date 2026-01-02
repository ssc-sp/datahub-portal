using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Model.Users
{
    public class EntraUser
    {
        /// <summary>
        /// Gets or sets the unique identifier of this user.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user's unique Graph identifier.
        /// </summary>
        public required string GraphGuid { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the associated portal user.
        /// </summary>
        public int? PortalUserId { get; set; }

        /// <summary>
        /// Gets or sets the associated portal user entity.
        /// </summary>
        public required PortalUser PortalUser { get; set; } = null!;

        /// <summary>
        /// Gets or sets the timestamp for concurrency control.
        /// </summary>
        public byte[]? Timestamp { get; set; }
    }
}
