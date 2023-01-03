using ResourceProvisioner.Domain.Entities;

namespace ResourceProvisioner.Application.Services;

public interface ITerraformService
{
    Task CopyTemplateAsync(DataHubTemplate template, Workspace workspace);
    Task ExtractVariables(DataHubTemplate template, Workspace workspace);
    Task ExtractBackendConfig(string workspaceAcronym);
}