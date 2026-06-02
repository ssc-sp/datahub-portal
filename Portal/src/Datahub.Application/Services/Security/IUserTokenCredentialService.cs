using Azure.Core;
using System.Security.Claims;

namespace Datahub.Application.Services.Security;

public enum UserTokenService
{
    KeyVault,
    Storage
}

public interface IUserTokenCredentialService
{
    Task<TokenCredential> GetTokenCredentialForUser(UserTokenService service, string? token = null);

    Task<string> GetUserToken(ClaimsPrincipal claimsPrincipal, UserTokenService service);
}
