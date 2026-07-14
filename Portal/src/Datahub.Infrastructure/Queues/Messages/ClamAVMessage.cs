using System;
using System.Text.Json.Serialization;

namespace Datahub.Infrastructure.Queues.Messages;

public class ClamAVBlobMetadata
{
    [JsonPropertyName("createdby")]
    public string CreatedBy { get; set; } = string.Empty;

    [JsonPropertyName("fileid")]
    public string FileId { get; set; } = string.Empty;

    [JsonPropertyName("uploadBatchId")]
    public string UploadBatchId { get; set; } = string.Empty;
}

public class ClamAVMessage
{
    [JsonPropertyName("ScanStartTime")]
    public DateTime ScanStartTime { get; set; }

    [JsonPropertyName("ScanEndTime")]
    public DateTime ScanEndTime { get; set; }

    [JsonPropertyName("ScanError")]
    public string ScanError { get; set; } = string.Empty;

    [JsonPropertyName("ScannedFile")]
    public string ScannedFile { get; set; } = string.Empty;

    [JsonPropertyName("OriginalBlobMetadata")]
    public ClamAVBlobMetadata OriginalBlobMetadata { get; set; } = new();
}
