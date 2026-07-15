using System;
using System.Text.Json.Serialization;

namespace Datahub.Infrastructure.Queues.Messages;

public class ClamAVBlobMetadata
{
    public const string CreatedByTag = "createdby";
    public const string FileIdTag = "fileid";
    public const string UploadBatchIdTag = "uploadBatchId";

    [JsonPropertyName(CreatedByTag)]
    public string CreatedBy { get; set; } = string.Empty;

    [JsonPropertyName(FileIdTag)]
    public Guid FileId { get; set; } = Guid.NewGuid();

    [JsonPropertyName(UploadBatchIdTag)]
    public Guid UploadBatchId { get; set; } = Guid.NewGuid();
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
