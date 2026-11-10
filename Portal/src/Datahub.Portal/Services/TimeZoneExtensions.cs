using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbelt.Blazor.Globalization;

namespace Datahub.Portal.Services
{
    public static class TimeZoneExtensions
    {
        public static async Task<string?> GetLocalTimeString(this ILocalTimeZone localTimeZoneService, DateTime utcTime)
        {
            var localTimeZone = await localTimeZoneService.GetLocalTimeZoneAsync(null);
            if (localTimeZone is null)
            {
                return null;
            }
            var local = TimeZoneInfo.ConvertTimeFromUtc(utcTime, localTimeZone);
            return local.ToString("g"); // or your preferred format
        }

        public static async Task<DateTime> ConvertFromUTC(this ILocalTimeZone localTimeZoneService, DateTime utcTime)
        {
            var localTimeZone = await localTimeZoneService.GetLocalTimeZoneAsync(null);
            if (localTimeZone is null)
            {
                return utcTime; // If no local time zone, return the UTC time as is            
            }
            // Convert the last update time to the local timezone            
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, localTimeZone);
        }
        public static string GetTimeZoneShortCode(TimeZoneInfo timeZoneInfo)
        {
            // Use StandardName or DaylightName to derive the short code
            var name = timeZoneInfo.IsDaylightSavingTime(DateTime.UtcNow)
                ? timeZoneInfo.DaylightName
                : timeZoneInfo.StandardName;

            // Extract the first letter of each word
            return string.Concat(name.Split(' ').Select(word => word[0])).ToUpperInvariant();
        }

        public static string LocalizeTimeZone(string engTimeZone, bool isFrench)
        {
            if (isFrench)
            {
                switch (engTimeZone)
                {
                    case ("UTC"): // Universal time zone
                        return "UTC";
                    case ("PST"):
                        return "HNP";
                    case ("MST"):
                        return "HNR";
                    case ("CST"):
                        return "HNC";
                    case ("EST"):
                        return "HNE";
                    case ("AST"):
                        return "HNA";
                    case ("NST"):
                        return "HNT";
                    case ("PDT"): // Daylight timezones
                        return "HAP";
                    case ("MDT"):
                        return "HAR";
                    case ("CDT"):
                        return "HAC";
                    case ("EDT"):
                        return "HAE";
                    case ("ADT"):
                        return "HAA";
                    case ("NDT"):
                        return "HAT";
                    default:
                        return String.Empty;
                }
            }
            return engTimeZone;
        }
    }
}
