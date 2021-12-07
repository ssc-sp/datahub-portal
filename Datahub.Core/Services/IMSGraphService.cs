using System.Collections.Generic;
using System.Threading.Tasks;
using Datahub.Core.Data;
using Microsoft.Graph;

namespace Datahub.Core.Services
{
    public interface IMSGraphService
    {
        Dictionary<string, GraphUser> UsersDict { get; set; }

        Task<Dictionary<string, GraphUser>> GetUsersAsync();
        Dictionary<string, GraphUser> GetUsersList();
        Task LoadUsersAsync();
                
        Task<GraphUser> GetUserAsync(string userId);
        string GetUserName(string userId);
        Task<string> GetUserEmail(string userId);
        Task<string> GetUserIdFromEmailAsync(string email);
    }
}