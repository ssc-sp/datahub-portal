namespace Datahub.Core.Model;

public enum FileScanStatus
{
    ScanInProgress,
    Succeeded,
    Error
}

public class FileScanResult
{
    public required string FileName { get; set; }
    public FileScanStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? ScanDate { get; set; }
    public bool HideError { get; set; }
}

public interface IFileScanService
{
    Task<FileScanResult?> GetFileScanResultAsync(string fileName);
    Task<Dictionary<string, FileScanResult>> GetFileScanResultsAsync(List<string> fileNames);
    Task HideFileScanErrorAsync(string fileName);
}
