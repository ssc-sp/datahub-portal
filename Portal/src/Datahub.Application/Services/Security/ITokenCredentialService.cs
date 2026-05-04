using Azure.Core;
using System.Security.Claims;

namespace Datahub.Application.Services.Security;

public interface ITokenCredentialService
{
    TokenCredential GetTokenCredential();

    Task<TokenCredential> GetTokenCredentialForUser(ClaimsPrincipal claimsPrincipal);
}
