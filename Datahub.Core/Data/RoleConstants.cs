using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Data
{
    public static class RoleConstants
    {
        public const string DATAHUB_ADMIN_PROJECT = "DHPGLIST";
        public const string DATAHUB_ROLE_ADMIN = DATAHUB_ADMIN_PROJECT + "-admin";

        /// <summary>
        /// This role should never be assigned to a user, but is used to lock out elements,
        /// e.g. for an admin using guest mode.
        /// </summary>
        public const string LOCKED_OUT_ROLE = "lockedout";
    }
}
