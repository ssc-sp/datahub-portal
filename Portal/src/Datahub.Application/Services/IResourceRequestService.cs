using Datahub.Core.Data.ResourceProvisioner;

namespace Datahub.Application.Services;

public interface IResourceRequestService
{
    public Task AddProjectToStorageQueue(CreateResourceData project);
}