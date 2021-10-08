using System.Threading.Tasks;
using Datahub.Shared.Data;

namespace Datahub.Shared.Services
{
    public interface IDataCreatorService
    {
        Task<bool> CreateFolder(Folder folder, Folder parent, Microsoft.Graph.User user);
        Task<bool> CreateRootFolderIfNotExist(string userId, string rootFolder);
    }
}