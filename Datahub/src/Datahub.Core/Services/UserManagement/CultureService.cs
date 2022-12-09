using System.Globalization;

namespace Datahub.Core.Services.UserManagement;

public class CultureService
{
    public string Culture => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
    public bool IsEnglish => Culture == English;
    public bool IsFrench => Culture == French;

    public const string French = "fr";
    public const string English = "en";
}