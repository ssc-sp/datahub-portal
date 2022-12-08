using System;
using System.Globalization;

namespace Datahub.Metadata.Utils
{
    internal static class CultureUtils
    {
        public static string SelectCulture(string englishLabel, string frenchLabel)
        {
            var isFrench = CultureInfo.CurrentCulture.Name.StartsWith("fr", StringComparison.InvariantCulture);
            return isFrench ? SelectLabel(frenchLabel, englishLabel) : SelectLabel(englishLabel, frenchLabel);
        }

        static string SelectLabel(string value, string alt) => !string.IsNullOrWhiteSpace(value) ? value : alt;
    }
}
