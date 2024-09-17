using Datahub.Application.Services.UserManagement;
using System.Globalization;

namespace Datahub.Infrastructure.Offline
{
    public class OfflineUserCultureService : ICultureService
    {
        public string Culture => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
    }
}
