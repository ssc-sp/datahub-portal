using System.Collections.Generic;

namespace Datahub.Core.Data.ResourceProvisioner;
// ReSharper disable UnusedAutoPropertyAccessor.Global
public record CreateResourceData
{
    public List<ResourceTemplate> Templates { get; init; }
    

    public ResourceWorkspace Workspace { get; init; }
    
}