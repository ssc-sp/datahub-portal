using System;
using System.Collections.Generic;
using System.Text;

namespace Datahub.Shared.Entities.WorkspaceToolConfiguration;

public abstract class ComputeTier
{
    public required string SKUName { get; set; }
    public required string TerraformSku { get; set; }
    public required int Cores { get; set; }
    public required string MemorySize { get; set; }
    public abstract string Cost { get; }
    public required string Type { get; set; } = "N/A";
    public bool IsAvailable { get; set; } = true;
}
