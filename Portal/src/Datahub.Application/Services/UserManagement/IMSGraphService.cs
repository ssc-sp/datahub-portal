using Datahub.Core.Data;

namespace Datahub.Application.Services.UserManagement;

public interface IMSGraphService
{
    Dictionary<string, GraphUser> UsersDict { get; set; }

    Task<GraphUser> GetUserAsync(string userId, CancellationToken token = default);
    Task<GraphUser> GetUserFromEmailAsync(string email, CancellationToken token);
    Task<Dictionary<string, GraphUser>> GetUsersListAsync(string filterText, CancellationToken token);
    Task<string> GetUserName(string userId, CancellationToken token = default);
    Task<string> GetUserEmail(string userId, CancellationToken token);
    Task<string> GetUserIdFromEmailAsync(string email, CancellationToken token);
    Task<GraphUser> GetUserFromSamAccountNameAsync(string userName, CancellationToken token);
}