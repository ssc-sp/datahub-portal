using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public interface IMiscStorageService
    {
        Task SaveObject<T>(T obj, string id);
        Task SaveObjects<T>(IEnumerable<T> objects, Func<T, string> idGenerator);
        Task<T> GetObject<T>(string id);
        Task<IEnumerable<T>> GetAllObjects<T>();
    }
}
