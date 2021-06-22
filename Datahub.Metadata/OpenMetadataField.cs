using CsvHelper.Configuration.Attributes;

namespace NRCan.Datahub.Metadata
{
    class OpenMetadataField
    {
        [Index(0)]
        public string FieldId { get; set; }
        [Index(1)]
        public string NameEnglish { get; set; }
        [Index(2)]
        public string NameFrench { get; set; }
        [Index(5)]
        public string DescriptionEnglish { get; set; }
        [Index(6)]
        public string DescriptionFrench { get; set; }
        [Index(15)]
        public string ObligationEnglish { get; set; }
        [Index(16)]
        public string ObligationFrench { get; set; }

    }
}
