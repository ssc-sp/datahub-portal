using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;

namespace Datahub.Portal.Services
{
    public class StaticStorageReader
    {
        private readonly ILogger<StaticStorageReader> logger;
        private readonly string storageConnectionString;
        private readonly BlobServiceClient blobServiceClient;
        public const string CONTAINER_NAME = "docset";

        public StaticStorageReader(ILogger<StaticStorageReader> logger,
                    IConfiguration config)
        {
            this.logger = logger;
            this.storageConnectionString = config.GetConnectionString("Docs");
            // Create a BlobServiceClient object which will be used to create a container client
            blobServiceClient = new BlobServiceClient(storageConnectionString);

        }

        public async Task<string> ReadDocToString(string name)
        {
            // Create the container and return a container client object
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(CONTAINER_NAME);
            var client = containerClient.GetBlobClient(name);
            if (await client.ExistsAsync())
            {
                var download = await client.DownloadAsync();
                using var stream = new MemoryStream();
                await download.Value.Content.CopyToAsync(stream);
                return System.Text.Encoding.UTF8.GetString(stream.ToArray());
            }
            throw new InvalidOperationException($"doc id {name} is not found");
        }

        public async Task<string> GetHTMLFromMDBlob(string mbBlobName)
        {
            var content = await ReadDocToString(mbBlobName);
            return Markdig.Markdown.ToHtml(content);
        }
    }
}
