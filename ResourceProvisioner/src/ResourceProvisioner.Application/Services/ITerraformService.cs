using Datahub.Shared.Entities;

namespace ResourceProvisioner.Application.Services;

public interface ITerraformService
{
    Task CopyTemplateAsync(string templateName, TerraformWorkspace terraformWorkspace);
    Task ExtractVariables(string templateName, TerraformWorkspace terraformWorkspace);
    Task ExtractBackendConfig(string workspaceAcronym);
    Task ExtractAllVariables(TerraformWorkspace terraformWorkspace);
    Task DeleteTemplateAsync(string templateName, TerraformWorkspace terraformWorkspace);
}