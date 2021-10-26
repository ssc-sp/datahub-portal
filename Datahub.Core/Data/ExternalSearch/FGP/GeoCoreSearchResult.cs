using System.Collections.Generic;

namespace Datahub.Core.Data.External.FGP
{
    public class GeoCoreSearchResult
    {
        public double QueryCostInUSD { get; set; }
        public int EngineExecutionTimeInMillis { get; set; }
        public int Count { get; set; }
        public int DataScannedInMB { get; set; }
        public IList<GeoCoreItem> Items { get; set; }
    }
}