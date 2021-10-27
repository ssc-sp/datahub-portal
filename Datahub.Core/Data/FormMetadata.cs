using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Data
{
    public class FormMetadata<T>
    {
        public string Header { get; set; }
        public string SubHeader { get; set; }
        public string UserId { get; set; }

        public IList<T> DataSet { get; set; }

        public IList<Func<T, string>> AccessorFunctions { get; set; }

        public IList<string> Headers { get; set; }

        public string GridTemplateColumns { get; set; }

        public IList<string> MarkDownContent { get; set; }

        public IList<string> MarkDownContentFooter { get; set; }

        public IList<(Delegate Label, Delegate Choices)> FilterProperties { get; set; }

        public bool IsSubmitEnabled { get; set; }

        public string TableRoles { get; set; }
        public bool IsLoaded()
        {
            return Header != null && DataSet != null && AccessorFunctions != null && Headers != null && GridTemplateColumns != null;
        }
    }
}
