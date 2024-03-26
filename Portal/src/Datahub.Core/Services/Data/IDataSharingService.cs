using Datahub.Core.Data;

namespace Datahub.Core.Services.Data;

public interface IDataSharingService
{
    Task<bool> AddSharedUsers(FileMetaData file, string sharedUserId, string role);
    Task<bool> ChangeFileOwner(FileMetaData file, GraphUser newOwner, string currentUserId);
    Task LoadSharedUsers(FileMetaData file);
    Task<bool> RemoveSharedUsers(FileMetaData file, string sharedUserId);
}