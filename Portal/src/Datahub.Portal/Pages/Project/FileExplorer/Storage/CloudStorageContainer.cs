﻿using Datahub.Application.Services.Storage;

namespace Datahub.Portal.Pages.Project.FileExplorer.Storage;

public class CloudStorageContainer
{
    public CloudStorageContainer(string accountName, string containerName, CloudStorageProviderType provider, ICloudStorageManager storageManager, int? id = null, bool enabled = true)
    {
        ContainerName = containerName;
        AccountName = accountName;
        CloudStorageProvider = provider;
        StorageManager = storageManager;
        Id = id;
        Enabled = enabled;
    }

    public int? Id { get; set; }
    public bool Enabled { get; set; }
    public string Name => ContainerName;
    public string ContainerName { get; }
    public string AccountName { get; }
    public CloudStorageProviderType CloudStorageProvider { get; }
    public ICloudStorageManager StorageManager { get; }
}

public enum CloudStorageProviderType
{
    Azure,
    AWS,
    GCP
}