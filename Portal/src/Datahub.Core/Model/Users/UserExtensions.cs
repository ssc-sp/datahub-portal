using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Model.Users
{
    public static class UserExtensions
    {
        public static string? UserUID(this PortalUser user)
        {
            if (user.EntraUser?.GraphGuid != null)
            {
                return user.EntraUser.GraphGuid;
            }
            else if (user.ExternalUser?.OID != null)
            {
                return user.ExternalUser.OID;
            }
            return null;
        }
    }
}
