using Datahub.Core.Services;
using Microsoft.Graph.Models;

namespace Datahub.Portal.Services.Offline;

public class OfflineDataCreatorService : IDataCreatorService
{
    public OfflineDataCreatorService()
    {
    }

    public Task<bool> CreateFolder(Core.Data.Folder folder, Core.Data.Folder parent, User user)
    {
        return Task.FromResult(true);
    }

    public Task<bool> CreateRootFolderIfNotExist(string userId, string rootFolder)
    {
        return Task.FromResult(true);
    }
}