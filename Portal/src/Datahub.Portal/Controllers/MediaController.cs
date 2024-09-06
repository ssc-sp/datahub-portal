using System.Security.Cryptography;
using System.Text;
using Azure.Storage.Blobs;
using Datahub.Application.Configuration;
using Datahub.Core.Model.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Portal.Controllers;

[ApiController]
public class MediaController : Controller
{
    public static readonly string PostMediaSaltySecret = RandomNumberGenerator
        .GetBytes(128)
        .Select(b => b.ToString("X2"))
        .Aggregate((a, b) => a + b);

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
    [Authorize]
    public IActionResult GetMedia(string filePath)
    {
        if (_datahubPortalConfiguration?.Media?.StorageConnectionString is null)
            return Unauthorized("No token available");
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

    /// <summary>
    /// Redirect the video mp4 to the azure storage blob and return the video stream.
    /// </summary>
    /// <returns></returns>
    [HttpGet("api/docs/{**filePath}")]
    [Authorize]
    public IActionResult GetDocs(string filePath)
    {
        if (_datahubPortalConfiguration?.Media?.StorageConnectionString is null)
            return Unauthorized("No token available");
        var blobReference = CloudStorageAccount.Parse(_datahubPortalConfiguration.Media.StorageConnectionString)
            .CreateCloudBlobClient()
            .GetContainerReference("docs")
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
        if (Request.Form.Files.Count == 0)
        {
            return BadRequest("No files uploaded");
        }

        if (Request.Form.Files.Count > 1)
        {
            return BadRequest("Cannot upload more than one file at a time");
        }

        // validate the jwt bearer token to ensure the user is authenticated
        var tokenString = Request.Headers["Authorization"].ToString().Split(" ")[1];

        if (tokenString != PostMediaSaltySecret)
        {
            return Unauthorized();
        }

        var file = Request.Form.Files[0];
        var filePath = "/uploads/upload-" + Guid.NewGuid() + Path.GetExtension(file.FileName);
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