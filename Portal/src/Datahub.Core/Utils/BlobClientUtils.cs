using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Datahub.Core.Utils;

public class BlobClientUtils
{
    private readonly string _connectionString;
    private readonly string _containerName;

    public BlobClientUtils(string connectionString, string containerName)
    {
        this._connectionString = connectionString;
        this._containerName = containerName;
    }

    public async Task UploadFile(string fileName, Stream fileData, IDictionary<string, string> metadata, Action<long> progress)
    {
        var blobServiceClient = new BlobServiceClient(_connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
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