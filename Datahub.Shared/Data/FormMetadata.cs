using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRCan.Datahub.Shared.Data
{
    public class FormMetadata
    {
        public string Header { get; set; }
        public string SubHeader { get; set; }

        //public List<Func<object, string>> AccessorFunctions { get; set; }

        public List<Func<T, string>> GetAccessorFunctions<T>()
        {
            return null;
        }
    }
}
