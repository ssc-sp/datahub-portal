using Azure.Core;
using Datahub.Application.Services.Security;
using System.Security.Claims;

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
