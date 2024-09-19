﻿using Blazored.LocalStorage;
using Datahub.Application.Services.UserManagement;
using System.Globalization;

namespace Datahub.Infrastructure.Services.UserManagement
{
    public class UserCultureService(ILocalStorageService localStorageService) : ICultureService
    {
        private const string LANGUAGE_LOCALSTORAGE_KEY = "language";

        private readonly ILocalStorageService _localStorageService = localStorageService;

        public string Culture => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();

        public async ValueTask<string?> GetLanguageFromLocalStorageAsync()
        {
            var language = await _localStorageService.GetItemAsStringAsync(LANGUAGE_LOCALSTORAGE_KEY);
            return language;
        }

        public async Task SetLanguageInLocalStorageAsync(string language)
        {
            await _localStorageService.SetItemAsStringAsync(LANGUAGE_LOCALSTORAGE_KEY, language);
        }
    }
}
