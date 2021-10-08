using System.Collections;
using System.Collections.Generic;

namespace Datahub.Shared.Data.External.FGP
{
    public class GeoCoreOption
    {
        public string Url { get; set; }
        public BilingualText Description { get; set; }
        public string Protocol { get; set; }
        public BilingualText Name { get; set; }
    }

    public class GeoCoreOptionsList : List<GeoCoreOption> { }
}