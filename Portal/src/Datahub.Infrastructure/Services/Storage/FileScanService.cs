using Datahub.Core.Model;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services.Storage;

public class FileScanService : IFileScanService
{
    private readonly ILogger<FileScanService> _logger;

    public FileScanService(ILogger<FileScanService> logger)
    {
        _logger = logger;
    }

    public Task<FileScanResult?> GetFileScanResultAsync(string fileName)
    {
        _logger.LogInformation("File scan status lookup requested for {FileName}, but no scan provider is configured.", fileName);

        var scanResult = new FileScanResult
        {
            FileName = fileName,
            Status = FileScanStatus.ScanInProgress,
            ScanDate = DateTime.UtcNow
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
}
