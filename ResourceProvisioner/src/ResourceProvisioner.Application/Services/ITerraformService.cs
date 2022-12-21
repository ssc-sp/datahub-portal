using ResourceProvisioner.Domain.Entities;

namespace ResourceProvisioner.Application.Services;

public interface ITerraformService
{
    Task CopyTemplateAsync(DataHubTemplate template, string workspaceAcronym);
    Task ExtractVariables(DataHubTemplate template, string workspaceAcronym);
    Task ExtractBackendConfig(string workspaceAcronym);
}