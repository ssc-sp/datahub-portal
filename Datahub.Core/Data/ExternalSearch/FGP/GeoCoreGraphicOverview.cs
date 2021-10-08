using System.Collections.Generic;

namespace Datahub.Core.Data.External.FGP
{
    public class GeoCoreGraphicOverview
    {
        // typo in original
        public string OverviewFileTupe { get; set; }
        public string OverviewFilename { get; set; }
        public string OverviewFileDescription { get; set; }
    }

    public class GeoCoreGraphicOverviewList : List<GeoCoreGraphicOverview> { }
}