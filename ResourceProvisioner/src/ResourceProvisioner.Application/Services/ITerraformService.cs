using Datahub.Shared.Entities;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;

namespace ResourceProvisioner.Application.Services;

public interface ITerraformService
{
    Task CopyTemplateAsync(string templateName, TerraformWorkspace terraformWorkspace);
    Task ExtractVariables(string templateName, CreateResourceRunCommand command);
    Task ExtractBackendConfig(string workspaceAcronym);
    Task ExtractAllVariables(CreateResourceRunCommand command);
    Task DeleteTemplateAsync(string templateName, TerraformWorkspace terraformWorkspace);
    Task DeleteWorkspaceAsync(TerraformWorkspace terraformWorkspace, string resourcegroup);
}