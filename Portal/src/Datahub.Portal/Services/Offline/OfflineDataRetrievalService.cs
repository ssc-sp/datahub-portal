using Datahub.Core.Services;
using Microsoft.AspNetCore.Components;
using Datahub.Core.Services.Security;
using Datahub.Core.Services.Storage;

namespace Datahub.Portal.Services.Offline;

public class OfflineDataRetrievalService : DataRetrievalService
{
    public OfflineDataRetrievalService(ILogger<DataRetrievalService> logger,
        IKeyVaultService keyVaultService,                            
        NavigationManager navigationManager) : base(logger, keyVaultService,
        null,
        navigationManager
        )
    {
    }

  
}