using System;
using System.Collections.Generic;
using System.Text;

namespace Datahub.Shared.Entities;

public class DatabricksConfiguration
{
    public DatabricksTier GeneralPurposeTier { get; set; } = DatabricksTier.DefaultGeneralPurpose;
    public DatabricksTier MachineLearningTier { get; set; } = DatabricksTier.DefaultML;
    public DatabricksTier MachineLearningGpuTier { get; set; } = DatabricksTier.DefaultMLGpu;
    public bool EnableMachineLearning { get; set; } = false;
    public bool EnableMachineLearningGpu { get; set; } = false;
}
