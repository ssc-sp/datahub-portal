using System.Threading.Tasks;
using NRCan.Datahub.Shared.Data;

namespace NRCan.Datahub.Shared.Services
{
    public interface IDataCreatorService
    {
        Task<bool> CreateFolder(Folder folder, Folder parent, Microsoft.Graph.User user);
        Task<bool> CreateRootFolderIfNotExist(string userId, string rootFolder);
    }
}