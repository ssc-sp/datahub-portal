using Azure.Core;
using System.Security.Claims;

namespace Datahub.Application.Services.Security;

public enum UserTokenService
{
    KeyVault,
    Storage,
    Graph
}

public interface IUserTokenCredentialService
{
    TokenCredential GetTokenCredentialForUser(UserTokenService service, string? token = null);

    Task<string> GetUserToken(ClaimsPrincipal claimsPrincipal, UserTokenService service);
}
