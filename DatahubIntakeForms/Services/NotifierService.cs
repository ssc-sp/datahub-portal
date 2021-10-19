using System;
using System.Threading.Tasks;

namespace Datahub.Portal.Services
{
    public class NotifierService
    {

        // Can be called from anywhere
        public async Task Update(string key, bool reloadData)
        {
            if (Notify != null)
            {
                await Notify.Invoke(key, reloadData);
            }
        }

        public event Func<string, bool, Task> Notify;
    }

}