using System;
using System.Collections.Generic;
using System.Text;

namespace Datahub.Shared.Entities;

public class ComputeTier
{
    public string SKUName { get; set; }
    public string TerraformSku { get; set; }
    public int Cores { get; set; }
    public string MemorySize { get; set; }
    public string Cost { get; set; }
    public string Type { get; set; } = "N/A";
    public bool IsAvailable { get; set; }
}
