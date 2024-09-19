using System.Globalization;

namespace Datahub.Application.Services.UserManagement
{
    public interface ICultureService
    {
        const string French = "fr";
        const string English = "en";

        const string CaCultureSuffix = "CA";

        string CanadaEnglish => $"{English}-{CaCultureSuffix}";
        string CanadaFrench => $"{French}-{CaCultureSuffix}";

        CultureInfo CanadaEnglishCulture => CultureInfo.GetCultureInfo(CanadaEnglish);
        CultureInfo CanadaFrenchCulture => CultureInfo.GetCultureInfo(CanadaFrench);

        string Culture { get; }
        bool IsEnglish => Culture == English;
        bool IsFrench => Culture == French;

        ValueTask<string?> GetLanguageFromLocalStorageAsync();
        Task SetLanguageInLocalStorageAsync(string language);

        void OverrideCurrentCulture(string cultureName);
    }
}
