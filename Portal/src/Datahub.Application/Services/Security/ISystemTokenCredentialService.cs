using Azure.Core;
using System.Security.Claims;

namespace Datahub.Application.Services.Security;

public interface ISystemTokenCredentialService
{
    //portal
    //infra
    TokenCredential GetPortalTokenCredential();

    TokenCredential GetInfraTokenCredential();

}
