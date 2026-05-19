using Azure.Core;
using System.Security.Claims;

namespace Datahub.Application.Services.Security;

public interface IUserTokenCredentialService
{

    public const string KEYVAULT_SERVICE = "vault";
    public const string STORAGE_SERVICE = "storage";

    Task<TokenCredential> GetTokenCredentialForUser(string service, string? token = null);

    Task<string> GetUserToken(ClaimsPrincipal claimsPrincipal, string service);
}
