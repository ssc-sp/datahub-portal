using Datahub.Core.Storage;
using Datahub.Infrastructure.Services.Storage;
using Microsoft.AspNetCore.Mvc;

namespace Datahub.Portal.Controllers;

[Route(AzureCloudStorageManager.ControllerRoute)]
public class AzFileDownloadController : Controller
{
    private readonly IFileTokenService _fileTokenService;
    private readonly ILogger<AzFileDownloadController> _logger;

    public AzFileDownloadController(IFileTokenService fileTokenService, ILogger<AzFileDownloadController> logger)
    {
        _fileTokenService = fileTokenService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest("Missing token.");

        var entry = _fileTokenService.ResolveToken(token);
        if (entry is null)
        {
            _logger.LogWarning("File download token not found or expired");
            return NotFound("Token is invalid or has expired.");
        }
        var azStorageManager = entry.Manager as AzureCloudStorageManager;
        if (azStorageManager is null)
        {
            _logger.LogWarning("Invalid Storage Manager");
            return NotFound("Invalid Storage Manager.");
        }
        var client = await azStorageManager.GetBlobContainerClient(entry.Container);
        var blob = client.GetBlobClient(entry.FilePath);

        if (!await blob.ExistsAsync())
            return NotFound();
        //extract name from filePath
        var name = Path.GetFileName(entry.FilePath);
        var download = await blob.DownloadStreamingAsync();
        _logger.LogInformation("Serving file download for blob {BlobPath}, user {User}", entry.FilePath, entry.UserName);
        return File(
            download.Value.Content,
            contentType: "application/octet-stream",
            fileDownloadName: name,
            enableRangeProcessing: true
        );

    }
}
