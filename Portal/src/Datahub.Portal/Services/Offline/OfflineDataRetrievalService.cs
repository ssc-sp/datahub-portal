using Datahub.Application.Services.Security;
using Microsoft.AspNetCore.Components;
using Datahub.Infrastructure.Services.Storage;

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