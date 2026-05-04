using Azure.Core;
using System.Security.Claims;

namespace Datahub.Application.Services.Security;

public interface IUserTokenCredentialService
{

    Task<TokenCredential> GetTokenCredentialForUser(string vaultToken);

    Task<string> GetUserToken(ClaimsPrincipal claimsPrincipal);
}
