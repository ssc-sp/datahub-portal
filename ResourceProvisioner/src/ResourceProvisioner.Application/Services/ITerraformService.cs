using Datahub.Shared.Entities;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;

namespace ResourceProvisioner.Application.Services;

public interface ITerraformService
{
    Task CopyTemplateAsync(string templateName, WorkspaceDefinition workspaceDefinition);
    Task ExtractVariables(string templateName, WorkspaceDefinition command);
    Task ExtractBackendConfig(string workspaceAcronym);
    Task ExtractAllVariables(WorkspaceDefinition command);
    Task DeleteTemplateAsync(string templateName, TerraformWorkspace terraformWorkspace);
    Task DeleteWorkspaceAsync(TerraformWorkspace terraformWorkspace, string resourcegroup);
}