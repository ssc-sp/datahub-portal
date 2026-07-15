using Azure.Core;
using Datahub.Application.Services.Security;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Datahub.Functions.Services
{
    public class ServerUserTokenProviderService : IUserTokenCredentialService
    {
        public TokenCredential GetTokenCredentialForUser(UserTokenService service, string? token = null)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserToken(ClaimsPrincipal claimsPrincipal, UserTokenService service)
        {
            throw new NotImplementedException();
        }
    }
}
