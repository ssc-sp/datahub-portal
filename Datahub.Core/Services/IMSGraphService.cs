using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Datahub.Core.Data;
using Microsoft.Graph;

namespace Datahub.Core.Services
{
    public interface IMSGraphService
    {
        Dictionary<string, GraphUser> UsersDict { get; set; }

        Task<GraphUser> GetUserAsync(string userId, CancellationToken tkn);
        Task<Dictionary<string, GraphUser>> GetUsersListAsync(string filterText, CancellationToken tkn);
        Task<string> GetUserName(string userId, CancellationToken tkn = default);
        Task<string> GetUserEmail(string userId, CancellationToken tkn);
        Task<string> GetUserIdFromEmailAsync(string email, CancellationToken tkn);
    }
}