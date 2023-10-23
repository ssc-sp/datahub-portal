using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Markdown
{
    public static class MarkdownTools
    {
        public static string GetIDFromString(string url)
        {
            return GetStableHashCode(url).ToString("X").ToLowerInvariant();
        }

        public static bool CompareCulture(string c1, string c2)
        {
            var ci1 = CultureInfo.GetCultureInfo(c1);
            var ci2 = CultureInfo.GetCultureInfo(c2);
            return ci1.Equals(ci2) || ci1.Parent.Equals(ci2) || ci2.Parent.Equals(ci1);
        }

        static long GetStableHashCode(string str)
        {
            unchecked
            {
                long hash1 = 5381;
                long hash2 = hash1;

                for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1 || str[i + 1] == '\0')
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
    }
}
