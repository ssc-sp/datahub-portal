using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Datahub.Core.Services.Api;
using Datahub.Infrastructure.Services.Storage;

namespace Datahub.Portal.Controllers;

[Route("[controller]/[action]")]
[Authorize]
public class PrivateStorageController : Controller
{
    private readonly DataRetrievalService dataRetrievalService;

    private ILogger<PublicController> _logger { get; set; }
    private IPublicDataFileService _pubFileService { get; set; }

    public PrivateStorageController(
        ILogger<PublicController> logger, 
        IPublicDataFileService pubFileService,
        DataRetrievalService dataRetrievalService
    )
    {
        _logger = logger;
        _pubFileService = pubFileService;
        this.dataRetrievalService = dataRetrievalService;
    }

    [Route("{fileId}")]
    public async Task<IActionResult> DownloadFile(string fileId)
    {
        var remoteIp = HttpContext.Connection.RemoteIpAddress;

        try
        {
            var fileIdGuid = Guid.Parse(fileId);
            var result = await _pubFileService.DownloadPublicUrlSharedFile(fileIdGuid, remoteIp);
            if (result == null)
            {
                _logger.LogError($"File not found: {fileId}");
                return NotFound();
            }
            else
            {
                return Redirect(result.ToString());
            }
        }
        catch (FormatException)
        {
            _logger.LogError($"Invalid file id (not a guid): {fileId}");
            return NotFound();
        }
    }
}