using Datahub.Core.Data.ResourceProvisioner;
using Datahub.Shared.Entities;

namespace Datahub.Application.Services;

public interface IResourceMessagingService
{
    public Task SendToTerraformQueue(CreateResourceData project);
    
    public Task SendToUserQueue(WorkspaceDefinition workspaceDefinition);
}