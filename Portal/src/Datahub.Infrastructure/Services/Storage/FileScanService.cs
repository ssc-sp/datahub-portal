using Datahub.Core.Model;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services.Storage;

// Temporary implementation - in production this would connect to virus scanning service/database
public class FileScanService : IFileScanService
{
    private readonly ILogger<FileScanService> _logger;
    private readonly Dictionary<string, FileScanResult> _mockScanResults;

    public FileScanService(ILogger<FileScanService> logger)
    {
        _logger = logger;
        // Mock data for demonstration purposes
        _mockScanResults = new Dictionary<string, FileScanResult>();
    }

    public Task<FileScanResult?> GetFileScanResultAsync(string fileName)
    {
        if (_mockScanResults.TryGetValue(fileName, out var result))
        {
            return Task.FromResult<FileScanResult?>(result);
        }

        // Default to success for now - in production this would query scan service
        var scanResult = new FileScanResult
        {
            FileName = fileName,
            Status = FileScanStatus.Succeeded,
            ScanDate = DateTime.UtcNow,
            HideError = false
        };

        return Task.FromResult<FileScanResult?>(scanResult);
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
