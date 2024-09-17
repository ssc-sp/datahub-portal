using Datahub.Application.Services.UserManagement;
using System.Globalization;

namespace Datahub.Infrastructure.Services.UserManagement
{
    public class UserCultureService : ICultureService
    {
        public string Culture => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
    }
}
