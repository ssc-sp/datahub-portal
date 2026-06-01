using Azure.Core;

namespace Datahub.Application.Services.Security;

public interface ISystemTokenCredentialService
{
    TokenCredential GetTokenCredential();
}

public static class SystemTokenCredentialServiceKeys
{
    public const string Infra = "infra";
}
