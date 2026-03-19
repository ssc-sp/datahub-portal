using Datahub.Application.Services.Security;
using Datahub.Application.Configuration;
using Microsoft.AspNetCore.Components;
using Datahub.Infrastructure.Services.Storage;

namespace Datahub.Portal.Services.Offline;

public class OfflineDataRetrievalService : DataRetrievalService
{
    public OfflineDataRetrievalService(ILogger<DataRetrievalService> logger,
        IKeyVaultService keyVaultService,
        DatahubPortalConfiguration portalConfiguration,
        NavigationManager navigationManager) : base(logger, keyVaultService,
        null,
        portalConfiguration,
        navigationManager
        )
    {
    }

  
}