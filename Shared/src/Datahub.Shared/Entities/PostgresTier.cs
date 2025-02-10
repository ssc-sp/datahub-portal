using System;
using System.Collections.Generic;
using System.Text;

namespace Datahub.Shared.Entities
{
    public class PostgresTier
    {
        public string SKUName { get; set; }
        public int Cores { get; set; }
        public string MemorySize { get; set; }
        public string PSQL_SKU { get; set; }

        public string Cost { get; set; }

        public bool IsAvailable { get; set; }

        public static List<PostgresTier> GetPostgresTiers()
        {
            var postgresTiers = new List<PostgresTier>();
            postgresTiers.Add(new PostgresTier { SKUName = "B1ms (default)", Cores = 1, MemorySize = "2GiB", PSQL_SKU = "B_Standard_B1ms", Cost = "$12.41/month", IsAvailable = true });
            postgresTiers.Add(new PostgresTier { SKUName = "B2s", Cores = 2, MemorySize = "4GiB", PSQL_SKU = "B_Standard_B2s", Cost = "$49.64/month", IsAvailable = true });
            postgresTiers.Add(new PostgresTier { SKUName = "B4ms", Cores = 4, MemorySize = "16GiB", PSQL_SKU = "B_Standard_B4ms", Cost = "$198.56/month", IsAvailable = true });
            postgresTiers.Add(new PostgresTier { SKUName = "B8ms", Cores = 8, MemorySize = "32GiB", PSQL_SKU = "B_Standard_B8ms", Cost = "$397.12/month", IsAvailable = true });

            return postgresTiers;
        }
    }
}
