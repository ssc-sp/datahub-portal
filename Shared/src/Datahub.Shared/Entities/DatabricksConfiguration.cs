using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Datahub.Shared.Entities;

public class DatabricksConfiguration
{
    public const string GENERAL_PURPOSE_TIER_CONFIG_JSON_KEY = "general_purpose_cluster";
    public const string MACHINE_LEARNING_TIER_CONFIG_JSON_KEY = TerraformVariables.MlCompute;
    public const string MACHINE_LEARNING_GPU_TIER_CONFIG_JSON_KEY = TerraformVariables.MlGpuCompute;
    public const string ENABLE_MACHINE_LEARNING_CONFIG_JSON_KEY = TerraformVariables.EnableMlCluster;
    public const string ENABLE_MACHINE_LEARNING_GPU_CONFIG_JSON_KEY = TerraformVariables.EnableMlGpuCluster;
    public static readonly Version MinimumConfigurableWorkspaceVersion = new(5, 2, 0);

    [JsonPropertyName(GENERAL_PURPOSE_TIER_CONFIG_JSON_KEY)]
    public string GeneralPurposeTierSku { get; set; } = DatabricksTier.DefaultGeneralPurpose.DatabricksSKU;

    [JsonPropertyName(MACHINE_LEARNING_TIER_CONFIG_JSON_KEY)]
    public string MachineLearningTierSku
    {
        get => EnableMachineLearning ? _machineLearningTierSku : null!;
        set => _machineLearningTierSku = value ?? DatabricksTier.DefaultML.DatabricksSKU;
    }

    [JsonPropertyName(MACHINE_LEARNING_GPU_TIER_CONFIG_JSON_KEY)]
    public string MachineLearningGpuTierSku
    {
        get => EnableMachineLearningGpu ? _machineLearningGpuTierSku : null!;
        set => _machineLearningGpuTierSku = value ?? DatabricksTier.DefaultMLGpu.DatabricksSKU;
    }

    public void ResetMLTier() => MachineLearningTierSku = EnableMachineLearning ? DatabricksTier.DefaultML.DatabricksSKU : null!;
    public void ResetMLGpuTier() => MachineLearningGpuTierSku = EnableMachineLearningGpu ? DatabricksTier.DefaultMLGpu.DatabricksSKU : null!;

    [JsonPropertyName(ENABLE_MACHINE_LEARNING_CONFIG_JSON_KEY)]
    public bool EnableMachineLearning { get; set; } = false;

    [JsonPropertyName(ENABLE_MACHINE_LEARNING_GPU_CONFIG_JSON_KEY)]
    public bool EnableMachineLearningGpu { get; set; } = false;

    private string _machineLearningTierSku = DatabricksTier.DefaultML.DatabricksSKU;
    private string _machineLearningGpuTierSku = DatabricksTier.DefaultMLGpu.DatabricksSKU;

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

    public DatabricksConfiguration Clone()
    {
        return new DatabricksConfiguration
        {
            GeneralPurposeTierSku = this.GeneralPurposeTierSku,
            MachineLearningTierSku = this.MachineLearningTierSku,
            MachineLearningGpuTierSku = this.MachineLearningGpuTierSku,
            EnableMachineLearning = this.EnableMachineLearning,
            EnableMachineLearningGpu = this.EnableMachineLearningGpu
        };
    }
}
