using Microsoft.Graph.Models;

namespace Datahub.Core.Services;

public interface IDataCreatorService
{
    Task<bool> CreateFolder(Core.Data.Folder folder, Core.Data.Folder parent, User user);
    Task<bool> CreateRootFolderIfNotExist(string userId, string rootFolder);
}