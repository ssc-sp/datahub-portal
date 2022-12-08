using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System;
using System.Security.Claims;
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

     

        public Task<User> GetCurrentGraphUserAsync()
        {
            return Task.FromResult(new User
            {
                DisplayName = "Offline User",
                Id = UserGuid.ToString(),
                UserPrincipalName = "me@me.com",
                Mail = "nabeel.bader@nrcan-rncan.gc.ca"
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

        public Task<User> GetAnonymousGraphUserAsync()
        {
            return Task.FromResult(AnonymousUser);
        }

        public Task<string> GetAzureUserID()
        {
            return Task.FromResult(UserGuid.ToString());
        }

        public Task<User> GetGraphUserAsync(string userId)
        {
            return Task.FromResult(new User
            {
                DisplayName = "Offline User random",
                Id = userId,
                UserPrincipalName = "me@me.com"
            });
        }

        public Task<bool> IsUserWithoutInitiatives()
        {
            return Task.FromResult(false);
        }

        public Task<bool> IsViewingAsGuest()
        {
            return Task.FromResult(false);
        }

        public Task SetViewingAsGuest(bool isGuest)
        {
            // do nothing
            return Task.Delay(0);
        }

        public Task<bool> IsViewingAsVisitor()
        {
            return Task.FromResult(false);
        }

        public Task SetViewingAsVisitor(bool isVisitor)
        {
            return Task.CompletedTask;
        }

        public Task<ClaimsPrincipal> GetAuthenticatedUser(bool forceReload = false)
        {
            return Task.FromResult(new ClaimsPrincipal());
        }

        public Task<bool> IsUserProjectAdmin(string projectAcronym)
        {
            return Task.FromResult(false);
        }

        public Task<bool> IsUserDatahubAdmin()
        {
            return Task.FromResult(false);
        }
    }
}