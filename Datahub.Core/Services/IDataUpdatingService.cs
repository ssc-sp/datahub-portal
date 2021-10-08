using System.Threading.Tasks;
using Microsoft.Graph;
using Datahub.Shared.Data;

namespace Datahub.Shared.Services
{
    public interface IDataUpdatingService
    {
        Task<bool> MoveFile(FileMetaData file, string newParentFolder, User currentUser);
        Task<bool> RenameFile(FileMetaData file, string newFileName, User currentUser);
        Task<bool> RenameFolder(Shared.Data.Folder folder, string newFolderName, User currentUser);
    }
}