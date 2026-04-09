using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Datahub.Core.Utils;

public class BlobClientUtils
{
    private readonly BlobContainerClient _containerClient;

    public BlobClientUtils(BlobContainerClient containerClient)
    {
        _containerClient = containerClient;
    }

    public async Task UploadFile(string fileName, Stream fileData, IDictionary<string, string> metadata, Action<long> progress)
    {
        var blob = _containerClient.GetBlobClient(fileName);

        var uploadOptions = new BlobUploadOptions();
        if (progress is not null)
        {
            uploadOptions.ProgressHandler = new Progress<long>(progress);
        }

        await blob.UploadAsync(fileData, uploadOptions);
        await blob.SetMetadataAsync(metadata);
    }
}
