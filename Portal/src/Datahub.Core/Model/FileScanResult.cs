namespace Datahub.Core.Model;

/// <summary>
/// Represents the current virus-scan lifecycle state for a file.
/// </summary>
public enum FileScanStatus
{
    /// <summary>
    /// The scan request has been accepted and is still running.
    /// </summary>
    ScanInProgress,

    /// <summary>
    /// The file scan completed successfully and no threats were found.
    /// </summary>
    Succeeded,

    /// <summary>
    /// The file scan completed with an error or threat detection.
    /// </summary>
    Error
}

/// <summary>
/// Represents the scan result metadata for a single file.
/// </summary>
public class FileScanResult
{
    /// <summary>
    /// Gets or sets the file name associated with this scan result.
    /// </summary>
    public required string FileName { get; set; }

    /// <summary>
    /// Gets or sets the current scan status for the file.
    /// </summary>
    public FileScanStatus Status { get; set; }

    /// <summary>
    /// Gets or sets optional error details when <see cref="Status"/> is <see cref="FileScanStatus.Error"/>.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp of the latest scan status update for the file.
    /// </summary>
    public DateTime? ScanDate { get; set; }
}

/// <summary>
/// Provides scan-result operations for files.
/// </summary>
public interface IFileScanService
{
    /// <summary>
    /// Gets the scan result for a single file.
    /// </summary>
    /// <param name="fileName">The file name to look up.</param>
    /// <returns>The scan result if available; otherwise <see langword="null"/>.</returns>
    Task<FileScanResult?> GetFileScanResultAsync(string fileName);

    /// <summary>
    /// Gets scan results for multiple files.
    /// </summary>
    /// <param name="fileNames">The file names to look up.</param>
    /// <returns>A dictionary keyed by file name containing scan results.</returns>
    Task<Dictionary<string, FileScanResult>> GetFileScanResultsAsync(List<string> fileNames);
}
