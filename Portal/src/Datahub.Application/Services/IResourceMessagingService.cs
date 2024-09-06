using Datahub.Shared.Entities;

namespace Datahub.Application.Services;

public interface IResourceMessagingService
{
    public Task SendToTerraformQueue(WorkspaceDefinition project);
    
    public Task SendToUserQueue(WorkspaceDefinition workspaceDefinition);
    
    public Task<WorkspaceDefinition> GetWorkspaceDefinition(string projectAcronym, string? requestingUserEmail = null);
}