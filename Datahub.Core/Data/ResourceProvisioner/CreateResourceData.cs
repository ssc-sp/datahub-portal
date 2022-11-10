using System.Collections.Generic;

namespace Datahub.Core.Data.ResourceProvisioner;

public record CreateResourceData
{
    public List<ResourceTemplate> Templates { get; init; }
    public ResourceWorkspace Workspace { get; init; }   
}