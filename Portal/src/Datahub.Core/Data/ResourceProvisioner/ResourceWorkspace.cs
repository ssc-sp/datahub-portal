using System.Collections.Generic;

namespace Datahub.Core.Data.ResourceProvisioner;

// ReSharper disable UnusedAutoPropertyAccessor.Global
public record ResourceWorkspace
{
    public string Name { get; set; }
    public string Acronym { get; set; }
    
    public WorkspaceOrganization Organization { get; set; }
    
    public List<WorkspaceUser> Users { get; init; }
};