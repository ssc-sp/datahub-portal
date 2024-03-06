using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Datahub.Core.Data;
using Datahub.Core.Services.Api;
using Datahub.Infrastructure.Services.Storage;

namespace Datahub.Portal.Controllers;

[Route("[controller]/[action]")]
[AllowAnonymous]
public class PublicController : Controller
{
    private readonly DataRetrievalService dataRetrievalService;

    private ILogger<PublicController> _logger { get; set; }
    private IPublicDataFileService _pubFileService { get; set; }

    public PublicController(
        ILogger<PublicController> logger,
        IPublicDataFileService pubFileService,
        DataRetrievalService dataRetrievalService
    )
    {
        _logger = logger;
        _pubFileService = pubFileService;
        this.dataRetrievalService = dataRetrievalService;
    }

    public IActionResult HelloWorld()
    {
        _logger.LogDebug("Unauthenticated hello world");
        return Ok("hello world");
    }

    public async Task<IActionResult> BlobTest()
    {
        var filemd = new FileMetaData()
        {
            Filename = "privacy.html",
            Name = "privacy.html"
        };

        var project = "canmetrobo";

        _logger.LogDebug($"Downloading {filemd.Filename} from project {project}");

        var uri = await dataRetrievalService.GetUserDelegationSasBlob(DataRetrievalService.DEFAULTCONTAINERNAME, filemd.Filename, project);

        return Redirect(uri.ToString());
    }

    public async Task<IActionResult> DataLakeTest()
    {
        var filemd = new FileMetaData()
        {
            Folderpath = "nrcan-rncan.gc.ca/alexander.khavich",
            Filename = "serious.gif"
        };

        _logger.LogDebug($"Downloading {filemd.Filename}");

        var uri = await dataRetrievalService.DownloadFile(DataRetrievalService.DEFAULTCONTAINERNAME, filemd, null);
        return Redirect(uri.ToString());
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