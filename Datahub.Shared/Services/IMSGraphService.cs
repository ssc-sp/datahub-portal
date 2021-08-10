using System.Collections.Generic;
using System.Threading.Tasks;
using NRCan.Datahub.Shared.Data;

namespace NRCan.Datahub.Shared.Services
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
    }
}