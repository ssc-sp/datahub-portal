using System;
using System.Collections.Generic;
using System.Text;

namespace Datahub.Shared.Entities.WorkspaceToolConfiguration;

public class DatabricksTier : ComputeTier
{
    public string DatabricksSKU
    {
        get => TerraformSku;
        set => TerraformSku = value;
    }

    public string GPUs { get; set; } = "N/A";

    public decimal? HourlyCost { get; set; } = null;

    public new string Cost => HourlyCost.HasValue ? $"{HourlyCost.Value:C2}/h" : "N/A";

    public static readonly DatabricksTier DefaultGeneralPurpose = new() { SKUName = "D4ds v5", Cores = 4, MemorySize = "16 GiB", DatabricksSKU = "Standard_D4ds_v5", Type = "General Purpose", IsAvailable = true, HourlyCost = 1.10m };

    public static readonly DatabricksTier DefaultML = new() { SKUName = "D4ds v5", Cores = 4, MemorySize = "16 GiB", DatabricksSKU = "Standard_D4ds_v5", Type = "Machine Learning", IsAvailable = true, HourlyCost = 1.10m };

    public static readonly DatabricksTier DefaultMLGpu = new() { SKUName = "NC4as T4 v3", Cores = 4, MemorySize = "28 GiB", DatabricksSKU = "Standard_NC4as_T4_v3", GPUs = "1 x T4", Type = "Machine Learning GPU", IsAvailable = true, HourlyCost = 1.60m };

    public static IEnumerable<DatabricksTier> GetGeneralPurposeTiers()
    {
        yield return DefaultGeneralPurpose;
        yield return new DatabricksTier { SKUName = "D8ds v5", Cores = 8, MemorySize = "32 GiB", DatabricksSKU = "Standard_D8ds_v5", Type = "General Purpose", IsAvailable = true, HourlyCost = 2.20m };
        yield return new DatabricksTier { SKUName = "D16ds v5", Cores = 16, MemorySize = "64 GiB", DatabricksSKU = "Standard_D16ds_v5", Type = "General Purpose", IsAvailable = true, HourlyCost = 4.40m };
        yield return new DatabricksTier { SKUName = "D32ds v5", Cores = 32, MemorySize = "128 GiB", DatabricksSKU = "Standard_D32ds_v5", Type = "General Purpose", IsAvailable = true, HourlyCost = 8.80m };
        yield return new DatabricksTier { SKUName = "D48ds v5", Cores = 48, MemorySize = "192 GiB", DatabricksSKU = "Standard_D48ds_v5", Type = "General Purpose", IsAvailable = true, HourlyCost = 13.10m };
        yield return new DatabricksTier { SKUName = "D64ds v5", Cores = 64, MemorySize = "256 GiB", DatabricksSKU = "Standard_D64ds_v5", Type = "General Purpose", IsAvailable = true, HourlyCost = 17.50m };
    }

    public static IEnumerable<DatabricksTier> GetMachineLearningTiers()
    {
        yield return DefaultML;
        yield return new DatabricksTier { SKUName = "D8ds v5", Cores = 8, MemorySize = "32 GiB", DatabricksSKU = "Standard_D8ds_v5", Type = "Machine Learning", IsAvailable = true, HourlyCost = 2.20m };
        yield return new DatabricksTier { SKUName = "D16ds v5", Cores = 16, MemorySize = "64 GiB", DatabricksSKU = "Standard_D16ds_v5", Type = "Machine Learning", IsAvailable = true, HourlyCost = 4.40m };
        yield return new DatabricksTier { SKUName = "D32ds v5", Cores = 32, MemorySize = "128 GiB", DatabricksSKU = "Standard_D32ds_v5", Type = "Machine Learning", IsAvailable = true, HourlyCost = 8.80m };
    }

    public static IEnumerable<DatabricksTier> GetMachineLearningGpuTiers()
    {
        yield return DefaultMLGpu;
        yield return new DatabricksTier { SKUName = "NC8as T4 v3", Cores = 8, MemorySize = "56 GiB", DatabricksSKU = "Standard_NC8as_T4_v3", GPUs = "1 x T4", Type = "Machine Learning GPU", IsAvailable = true, HourlyCost = 2.30m };
        yield return new DatabricksTier { SKUName = "NC16as T4 v3", Cores = 16, MemorySize = "110 GiB", DatabricksSKU = "Standard_NC16as_T4_v3", GPUs = "1 x T4", Type = "Machine Learning GPU", IsAvailable = true, HourlyCost = 3.70m };
        yield return new DatabricksTier { SKUName = "NC64as T4 v3", Cores = 64, MemorySize = "440 GiB", DatabricksSKU = "Standard_NC64as_T4_v3", GPUs = "4 x T4", Type = "Machine Learning GPU", IsAvailable = true, HourlyCost = 14.10m };
        yield return new DatabricksTier { SKUName = "NV36ads A10 v5", Cores = 36, MemorySize = "440 GiB", DatabricksSKU = "Standard_NV36ads_A10_v5", GPUs = "1 x A10", Type = "Machine Learning GPU", IsAvailable = true, HourlyCost = 8.30m };
    }
}
