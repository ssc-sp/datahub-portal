using System.ComponentModel.DataAnnotations;
using Elemental.Components;

namespace Datahub.Portal.Data.Forms.WebAnalytics;

public class WebAnalytics
{
    [Key]
    [AeFormIgnore]
    public int WebAnalyticsID { get; set; }
    [MaxLength(20)] public string Owner { get; set; }
    [Required][MaxLength(4000)] public string URL { get; set; }
    [MaxLength(1000)] public string MeasurementOriginal { get; set; }
    [MaxLength(100)] public string PageviewMeasurement { get; set; }
    [MaxLength(100)] public string DownloadMeasurement { get; set; }
    [MaxLength(100)] public string ClickoutMeasurement { get; set; }
    [MaxLength(4000)] public string ClickoutURL { get; set; }
    [MaxLength(100)] public string Pageview { get; set; }
    [MaxLength(100)] public string Downloads { get; set; }
    [MaxLength(100)] public string OutboundLink { get; set; }
    [Required] public int Indicator { get; set; }
    [Required][MaxLength(100)] public string Lang { get; set; }
    [MaxLength(100)] public string HTTPStatus { get; set; }
    [AeFormIgnore]
    public string LastUpdatedUserId { get; set; }

    [AeFormIgnore]
    public DateTime LastUpdatedDT { get; set; }

    [AeFormIgnore]
    [Timestamp]
    public byte[] Timestamp { get; set; }
}