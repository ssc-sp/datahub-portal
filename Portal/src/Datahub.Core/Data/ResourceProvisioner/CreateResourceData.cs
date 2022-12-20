using System.Collections.Generic;

namespace Datahub.Core.Data.ResourceProvisioner;

// ReSharper disable UnusedAutoPropertyAccessor.Global
public record CreateResourceData
{
    // ReSharper disable once MemberCanBePrivate.Global
    public List<ResourceTemplate> Templates { get; init; }


    public ResourceWorkspace Workspace { get; init; }

    public string RequestingUserEmail { get; init; }

    public static CreateResourceData NewProjectTemplate(string projectName, string acronym, string sector,
        string organization,
        string requestingUserEmail)
    {
        // TODO: Validation
        return new CreateResourceData(projectName, acronym, sector, organization, requestingUserEmail);
    }

    public static CreateResourceData ResourceRunTemplate(ResourceWorkspace workspace,
        List<ResourceTemplate> resourceTemplates, string requestingUserEmail)
    {
        // TODO: Validation
        return new CreateResourceData(workspace, resourceTemplates, requestingUserEmail);
    }

    private CreateResourceData(ResourceWorkspace workspace, List<ResourceTemplate> resourceTemplates,
        string requestingUserEmail)
    {
        Workspace = workspace;
        Templates = resourceTemplates;
        RequestingUserEmail = requestingUserEmail;
    }

    /// <summary>
    /// New Project Template that is used for the new project pull request
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="acronym"></param>
    /// <param name="sector"></param>
    /// <param name="organization"></param>
    /// <param name="requestingUserEmail"></param>
    private CreateResourceData(string projectName, string acronym, string sector, string organization,
        string requestingUserEmail)
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
        };
        RequestingUserEmail = requestingUserEmail;
    }

    public CreateResourceData()
    {
    }
}