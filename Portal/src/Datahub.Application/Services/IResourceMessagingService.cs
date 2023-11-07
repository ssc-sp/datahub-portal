using Datahub.Core.Data.ResourceProvisioner;
using Datahub.Shared.Entities;

namespace Datahub.Application.Services;

public interface IResourceMessagingService
{
    public Task SendToTerraformQueue(WorkspaceDefinition project);
    
    public Task SendToTerraformDeleteQueue(WorkspaceDefinition project, int projectId);
    
    public Task SendToUserQueue(WorkspaceDefinition workspaceDefinition, string? connectionString = null, string? queueName = null);
    
    public Task<WorkspaceDefinition> GetWorkspaceDefinition(string projectAcronym, string? requestingUserEmail = null);
}