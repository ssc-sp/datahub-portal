using Microsoft.Graph;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public interface IUserInformationService
    {
        Task<User> GetUserAsync();
        Task<string> GetUserIdString();
        Task<bool> HasUserAcceptedTAC();
        Task<bool> RegisterUserTAC();
        Task<bool> RegisterUserLanguage(string language);
        Task<string> GetUserLanguage();
        Task<bool> IsFrench();        
        Task<string> GetUserEmailDomain();
        Task<string> GetUserEmailPrefix();
        bool SetLanguage(string language);
        Task<string> GetUserRootFolder();        
    }
}
