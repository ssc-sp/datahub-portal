﻿using Datahub.Core.Services;
using Microsoft.AspNetCore.Components;
using Datahub.Portal.Services.Storage;

namespace Datahub.Portal.Services.Offline
{
    public class OfflineDataRetrievalService : DataRetrievalService
    {
        public OfflineDataRetrievalService(ILogger<DataRetrievalService> logger,
                            IKeyVaultService keyVaultService,                            
                            NavigationManager navigationManager,
                            UIControlsService uiService) : base(logger, keyVaultService,
                             null,
                             navigationManager,
                             uiService)
        {
        }

  
    }
}
