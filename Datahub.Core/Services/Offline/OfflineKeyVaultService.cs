using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public class OfflineKeyVaultService : IKeyVaultService
    {
        public Task<string> GetSecret(string secretName)
        {
            return Task.FromResult(string.Empty);
        }

        public Task<string> SignAsync(string data)
        {
            return Task.FromResult(data);
        }
    }
}
