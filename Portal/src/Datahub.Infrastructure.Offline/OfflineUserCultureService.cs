using Blazored.LocalStorage;
using Datahub.Application.Services.UserManagement;
using System.Globalization;

namespace Datahub.Infrastructure.Offline
{
    public class OfflineUserCultureService(ILocalStorageService localStorageService) : ICultureService
    {
        private const string LANGUAGE_LOCALSTORAGE_KEY = "language";

        private readonly ILocalStorageService _localStorageService = localStorageService;

        public string Culture => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();

        public async ValueTask<string?> GetLanguageFromLocalStorageAsync()
        {
            var language = await _localStorageService.GetItemAsStringAsync(LANGUAGE_LOCALSTORAGE_KEY);
            return language;
        }

        public void OverrideCurrentCulture(string cultureName)
        {
            var newCulture = CultureInfo.GetCultureInfo(cultureName);
            Thread.CurrentThread.CurrentCulture = newCulture;
            Thread.CurrentThread.CurrentUICulture = newCulture;
        }

        public async Task SetLanguageInLocalStorageAsync(string language)
        {
            await _localStorageService.SetItemAsStringAsync(LANGUAGE_LOCALSTORAGE_KEY, language);
        }
    }
}
