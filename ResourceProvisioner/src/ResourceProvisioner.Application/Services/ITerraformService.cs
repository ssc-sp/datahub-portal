using Datahub.Shared;
using ResourceProvisioner.Domain.Entities;

namespace ResourceProvisioner.Application.Services;

public interface ITerraformService
{
    Task CopyTemplateAsync(DataHubTemplate template, TerraformWorkspace terraformWorkspace);
    Task ExtractVariables(DataHubTemplate template, TerraformWorkspace terraformWorkspace);
    Task ExtractBackendConfig(string workspaceAcronym);
}