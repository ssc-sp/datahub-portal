using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{ 
    public interface IExcelContentParser
    {
        String ValidMimeType { get; }

        public Boolean CanParse(String mimeType) => ValidMimeType.Equals(mimeType, StringComparison.InvariantCultureIgnoreCase);
        Task<IList<String[]>> GetRows(String input);
    }
}
