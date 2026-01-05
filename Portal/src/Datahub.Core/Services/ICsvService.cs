using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Services;

public interface ICsvService
{
    public interface ICsvRecord { }

    Stream GenerateCsvStreamFromRecords<TData>(IEnumerable<TData> records)
        where TData : ICsvRecord;

    Stream GenerateCsvStreamFromDynamicRecords(IEnumerable<dynamic> dynamicRecords);
}
