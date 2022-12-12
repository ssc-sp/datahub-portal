using System.Collections.Generic;

namespace Datahub.Core.Data.ResourceProvisioner;
// ReSharper disable UnusedAutoPropertyAccessor.Global
public record CreateResourceData
{
    
    // ReSharper disable once MemberCanBePrivate.Global
    public List<ResourceTemplate> Templates { get; init; }
    

    public ResourceWorkspace Workspace { get; init; }

    public CreateResourceData(string projectName, string acronym, string sector, string organization,
        string userEmail, string userId)
    {
        Templates = new List<ResourceTemplate>() { ResourceTemplate.Default };
        Workspace = new ResourceWorkspace()
        {
            Name = projectName,
            Acronym = acronym,
            Organization = new WorkspaceOrganization()
            {
                Code = sector,
                Name = organization,
            },

            Users = new List<WorkspaceUser>()
            {
                new WorkspaceUser()
                {
                    Email = userEmail,
                    Guid = userId,
                }
            }
        };
    }
    
    public CreateResourceData(){}

}