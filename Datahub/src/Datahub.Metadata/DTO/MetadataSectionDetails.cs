using System.Collections.Generic;

namespace Datahub.Metadata.DTO
{
    public class MetadataSectionDetails
    {
        public string ListId {  get; set; }
        public int SectionId { get; set; }
        public string English { get; set; }
        public string French { get; set; }
        public HashSet<int> Fields { get; set; }
    }
}
