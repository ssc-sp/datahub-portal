using System.Threading.Tasks;
using Microsoft.Graph;
using Datahub.Core.Data;

namespace Datahub.Core.Services
{
    public interface IDataUpdatingService
    {
        Task<bool> MoveFile(FileMetaData file, string newParentFolder, User currentUser);
        Task<bool> RenameFile(FileMetaData file, string newFileName, User currentUser);
        Task<bool> RenameFolder(Core.Data.Folder folder, string newFolderName, User currentUser);
    }
}