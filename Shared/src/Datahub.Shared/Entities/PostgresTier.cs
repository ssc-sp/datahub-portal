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
        public string MaxIOPS { get; set; } = "N/A";
        public string MaxBandwidth { get; set; } = "N/A";
        public string Type { get; set; } = "N/A";
        public bool IsAvailable { get; set; }

        public static List<PostgresTier> GetPostgresTiers()
        {
            var postgresTiers = new List<PostgresTier>();
            postgresTiers.Add(new PostgresTier { SKUName = "B1ms", Cores = 1, MemorySize = "2 GiB", PSQL_SKU = "B_Standard_B1ms", MaxIOPS = "640", MaxBandwidth = "10 MiB/sec", Type = "Burstable", IsAvailable = true, Cost = "$12.41/month" });
            postgresTiers.Add(new PostgresTier { SKUName = "B2s", Cores = 2, MemorySize = "4 GiB", PSQL_SKU = "B_Standard_B2s", MaxIOPS = "1,280", MaxBandwidth = "15 MiB/sec", Type = "Burstable", IsAvailable = true, Cost = "$49.64/month" });
            postgresTiers.Add(new PostgresTier { SKUName = "B4ms", Cores = 4, MemorySize = "16 GiB", PSQL_SKU = "B_Standard_B4ms", MaxIOPS = "2,880", MaxBandwidth = "35 MiB/sec", Type = "Burstable", IsAvailable = true, Cost = "$198.56/month" });
            postgresTiers.Add(new PostgresTier { SKUName = "B8ms", Cores = 8, MemorySize = "32 GiB", PSQL_SKU = "B_Standard_B8ms", MaxIOPS = "4,320", MaxBandwidth = "50 MiB/sec", Type = "Burstable", IsAvailable = true, Cost = "$397.12/month" });
            postgresTiers.Add(new PostgresTier { SKUName = "B12ms", Cores = 12, MemorySize = "48 GiB", PSQL_SKU = "B_Standard_B12ms", MaxIOPS = "4,320", MaxBandwidth = "50 MiB/sec", Type = "Burstable", IsAvailable = true, Cost = "$595.68/month" });
            postgresTiers.Add(new PostgresTier { SKUName = "B16ms", Cores = 16, MemorySize = "64 GiB", PSQL_SKU = "B_Standard_B16ms", MaxIOPS = "4,320", MaxBandwidth = "50 MiB/sec", Type = "Burstable", IsAvailable = true, Cost = "$794.24/month" });
            postgresTiers.Add(new PostgresTier { SKUName = "B20ms", Cores = 20, MemorySize = "80 GiB", PSQL_SKU = "B_Standard_B20ms", MaxIOPS = "4,320", MaxBandwidth = "50 MiB/sec", Type = "Burstable", IsAvailable = true, Cost = "$992.80/month" });
            return postgresTiers;
        }
    }
}