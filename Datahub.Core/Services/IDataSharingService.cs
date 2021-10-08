using System.Threading.Tasks;
using Datahub.Core.Data;

namespace Datahub.Portal.Services
{
    public interface IDataSharingService
    {
        Task<bool> AddSharedUsers(FileMetaData file, string sharedUserId, string role);
        Task<bool> ChangeFileOwner(FileMetaData file, GraphUser newOwner, string currentUserId);
        Task LoadSharedUsers(FileMetaData file);
        Task<bool> RemoveSharedUsers(FileMetaData file, string sharedUserId);
    }
}