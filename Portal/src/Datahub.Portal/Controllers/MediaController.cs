using System.Security.Cryptography;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Datahub.Application.Configuration;
using Microsoft.AspNetCore.Mvc;

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
    public IActionResult GetMedia(string filePath)
    {
        return RedirectToBlobWithReadSas("media", filePath);
    }

    /// <summary>
    /// Redirect the docs file to the azure storage blob and return the file stream.
    /// </summary>
    /// <returns></returns>
    [HttpGet("api/docs/{**filePath}")]
    public IActionResult GetDocs(string filePath)
    {
        return RedirectToBlobWithReadSas("docs", filePath);
    }

    private IActionResult RedirectToBlobWithReadSas(string containerName, string filePath)
    {
        if (string.IsNullOrWhiteSpace(_datahubPortalConfiguration?.Media?.StorageConnectionString))
            return Unauthorized("No token available");

        var blobServiceClient = new BlobServiceClient(_datahubPortalConfiguration.Media.StorageConnectionString);
        var blobClient = blobServiceClient
            .GetBlobContainerClient(containerName)
            .GetBlobClient(filePath);

        if (!blobClient.CanGenerateSasUri)
            return StatusCode(StatusCodes.Status500InternalServerError, "Storage client cannot generate SAS URI.");

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = filePath,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(5)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);
        var sasUri = blobClient.GenerateSasUri(sasBuilder);

        return Redirect(sasUri.ToString());
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
            await blobClient.UploadAsync(file.OpenReadStream(), overwrite: true);
            return Ok("/api/media/" + filePath);
        }
        catch (Exception e)
        {
            return BadRequest(e);
        }
    }
}
