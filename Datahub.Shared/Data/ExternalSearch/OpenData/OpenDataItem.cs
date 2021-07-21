using System;

namespace NRCan.Datahub.Shared.Data.External.OpenData
{
    public class OpenDataItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public BilingualText Title_Translated { get; set; }
        public OpenDataOrganization Organization { get; set; }
        public BilingualText Org_Title_At_Publication { get; set; }
        public BilingualText Notes_Translated { get; set; }
        public string Notes { get; set; }
    }
}