using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRCan.Datahub.Shared.Data
{
    public class FormMetadata<T>
    {
        public string Header { get; set; }
        public string SubHeader { get; set; }

        public IList<T> DataSet { get; set; }

        public IList<Func<T, string>> AccessorFunctions { get; set; }

        public IList<string> Headers { get; set; }

        public string GridTemplateColumns { get; set; }

        //public Type GetClassType()
        //{
        //    return typeof(T);
        //}
    }
}
