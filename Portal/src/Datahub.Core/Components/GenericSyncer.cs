using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Components
{
    public class GenericSyncer
    {
        public event Action Notify;
        public event Func<Task> NotifyAsync;

        public void Ping() => Notify?.Invoke();
        public async Task PingAsync() => await NotifyAsync?.Invoke();
    }
}
