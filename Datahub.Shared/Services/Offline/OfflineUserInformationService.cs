using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NRCan.Datahub.Shared.Services
{
    public class OfflineUserInformationService : IUserInformationService
    {
        
        private ILogger<IUserInformationService> _logger;
        private GraphServiceClient graphServiceClient;
        

        public OfflineUserInformationService(ILogger<IUserInformationService> logger)
        {
            _logger = logger;
        }

        public async Task<User> GetCurrentUserAsync()
        {
            return await GetUserAsync();
        }

        public async Task<User> GetUserAsync()
        {
            return new User
            {
                DisplayName = "Offline User",
                Id = "1",
                UserPrincipalName = "me@me.com"
            };
        }

        public Task<string> GetUserEmailDomain()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserEmailPrefix()
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetUserIdString()
        {
            return "1";
        }

        public async Task<string> GetUserLanguage()
        {
            return "en-CA";
        }

        public async Task<string> GetUserRootFolder()
        {
            return "/";
        }

        public async Task<bool> HasUserAcceptedTAC()
        {
            return true;
        }

        public async Task<bool> IsFrench()
        {
            return false;
        }

        public Task<bool> RegisterUserTAC()
        {
            throw new NotImplementedException();
        }

        public Task<bool> RegisterUserLanguage(string language)
        {
            throw new NotImplementedException();
        }
        
        public bool SetLanguage(string language)
        {
            throw new NotImplementedException();
        }
    }
}
