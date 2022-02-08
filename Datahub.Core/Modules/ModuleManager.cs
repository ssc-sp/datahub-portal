using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Modules
{
    public class ModuleManager
    {
        public List<Type> Modules { get; set; }

        public bool IsEnabled(string moduleName)
        {
            return Modules.Any(t => string.Equals(t.Name, moduleName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
