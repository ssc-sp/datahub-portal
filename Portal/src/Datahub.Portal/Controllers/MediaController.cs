using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Datahub.Application.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;


namespace Datahub.Portal.Controllers;

[ApiController]
public class MediaController : Controller
{
    private readonly DatahubPortalConfiguration _datahubPortalConfiguration;
    private readonly ILogger<MediaController> _logger;

    public MediaController(DatahubPortalConfiguration datahubPortalConfiguration, ILogger<MediaController> logger)
    {
        _datahubPortalConfiguration = datahubPortalConfiguration;
        _logger = logger;
    }
    
    /// <summary>
    /// Redirect the video mp4 to the azure storage blob and return the video stream.
    /// </summary>
    /// <returns></returns>
    [HttpGet("api/media/{**filePath}")]
    [Authorize]
    public IActionResult GetMedia(string filePath)
    {
        var blobReference = CloudStorageAccount.Parse(_datahubPortalConfiguration.Media.StorageConnectionString)
            .CreateCloudBlobClient()
            .GetContainerReference("media")
            .GetBlockBlobReference(filePath);
            
        var sasToken = blobReference
            .GetSharedAccessSignature(new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(5)
            });
        
        var sasUrl = blobReference.Uri + sasToken;
        return Redirect(sasUrl);
    }

    [HttpPost("api/media/upload")]
    //[Authorize]
    public async Task<IActionResult> PostMedia()
    {
        Console.WriteLine(Request.Headers.Authorization);
        var file = Request.Form.Files[0];
        var filePath = "/uploads/upload-" + Guid.NewGuid()+Path.GetExtension(file.FileName);
        try
        {
            var blobServiceClient = new BlobServiceClient(_datahubPortalConfiguration.Media.StorageConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient("media");
            var blobClient = containerClient.GetBlobClient(filePath);
            await blobClient.UploadAsync(file.OpenReadStream());
            return Ok("/api/media/" + filePath);
        }
        catch (Exception e)
        {
            return BadRequest(e);
        }
    }
}