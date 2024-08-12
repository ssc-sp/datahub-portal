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

        public enum TopBarEnvironments
        {
            Development,
            Integration,
            ProofOfConcept,
            Production,
            ProductionProtected
        }

        public static TopBarEnvironments TopBarEnvironment()
        {
            var whereAmI = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            return whereAmI switch
            {
                "Development" or "dev" => TopBarEnvironments.Development,
                "int" => TopBarEnvironments.Integration,
                "poc" => TopBarEnvironments.ProofOfConcept,
                "prod" => TopBarEnvironments.Production,
                "prot" => TopBarEnvironments.ProductionProtected,
                _ => TopBarEnvironments.Development
            };
        }
    }
}
