using Datahub.Shared.Entities;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;

namespace ResourceProvisioner.Application.Services;

/// <summary>
/// Provides Terraform workspace provisioning and cleanup operations.
/// </summary>
public interface ITerraformService
{
    /// <summary>
    /// Copies the specified Terraform template into the target workspace definition.
    /// </summary>
    /// <param name="templateName">The Terraform template to copy.</param>
    /// <param name="workspaceDefinition">The workspace definition that defines the target environment and configuration.</param>
    /// <returns>A task that completes when the template has been copied.</returns>
    Task CopyTemplateAsync(string templateName, WorkspaceDefinition workspaceDefinition);

    /// <summary>
    /// Extracts the Terraform variables for the specified template.
    /// </summary>
    /// <param name="templateName">The name of the Terraform template being processed.</param>
    /// <param name="command">The workspace definition used to generate template variables.</param>
    /// <returns>A task that completes when the variables have been extracted.</returns>
    Task ExtractVariables(string templateName, WorkspaceDefinition command);

    /// <summary>
    /// Extracts the backend configuration required to initialize Terraform for the workspace.
    /// </summary>
    /// <param name="workspaceAcronym">The unique acronym or identifier of the workspace.</param>
    /// <returns>A task that completes when the backend configuration has been extracted.</returns>
    Task ExtractBackendConfig(string workspaceAcronym);

    /// <summary>
    /// Extracts all Terraform variables required for the workspace definition.
    /// </summary>
    /// <param name="command">The workspace definition that contains all variable values.</param>
    /// <returns>A task that completes when all variables have been extracted.</returns>
    Task ExtractAllVariables(WorkspaceDefinition command);

    /// <summary>
    /// Removes the specified Terraform template from the workspace.
    /// </summary>
    /// <param name="templateName">The template to remove from the workspace.</param>
    /// <param name="terraformWorkspace">The Terraform workspace to update.</param>
    /// <returns>A task that completes when the template has been deleted.</returns>
    Task DeleteTemplateAsync(string templateName, TerraformWorkspace terraformWorkspace);

    /// <summary>
    /// Deletes the Terraform workspace and its associated resource group.
    /// </summary>
    /// <param name="terraformWorkspace">The Terraform workspace to delete.</param>
    /// <param name="resourcegroup">The Azure resource group that should be removed as part of the workspace cleanup.</param>
    /// <returns>A task that completes when the workspace has been deleted.</returns>
    Task DeleteWorkspaceAsync(TerraformWorkspace terraformWorkspace, string resourcegroup);
}
