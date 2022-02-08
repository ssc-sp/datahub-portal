using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Modules
{
    public class ModuleManager
    {
        public List<Type> Modules { get; private set; }

        public bool IsEnabled(string moduleName)
        {
            return Modules.Any(t => string.Equals(t.Name, moduleName, StringComparison.OrdinalIgnoreCase));
        }

        private static List<Type> TryFindInAssembly<T>(Assembly asm)
        {
            try
            {
                return asm.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(T))).ToList();
            }
            catch (Exception ex)
            {
                return new List<Type>();
            }
        }


        public void LoadModules(string filterString = "*")
        {
            var filters = filterString.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var allModules = AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm => TryFindInAssembly<IDatahubModule>(asm)).ToList();
            if (!filters.Contains("*"))
            {
                var moduleFilters = filters.Select(c => c.ToLowerInvariant()).ToHashSet();
                Modules = allModules.Where(t => moduleFilters.Contains(t.Name.ToLowerInvariant())).ToList();
            }
            else
            {
                Modules = allModules;
            }
        }
    }
}
