using System.Collections.Generic;

namespace Datahub.Functions.Models;

/// <summary>
/// Minimal projection of the Event Grid payload for blob metadata/property updates.
/// </summary>
internal sealed class BlobMetadataEventData
{
    public string? Url { get; set; }

    public Dictionary<string, string>? Metadata { get; set; }
}
