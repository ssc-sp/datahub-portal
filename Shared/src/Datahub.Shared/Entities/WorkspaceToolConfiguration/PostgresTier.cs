using System;
using System.Collections.Generic;
using System.Text;

namespace Datahub.Shared.Entities.WorkspaceToolConfiguration
{
    public class PostgresTier : ComputeTier
    {
        public string PSQL_SKU
        {
            get => TerraformSku;
        }
        public string MaxIOPS { get; set; } = "N/A";
        public string MaxBandwidth { get; set; } = "N/A";
        public decimal MonthlyCost { get; set; }

        public override string Cost => $"${MonthlyCost}/m";

        public static readonly PostgresTier DefaultTier = new() { SKUName = "B1ms", Cores = 1, MemorySize = "2 GiB", TerraformSku = "B_Standard_B1ms", MaxIOPS = "640", MaxBandwidth = "10 MiB/sec", Type = "Burstable", IsAvailable = true, MonthlyCost = 17.80m };

        public static List<PostgresTier> GetPostgresTiers()
        {
            var postgresTiers = new List<PostgresTier>
            {
                DefaultTier,
                new() { SKUName = "B2s", Cores = 2, MemorySize = "4 GiB", TerraformSku = "B_Standard_B2s", MaxIOPS = "1,280", MaxBandwidth = "15 MiB/sec", Type = "Burstable", IsAvailable = true, MonthlyCost = 71.20m },
                new() { SKUName = "B4ms", Cores = 4, MemorySize = "16 GiB", TerraformSku = "B_Standard_B4ms", MaxIOPS = "2,880", MaxBandwidth = "35 MiB/sec", Type = "Burstable", IsAvailable = true, MonthlyCost = 284.85m },
                new() { SKUName = "B8ms", Cores = 8, MemorySize = "32 GiB", TerraformSku = "B_Standard_B8ms", MaxIOPS = "4,320", MaxBandwidth = "50 MiB/sec", Type = "Burstable", IsAvailable = true, MonthlyCost = 569.69m },
                new() { SKUName = "B12ms", Cores = 12, MemorySize = "48 GiB", TerraformSku = "B_Standard_B12ms", MaxIOPS = "4,320", MaxBandwidth = "50 MiB/sec", Type = "Burstable", IsAvailable = true, MonthlyCost = 854.54m },
                new() { SKUName = "B16ms", Cores = 16, MemorySize = "64 GiB", TerraformSku = "B_Standard_B16ms", MaxIOPS = "4,320", MaxBandwidth = "50 MiB/sec", Type = "Burstable", IsAvailable = true, MonthlyCost = 1139.38m },
                new() { SKUName = "B20ms", Cores = 20, MemorySize = "80 GiB", TerraformSku = "B_Standard_B20ms", MaxIOPS = "4,320", MaxBandwidth = "50 MiB/sec", Type = "Burstable", IsAvailable = true, MonthlyCost = 1424.23m }
            };
            return postgresTiers;
        }
    }
}
