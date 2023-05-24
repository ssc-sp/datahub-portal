using Datahub.Application.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace Datahub.Portal.Controllers;

[ApiController]
public class MediaController : Controller
{
    private readonly DatahubPortalConfiguration _datahubPortalConfiguration;

    public MediaController(DatahubPortalConfiguration datahubPortalConfiguration)
    {
        _datahubPortalConfiguration = datahubPortalConfiguration;
    }
    
    /// <summary>
    /// Redirect the video mp4 to the azure storage blob and return the video stream.
    /// </summary>
    /// <returns></returns>
    [HttpGet("api/media/{**filePath}")]
    public async Task<IActionResult> GetMedia(string filePath)
    {
        // verify the user is authenticated
        if (User.Identity is { IsAuthenticated: false })
        {
            return Unauthorized();
        }
        
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
}