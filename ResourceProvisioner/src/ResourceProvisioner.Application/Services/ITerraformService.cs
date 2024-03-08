using Datahub.Shared.Entities;

namespace ResourceProvisioner.Application.Services;

public interface ITerraformService
{
	Task CopyTemplateAsync(TerraformTemplate template, TerraformWorkspace terraformWorkspace);
	Task ExtractVariables(TerraformTemplate template, TerraformWorkspace terraformWorkspace);
	Task ExtractBackendConfig(string workspaceAcronym);
	Task ExtractAllVariables(TerraformWorkspace terraformWorkspace);
}