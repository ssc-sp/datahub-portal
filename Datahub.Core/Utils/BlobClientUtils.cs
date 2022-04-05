using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Datahub.Core.Utils
{
    public class BlobClientUtils
    {
        readonly string _connectionString;
        readonly string _containerName;

        public BlobClientUtils(string connectionString, string containerName)
        {
            _connectionString = connectionString;
            _containerName = containerName;
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
}
