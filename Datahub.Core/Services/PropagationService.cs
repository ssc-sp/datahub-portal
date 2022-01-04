using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public class PropagationService : IPropagationService
    {
        public event Func<IEnumerable<string>, Task> UpdateSystemNotifications;

        public async Task PropagateSystemNotificationUpdate(IEnumerable<string> userIds)
        {
            if (UpdateSystemNotifications != null)
            {
                await UpdateSystemNotifications.Invoke(userIds);
            }
        }
    }
}
