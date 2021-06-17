using CsvHelper.Configuration.Attributes;

namespace Datahub.Metadata
{
    class CsvMetadataField
    {
        [Index(0)]
        public string FieldId { get; set; }
        [Index(1)]
        public string LabelEnglish { get; set; }
        [Index(2)]
        public string LabelFrench { get; set; }
        [Index(5)]
        public string DescriptionEnglish { get; set; }
        [Index(6)]
        public string DescriptionFrench { get; set; }
        [Index(15)]
        public string Obligation { get; set; }
    }
}
