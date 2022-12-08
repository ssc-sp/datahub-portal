using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public class ExcelTextContentParser : IExcelContentParser
    {
        public String ValidMimeType { get; } = "text/plain";

        public Task<IList<String[]>> GetRows(String input) =>
            Task.FromResult<IList<String[]>>(input.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Select(x =>
                x.Split("\t").Select(y => y ?? String.Empty).ToArray()
            ).ToList());
    }
}
