using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Graph;
using Datahub.Core.Data;
using Datahub.Core.Services;
using System;
using Microsoft.AspNetCore.Components;
using Datahub.Portal.Services.Storage;

namespace Datahub.Portal.Services.Offline
{
    public class OfflineDataRetrievalService : DataRetrievalService
    {
        public OfflineDataRetrievalService(ILogger<DataRetrievalService> logger,
                            IKeyVaultService keyVaultService,
                            DataLakeClientService dataLakeClientService,
                            NavigationManager navigationManager,
                            UIControlsService uiService) : base(logger, keyVaultService,
                             dataLakeClientService,
                             navigationManager,
                             uiService)
        {
        }

  
    }
}
