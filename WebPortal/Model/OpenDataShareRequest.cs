using System;
using System.ComponentModel.DataAnnotations;

namespace Datahub.Portal.Model
{
    public class MetadataField : Attribute
    {
    }

    public class OpenDataShareRequest
    {
        [Required]
        public string file_url { get; set; }
        [Required]
        public string file_id { get; set; }
        [Required]
        public string file_name { get; set; }
        [Required]
        public string email_contact { get; set; }
        [Required]
        [MetadataField]
        public string collection { get; set; }
        [Required]
        [MetadataField]
        public string title_translated_en { get; set; }
        [Required]
        [MetadataField]
        public string title_translated_fr { get; set; }
        [Required]
        [MetadataField]
        public string notes_translated_en { get; set; }
        [Required]
        [MetadataField]
        public string notes_translated_fr { get; set; }
        [Required]
        [MetadataField]
        public string keywords_en { get; set; }
        [Required]
        [MetadataField]
        public string keywords_fr { get; set; }
        [Required]
        [MetadataField]
        public string subject { get; set; }
        [Required]
        [MetadataField]
        public string frequency { get; set; }
        [Required]
        [MetadataField]
        public string date_published { get; set; }
        [Required]
        [MetadataField]
        public string jurisdiction { get; set; }

        [MetadataField]
        public string time_period_coverage_start { get; set; }
        [MetadataField]
        public string time_period_coverage_end { get; set; }
        [MetadataField]
        public string audience { get; set; }
        [MetadataField]
        public string digital_object_identifier { get; set; }
    }
}
