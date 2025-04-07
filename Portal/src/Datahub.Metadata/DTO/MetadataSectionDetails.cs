using System.Collections.Generic;

namespace Datahub.Metadata.DTO
{
    public class MetadataSectionDetails
    {
        public string ListId { get; set; }
        public int SectionId { get; set; }
        public string English { get; set; }
        public string French { get; set; }
        public HashSet<int> Fields { get; set; }
        public override string ToString()
        {
            return string.IsNullOrEmpty(English) ? "" : $"{English} / {French}";
        }
    }
    public class MetadataSectionDetailsComparer : IEqualityComparer<MetadataSectionDetails>
    {
        public bool Equals(MetadataSectionDetails x, MetadataSectionDetails y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;
            return x.ListId == y.ListId;
        }

        public int GetHashCode(MetadataSectionDetails obj)
        {
            return obj.ListId.GetHashCode();
        }
    }
}