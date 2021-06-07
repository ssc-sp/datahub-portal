using System;
using System.ComponentModel.DataAnnotations;
using Elemental.Components.Forms;

namespace NRCan.Datahub.Portal.Data.WebAnalytics
{
    public class WebAnalytics
    {
        [Key]
        [AeFormIgnore]
        public int WebAnalytics_ID { get; set; }
        [MaxLength(20)] public string Owner { get; set; }
        [Required] [MaxLength(4000)] public string URL { get; set; }
        [MaxLength(1000)] public string Measurement_Original { get; set; }
        [MaxLength(100)] public string Pageview_Measurement { get; set; }
        [MaxLength(100)] public string Download_Measurement { get; set; }
        [MaxLength(100)] public string Clickout_Measurement { get; set; }
        [MaxLength(4000)] public string Clickout_URL { get; set; }
        [MaxLength(100)] public string Pageview { get; set; }
        [MaxLength(100)] public string Downloads { get; set; }
        [MaxLength(100)] public string OutboundLink { get; set; }
        [Required] public int Indicator { get; set; }
        [Required] [MaxLength(100)] public string Lang { get; set; }
        [MaxLength(100)] public string HTTP_Status { get; set; }
        [AeFormIgnore]
        public string Last_Updated_UserId { get; set; }

        [AeFormIgnore]
        public DateTime Last_Updated_DT { get; set; }

        [AeFormIgnore]
        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}
