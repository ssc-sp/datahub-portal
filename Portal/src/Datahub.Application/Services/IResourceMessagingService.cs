using Datahub.Shared.Entities;

namespace Datahub.Application.Services;

public interface IResourceMessagingService
{
    public Task SendToTerraformQueue(WorkspaceDefinition project);
    
    public Task QueueRBACSync(WorkspaceDefinition workspaceDefinition);

    public Task<WorkspaceDefinition> CreateWorkspaceDefinition(string projectAcronym, string requestingUserEmail = "system-generated", string? cbrId = null);
}
