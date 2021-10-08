using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System.Threading.Tasks;

namespace Datahub.Shared.Services
{
    public class OfflineUserInformationService : IUserInformationService
    {
        readonly ILogger<IUserInformationService> _logger;

        public OfflineUserInformationService(ILogger<IUserInformationService> logger)
        {
            _logger = logger;
        }

     

        public Task<User> GetUserAsync()
        {
            return Task.FromResult(new User
            {
                DisplayName = "Offline User",
                Id = "1",
                UserPrincipalName = "me@me.com"
            });
        }

        public Task<string> GetUserEmailDomain()
        {
            return Task.FromResult("me@me.com");
        }

        public Task<string> GetUserEmailPrefix()
        {
            return Task.FromResult("");
        }

        public Task<string> GetUserIdString()
        {
            return Task.FromResult("1");
        }

        public Task<string> GetUserLanguage()
        {
            return Task.FromResult("en-CA");
        }

        public Task<string> GetUserRootFolder()
        {
            return Task.FromResult("/");
        }

        public Task<bool> HasUserAcceptedTAC()
        {
            return Task.FromResult(true);
        }

        public Task<bool> IsFrench()
        {
            return Task.FromResult(false);
        }

        public Task<bool> RegisterUserTAC()
        {
            return Task.FromResult(true);
        }

        public Task<bool> RegisterUserLanguage(string language)
        {
            return Task.FromResult(true);
        }
        
        public bool SetLanguage(string language)
        {
            return true;
        }
    }
}