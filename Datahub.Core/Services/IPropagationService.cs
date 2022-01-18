using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public interface IPropagationService
    {
        event Func<IEnumerable<string>, Task> UpdateSystemNotifications;
        Task PropagateSystemNotificationUpdate(IEnumerable<string> userIds);
    }
}
