using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Datahub.Core.Utils;

public class BlobClientUtils
{
    readonly string connectionString;
    readonly string containerName;

    public BlobClientUtils(string connectionString, string containerName)
    {
        this.connectionString = connectionString;
        this.containerName = containerName;
    }

    public async Task UploadFile(string fileName, Stream fileData, IDictionary<string, string> metadata, Action<long> progress)
    {
        var blobServiceClient = new BlobServiceClient(connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blob = containerClient.GetBlobClient(fileName);

        var uploadOptions = new BlobUploadOptions();
        if (progress is not null)
        {
            uploadOptions.ProgressHandler = new Progress<long>(progress);
        }

        await blob.UploadAsync(fileData, uploadOptions);
        await blob.SetMetadataAsync(metadata);
    }
}