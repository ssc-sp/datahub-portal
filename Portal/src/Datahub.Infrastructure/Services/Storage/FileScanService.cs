using Datahub.Core.Model;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services.Storage;

// Temporary implementation - in production this would connect to virus scanning service/database
public class FileScanService : IFileScanService
{
    private readonly ILogger<FileScanService> _logger;
    private readonly Dictionary<string, FileScanResult> _mockScanResults;
    private readonly Random _random;

    public FileScanService(ILogger<FileScanService> logger)
    {
        _logger = logger;
        _random = new Random();
        
        // Mock data for demonstration purposes - simulate various scan scenarios
        _mockScanResults = new Dictionary<string, FileScanResult>
        {
            // Example: Files with errors - using actual screenshot filenames for demo
            ["Screenshot 2026-01-22 082919.png"] = new FileScanResult
            {
                FileName = "Screenshot 2026-01-22 082919.png",
                Status = FileScanStatus.Error,
                ErrorMessage = "Potential threat detected. File signature matches known malware pattern.",
                ScanDate = DateTime.UtcNow.AddMinutes(-15),
                HideError = false
            },
            ["Screenshot 2026-01-22 091326.png"] = new FileScanResult
            {
                FileName = "Screenshot 2026-01-22 091326.png",
                Status = FileScanStatus.Error,
                ErrorMessage = "File appears to be corrupted or contains invalid data structures.",
                ScanDate = DateTime.UtcNow.AddMinutes(-8),
                HideError = false
            },
            ["Screenshot 2026-01-27 080903.png"] = new FileScanResult
            {
                FileName = "Screenshot 2026-01-27 080903.png",
                Status = FileScanStatus.Error,
                ErrorMessage = "Unable to scan encrypted or password-protected files.",
                ScanDate = DateTime.UtcNow.AddMinutes(-3),
                HideError = false
            },
            ["Screenshot 2026-01-26 112723.png"] = new FileScanResult
            {
                FileName = "Screenshot 2026-01-26 112723.png",
                Status = FileScanStatus.ScanInProgress, 
                ScanDate = DateTime.UtcNow,
                HideError = false
            },
            
            // Example: Files currently being scanned
            ["large-dataset.csv"] = new FileScanResult
            {
                FileName = "large-dataset.csv",
                Status = FileScanStatus.ScanInProgress,
                ScanDate = DateTime.UtcNow,
                HideError = false
            },
            ["video-file.mp4"] = new FileScanResult
            {
                FileName = "video-file.mp4",
                Status = FileScanStatus.ScanInProgress,
                ScanDate = DateTime.UtcNow,
                HideError = false
            }
        };
    }

    public Task<FileScanResult?> GetFileScanResultAsync(string fileName)
    {
        if (_mockScanResults.TryGetValue(fileName, out var result))
        {
            return Task.FromResult<FileScanResult?>(result);
        }

        // For demo purposes: randomly assign scan status to files not in mock data
        // In production, this would query the actual scan service/database
        var demoStatus = GetRandomScanStatus();
        
        var scanResult = new FileScanResult
        {
            FileName = fileName,
            Status = demoStatus,
            ErrorMessage = demoStatus == FileScanStatus.Error 
                ? $"An error occurred while scanning the file {fileName}. Please try again."
                : null,
            ScanDate = DateTime.UtcNow,
            HideError = false
        };

        // Cache the result for consistency during the session
        _mockScanResults[fileName] = scanResult;

        return Task.FromResult<FileScanResult?>(scanResult);
    }
    
    private FileScanStatus GetRandomScanStatus()
    {
        // Weighted random: 70% success, 20% scanning, 10% error
        var value = _random.Next(100);
        if (value < 70)
            return FileScanStatus.Succeeded;
        if (value < 90)
            return FileScanStatus.ScanInProgress;
        return FileScanStatus.Error;
    }

    public async Task<Dictionary<string, FileScanResult>> GetFileScanResultsAsync(List<string> fileNames)
    {
        var results = new Dictionary<string, FileScanResult>();
        
        foreach (var fileName in fileNames)
        {
            var result = await GetFileScanResultAsync(fileName);
            if (result != null)
            {
                results[fileName] = result;
            }
        }

        return results;
    }

    public Task HideFileScanErrorAsync(string fileName)
    {
        if (_mockScanResults.TryGetValue(fileName, out var result))
        {
            result.HideError = true;
        }

        return Task.CompletedTask;
    }
}
