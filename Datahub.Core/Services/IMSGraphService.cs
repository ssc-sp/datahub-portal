using System.Collections.Generic;
using System.Threading.Tasks;
using Datahub.Core.Data;

namespace Datahub.Core.Services
{
    public interface IMSGraphService
    {
        Dictionary<string, GraphUser> UsersDict { get; set; }

        Task<Dictionary<string, GraphUser>> GetUsersAsync();
        Dictionary<string, GraphUser> GetUsersList();
        Task LoadUsersAsync();
        
        GraphUser GetUser(string userId);
        string GetUserName(string userId);
        string GetUserEmail(string userId);
        string GetUserIdFromEmail(string email);
    }
}