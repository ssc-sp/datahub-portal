using Datahub.Core.Data;
using Microsoft.Graph;

namespace Datahub.Application.Services.UserManagement;

public interface IMSGraphService
{
    const string HttpClientName = "MSGraphClient";
    GraphServiceClient GetAuthenticatedClient();
    Task<GraphUser> GetUserAsync(string userId, CancellationToken token = default);
    Task<GraphUser> GetUserFromEmailAsync(string email, CancellationToken token);
    Task<Dictionary<string, GraphUser>> GetUsersListAsync(string filterText, CancellationToken token);
    Task<string> GetUserName(string userId, CancellationToken token = default);
    Task<string> GetUserEmail(string userId, CancellationToken token);
    Task<string> GetUserIdFromEmailAsync(string email, CancellationToken token);
    Task<GraphUser> GetUserFromSamAccountNameAsync(string userName, CancellationToken token);
}
