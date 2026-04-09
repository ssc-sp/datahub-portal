using Datahub.Application.Configuration;
using Microsoft.AspNetCore.Components;
using Datahub.Infrastructure.Services.Storage;

namespace Datahub.Portal.Services.Offline;

public class OfflineDataRetrievalService : DataRetrievalService
{
    public OfflineDataRetrievalService(ILogger<DataRetrievalService> logger,
        DatahubPortalConfiguration portalConfiguration,
        NavigationManager navigationManager) : base(logger, portalConfiguration, navigationManager)
    {
    }

  
}