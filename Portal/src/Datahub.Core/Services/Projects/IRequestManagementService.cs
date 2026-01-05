using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Users;
using Datahub.Shared.Entities;

namespace Datahub.Core.Services.Projects;

public interface IRequestManagementService
{
    /// <summary>
    /// Handles a Terraform request service asynchronously.
    /// </summary>
    /// <param name="project">The project to handle the Terraform request for.</param>
    /// <param name="terraformTemplate">The Terraform template to use for the request.</param>
    /// <param name="requestingUser">The user requesting the Terraform request.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the Terraform request was handled successfully or not.</returns>
    Task<bool> HandleTerraformRequestServiceAsync(Datahub_Project project, TerraformTemplate terraformTemplate,
        PortalUser requestingUser);

    /// <summary>
    /// Handles user updates to external permissions for a specified Datahub project.
    /// </summary>
    /// <param name="project">The Datahub project for which the user updates the external permissions.</param>
    /// <param name="currentUser">The current portal user making the updates.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleUserUpdatesToExternalPermissions(Datahub_Project project, PortalUser currentUser);

    /// <summary>
    /// Scaffolds database changes required for the terraform template using the given context.
    /// </summary>
    /// <param name="project">The project to scaffold the changes for.</param>
    /// <param name="requestingUser">The current portal user making the request.</param>
    /// <param name="requestedTemplate">The terraform template to scaffold for.</param>
    /// <param name="ctx">The db context to use to scaffold the changes</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ScaffoldLocalChanges(Datahub_Project project, PortalUser requestingUser, TerraformTemplate requestedTemplate,
        DatahubProjectDBContext ctx);

    public Task<bool> TriggerBuildVersionUpdates(string versionTag, string email);
    public Task SendVersionUpdateToQueueAsync(string versionTag, WorkspaceDefinition workspaceDefinition);
}
