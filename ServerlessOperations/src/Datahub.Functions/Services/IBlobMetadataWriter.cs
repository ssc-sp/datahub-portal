using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Datahub.Functions.Services;

/// <summary>
/// Persists additional blob metadata required by the ClamAV workflow once a file is marked clean.
/// </summary>
public interface IBlobMetadataWriter
{
    /// <summary>
    /// Adds or updates metadata entries when access is re-enabled for a blob.
    /// </summary>
    /// <param name="workspaceAcronym">Workspace acronym that owns the blob.</param>
    /// <param name="blobPath">Path to the blob within the workspace container.</param>
    /// <param name="metadataSnapshot">Optional metadata payload received from Event Grid.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetAccessEnabledMetadataAsync(
        string workspaceAcronym,
        string blobPath,
        IReadOnlyDictionary<string, string>? metadataSnapshot,
        CancellationToken cancellationToken = default);
}
