using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NRCan.Datahub.Portal.Services.Offline
{
    public class OfflineKeyVaultService : IKeyVaultService
    {
        public Task<string> GetSecret(string secretName)
        {
            return Task.FromResult(string.Empty);
        }
    }
}
