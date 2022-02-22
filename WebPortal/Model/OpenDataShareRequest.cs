using System;
using System.ComponentModel.DataAnnotations;

namespace Datahub.Portal.Model
{
    public class OpenDataShareRequest
    {
        [Required]
        public string FileURL { get; set; }
        [Required]
        public string CustomId { get; set; }
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
        public DateTime date_published { get; set; }
        [Required]
        public string jurisdiction { get; set; }
    }
}
