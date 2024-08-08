using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core
{
    public static class DevTools
    {
        public static bool IsDevelopment()
        {
            var whereAmI = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            return !string.IsNullOrEmpty(whereAmI) && whereAmI.ToLower() == "development";
        }
    }
}
