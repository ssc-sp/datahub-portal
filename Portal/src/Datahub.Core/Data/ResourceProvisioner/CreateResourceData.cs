using System.Collections.Generic;
using Datahub.Shared.Entities;

namespace Datahub.Core.Data.ResourceProvisioner;

// ReSharper disable UnusedAutoPropertyAccessor.Global
public record CreateResourceData
{
    // ReSharper disable once MemberCanBePrivate.Global
    public List<TerraformTemplate> Templates { get; init; }


    public TerraformWorkspace Workspace { get; init; }

    public string RequestingUserEmail { get; init; }

    public static CreateResourceData NewProjectTemplate(string projectName, string acronym, string sector,
        string organization,
        string requestingUserEmail)
    {
        // TODO: Validation
        return new CreateResourceData(projectName, acronym, sector, organization, requestingUserEmail);
    }

    public static CreateResourceData ResourceRunTemplate(TerraformWorkspace workspace,
        List<TerraformTemplate> resourceTemplates, string requestingUserEmail)
    {
        // TODO: Validation
        return new CreateResourceData(workspace, resourceTemplates, requestingUserEmail);
    }

    private CreateResourceData(TerraformWorkspace workspace, List<TerraformTemplate> resourceTemplates,
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
        Templates = new List<TerraformTemplate> { TerraformTemplate.Default };
        Workspace = new TerraformWorkspace()
        {
            Name = projectName,
            Acronym = acronym,
            TerraformOrganization = new TerraformOrganization()
            {
                Code = sector,
                Name = organization,
            },

            Users = new List<TerraformUser>()
        };
        RequestingUserEmail = requestingUserEmail;
    }

    public CreateResourceData()
    {
    }
}