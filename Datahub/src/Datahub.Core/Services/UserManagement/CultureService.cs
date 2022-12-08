﻿using System.Globalization;

namespace Datahub.Core.Services.UserManagement;

public class CultureService
{
    public string Culture => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
    public bool IsEnglish => Culture == "en";
    public bool IsFrench => Culture == "fr";
}