using System;
using System.ComponentModel.DataAnnotations;

namespace Datahub.Portal.Model
{
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
        public string collection { get; set; }
        [Required]
        public string title_translated_en { get; set; }
        [Required]
        public string title_translated_fr { get; set; }
        [Required]
        public string notes_translated_en { get; set; }
        [Required]
        public string notes_translated_fr { get; set; }
        [Required]
        public string keywords_en { get; set; }
        [Required]
        public string keywords_fr { get; set; }
        [Required]
        public string subject { get; set; }
        [Required]
        public string frequency { get; set; }
        [Required]
        public DateTime? date_published { get; set; }
        [Required]
        public string jurisdiction { get; set; }
        // not required fields
        public DateTime? time_period_coverage_start { get; set; }
        public DateTime? time_period_coverage_end { get; set; }
        public string audience { get; set; }
        public string digital_object_identifier { get; set; }
    }
}
