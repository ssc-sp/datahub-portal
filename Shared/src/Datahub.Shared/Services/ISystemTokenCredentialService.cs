using Azure.Core;

namespace Datahub.Application.Services.Security;

public interface ISystemTokenCredentialService
{
    //portal
    //infra
    TokenCredential GetPortalTokenCredential();

    TokenCredential GetInfraTokenCredential();

}
