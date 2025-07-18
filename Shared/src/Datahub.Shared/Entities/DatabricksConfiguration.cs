using System;
using System.Collections.Generic;
using System.Text;

namespace Datahub.Shared.Entities;

public class DatabricksConfiguration
{
    public string GeneralPurposeTierSku { get; set; } = DatabricksTier.DefaultGeneralPurpose.DatabricksSKU;
    public string MachineLearningTierSku { get; set; } = null!;
    public string MachineLearningGpuTierSku { get; set; } = null!;

    public void ResetMLTier() => MachineLearningTierSku = EnableMachineLearning ? DatabricksTier.DefaultML.DatabricksSKU : null!;
    public void ResetMLGpuTier() => MachineLearningGpuTierSku = EnableMachineLearningGpu ? DatabricksTier.DefaultMLGpu.DatabricksSKU : null!;

    public bool EnableMachineLearning
    {
        get => _enableMachineLearning;
        set
        {
            _enableMachineLearning = value;
            ResetMLTier();
        }
    }
    public bool EnableMachineLearningGpu
    {
        get => _enableMachineLearningGpu;
        set
        {
            _enableMachineLearningGpu = value;
            ResetMLGpuTier();
        }
    }

    private bool _enableMachineLearning = false;
    private bool _enableMachineLearningGpu = false;

    private DatabricksTier LookupGeneralPurposeTier() => DatabricksTier.GetGeneralPurposeTiers().FirstOrDefault(t => t.DatabricksSKU == GeneralPurposeTierSku) ?? null!;
    private DatabricksTier LookupMachineLearningTier() => DatabricksTier.GetMachineLearningTiers().FirstOrDefault(t => t.DatabricksSKU == MachineLearningTierSku) ?? null!;
    private DatabricksTier LookupMachineLearningGpuTier() => DatabricksTier.GetMachineLearningGpuTiers().FirstOrDefault(t => t.DatabricksSKU == MachineLearningGpuTierSku) ?? null!;

    public (decimal Min, decimal Max) GetMinMaxSelectedHourlyCosts()
    {
        IEnumerable<DatabricksTier> selectedTiers = [
            LookupGeneralPurposeTier(),
            LookupMachineLearningTier(),
            LookupMachineLearningGpuTier()
        ];
        var nonNullTiers = selectedTiers.Where(t => t != null);

        var minCost = nonNullTiers.Min(t => t.HourlyCost ?? 0);
        // multiply max cost by 3 as per existing estimation logic
        var maxCost = nonNullTiers.Max(t => t.HourlyCost ?? 0) * 3;
        return (minCost, maxCost);
    }
}
