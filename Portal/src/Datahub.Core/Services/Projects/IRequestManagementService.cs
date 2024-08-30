using Datahub.Metadata.DTO;
using Datahub.Metadata.Model;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Projects;
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
}