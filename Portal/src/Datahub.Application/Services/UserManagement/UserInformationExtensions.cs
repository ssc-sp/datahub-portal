using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datahub.Core.Model.Users;

namespace Datahub.Application.Services.UserManagement
{
    public static class UserInformationExtensions
    {
        public static async Task<string> GetGenericUserId(this IUserInformationService userInformationService)
        {
            if (await userInformationService.IsEntraUser())
            {
                return $"{UserExtensions.ENTRA}-{await userInformationService.GetCurrentUserEntraId()}";
            }
            else if (await userInformationService.IsExternalUser())
            {
                return $"{UserExtensions.EXTERNAL}-{await userInformationService.GetExternalUserNameIdentifier()}";
            }
            throw new InvalidOperationException("Unknown user type");
        }
    }
}
