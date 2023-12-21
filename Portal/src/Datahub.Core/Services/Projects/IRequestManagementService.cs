using Datahub.Metadata.DTO;
using Datahub.Metadata.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Datahub.Core.Model.Projects;
using Datahub.Shared.Entities;

namespace Datahub.Core.Services.Projects;

public record ProjectResourceFormParams(FieldDefinitions FieldDefinitions, MetadataProfile Profile);

public interface IRequestManagementService
{
    /// <summary>
    /// This method is used to handle the terraform request service, it takes in the project and the terraform template to run
    /// </summary>
    /// <param name="project"></param>
    /// <param name="terraformTemplate"></param>
    /// <returns>
    /// Returns true if the terraform request service was handled successfully, false otherwise
    /// </returns>
    Task<bool> HandleTerraformRequestServiceAsync(Datahub_Project project, string terraformTemplate);

    /// <summary>
    /// Handles user updates to external permissions for a given project.
    /// </summary>
    /// <param name="project">The datahub project.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleUserUpdatesToExternalPermissions(Datahub_Project project);
}