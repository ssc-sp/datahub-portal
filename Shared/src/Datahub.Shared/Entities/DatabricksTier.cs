using System;
using System.Collections.Generic;
using System.Text;

namespace Datahub.Shared.Entities;

public class DatabricksTier : ComputeTier
{
    public string DatabricksSKU
    {
        get => TerraformSku;
        set => TerraformSku = value;
    }
    public string GPUs { get; set; } = "N/A";

    public static readonly DatabricksTier DefaultGeneralPurpose = new() { SKUName = "D4ds v5", Cores = 4, MemorySize = "16 GiB", DatabricksSKU = "Standard_D4ds_v5", Cost = "TODO", Type = "General Purpose", IsAvailable = true };

    public static readonly DatabricksTier DefaultML = new() { SKUName = "D4ds v5", Cores = 4, MemorySize = "16 GiB", DatabricksSKU = "Standard_D4ds_v5", Cost = "TODO", Type = "Machine Learning", IsAvailable = true };

    public static readonly DatabricksTier DefaultMLGpu = new() { SKUName = "SNC4as T4 v3", Cores = 4, MemorySize = "28 GiB", DatabricksSKU = "Standard_NC4as_T4_v3", GPUs = "1 x T4", Cost = "TODO", Type = "Machine Learning GPU", IsAvailable = true };

    public static IEnumerable<DatabricksTier> GetGeneralPurposeTiers()
    {
        yield return DefaultGeneralPurpose;
        yield return new DatabricksTier { SKUName = "D8ds v5", Cores = 8, MemorySize = "32 GiB", DatabricksSKU = "Standard_D8ds_v5", Cost = "TODO", Type = "General Purpose", IsAvailable = true };
        yield return new DatabricksTier { SKUName = "D16ds v5", Cores = 16, MemorySize = "64 GiB", DatabricksSKU = "Standard_D16ds_v5", Cost = "TODO", Type = "General Purpose", IsAvailable = true };
        yield return new DatabricksTier { SKUName = "D32ds v5", Cores = 32, MemorySize = "128 GiB", DatabricksSKU = "Standard_D32ds_v5", Cost = "TODO", Type = "General Purpose", IsAvailable = true };
        yield return new DatabricksTier { SKUName = "D48ds v5", Cores = 48, MemorySize = "192 GiB", DatabricksSKU = "Standard_D48ds_v5", Cost = "TODO", Type = "General Purpose", IsAvailable = true };
        yield return new DatabricksTier { SKUName = "D64ds v5", Cores = 64, MemorySize = "256 GiB", DatabricksSKU = "Standard_D64ds_v5", Cost = "TODO", Type = "General Purpose", IsAvailable = true };
    }

    public static IEnumerable<DatabricksTier> GetMachineLearningTiers()
    {
        yield return DefaultML;
        yield return new DatabricksTier { SKUName = "D8ds v5", Cores = 8, MemorySize = "32 GiB", DatabricksSKU = "Standard_D8ds_v5", Cost = "TODO", Type = "Machine Learning", IsAvailable = true };
        yield return new DatabricksTier { SKUName = "D16ds v5", Cores = 16, MemorySize = "64 GiB", DatabricksSKU = "Standard_D16ds_v5", Cost = "TODO", Type = "Machine Learning", IsAvailable = true };
        yield return new DatabricksTier { SKUName = "D32ds v5", Cores = 32, MemorySize = "128 GiB", DatabricksSKU = "Standard_D32ds_v5", Cost = "TODO", Type = "Machine Learning", IsAvailable = true };
    }

    public static IEnumerable<DatabricksTier> GetMachineLearningGpuTiers()
    {
        yield return DefaultMLGpu;
        yield return new DatabricksTier { SKUName = "SNC8as T4 v3", Cores = 8, MemorySize = "56 GiB", DatabricksSKU = "Standard_NC8as_T4_v3", GPUs = "1 x T4", Cost = "TODO", Type = "Machine Learning GPU", IsAvailable = true };
        yield return new DatabricksTier { SKUName = "SNC16as T4 v3", Cores = 16, MemorySize = "110 GiB", DatabricksSKU = "Standard_NC16as_T4_v3", GPUs = "1 x T4", Cost = "TODO", Type = "Machine Learning GPU", IsAvailable = true };
        yield return new DatabricksTier { SKUName = "SNC64as T4 v3", Cores = 64, MemorySize = "440 GiB", DatabricksSKU = "Standard_NC64as_T4_v3", GPUs = "4 x T4", Cost = "TODO", Type = "Machine Learning GPU", IsAvailable = true };
        yield return new DatabricksTier { SKUName = "SNV36ads A10 v5", Cores = 36, MemorySize = "440 GiB", DatabricksSKU = "Standard_NV36ads_A10_v5", GPUs = "1 x A10", Cost = "TODO", Type = "Machine Learning GPU", IsAvailable = true };
    }
}
