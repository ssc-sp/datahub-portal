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
    }
}
