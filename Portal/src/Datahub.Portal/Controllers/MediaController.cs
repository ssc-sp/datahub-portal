using System.Security.Cryptography;
using System.Text;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Datahub.Application.Configuration;
using Datahub.Core.Model.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    /// Uses User Delegation SAS for enhanced security.
    /// </summary>
    /// <returns></returns>
    [HttpGet("api/media/{**filePath}")]
    public async Task<IActionResult> GetMedia(string filePath)
    {
        if (_datahubPortalConfiguration?.Media?.StorageConnectionString is null)
            return Unauthorized("No token available");

        try
        {
            var blobClient = await GetBlobClientWithUserDelegationSas("media", filePath);
            return Redirect(blobClient.Uri.ToString());
        }
        catch (Exception ex)
        {
            return BadRequest($"Error generating SAS token: {ex.Message}");
        }
    }

    /// <summary>
    /// Redirect the document to the azure storage blob and return the document stream.
    /// Uses User Delegation SAS for enhanced security.
    /// </summary>
    /// <returns></returns>
    [HttpGet("api/docs/{**filePath}")]
    public async Task<IActionResult> GetDocs(string filePath)
    {
        if (_datahubPortalConfiguration?.Media?.StorageConnectionString is null)
            return Unauthorized("No token available");

        try
        {
            var blobClient = await GetBlobClientWithUserDelegationSas("docs", filePath);
            return Redirect(blobClient.Uri.ToString());
        }
        catch (Exception ex)
        {
            return BadRequest($"Error generating SAS token: {ex.Message}");
        }
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

    /// <summary>
    /// Helper method to get a BlobClient with User Delegation SAS token
    /// </summary>
    private async Task<BlobClient> GetBlobClientWithUserDelegationSas(string containerName, string blobPath)
    {
        var blobServiceClient = new BlobServiceClient(_datahubPortalConfiguration.Media.StorageConnectionString);
        var accountName = blobServiceClient.AccountName;
        
        // Use DefaultAzureCredential for User Delegation SAS
        var credential = new DefaultAzureCredential();
        var blobUri = new Uri($"https://{accountName}.blob.core.windows.net");
        var authBlobServiceClient = new BlobServiceClient(blobUri, credential);
        
        // Get user delegation key
        var userDelegationKeyResponse = await authBlobServiceClient.GetUserDelegationKeyAsync(
            startsOn: DateTimeOffset.UtcNow.AddMinutes(-5),
            expiresOn: DateTimeOffset.UtcNow.AddMinutes(10));
        var userDelegationKey = userDelegationKeyResponse.Value;

        // Create SAS builder
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = blobPath,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(10)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        // Generate SAS token
        var sasToken = sasBuilder.ToSasQueryParameters(userDelegationKey, accountName);
        
        // Create URI with SAS
        var containerClient = authBlobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobPath);
        var blobUriBuilder = new BlobUriBuilder(blobClient.Uri)
        {
            Sas = sasToken
        };
        
        return new BlobClient(blobUriBuilder.ToUri());
    }
}