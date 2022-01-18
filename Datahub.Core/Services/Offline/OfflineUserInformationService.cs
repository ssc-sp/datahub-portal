using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public class OfflineUserInformationService : IUserInformationService
    {
        private static readonly Guid UserGuid = new Guid(); 

        readonly ILogger<IUserInformationService> _logger;

        private User AnonymousUser => UserInformationServiceConstants.GetAnonymousUser();

        public OfflineUserInformationService(ILogger<IUserInformationService> logger)
        {
            _logger = logger;
        }

     

        public Task<User> GetUserAsync()
        {
            return Task.FromResult(new User
            {
                DisplayName = "Offline User",
                Id = UserGuid.ToString(),
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
            return Task.FromResult(UserGuid.ToString());
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

        public Task<User> GetAnonymousUserAsync()
        {
            return Task.FromResult(AnonymousUser);
        }

        public Task<string> GetAzureUserID()
        {
            return Task.FromResult(UserGuid.ToString());
        }

        public Task<User> GetUserAsync(string userId)
        {
            return Task.FromResult(new User
            {
                DisplayName = "Offline User random",
                Id = userId,
                UserPrincipalName = "me@me.com"
            });
        }
    }
}