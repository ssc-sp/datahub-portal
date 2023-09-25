using Datahub.Application.Services.Storage;

namespace Datahub.Portal.Pages.Project.FileExplorer.Storage;

public class CloudStorageContainer
{
    public CloudStorageContainer(string accountName, string containerName, CloudStorageProviderType provider, ICloudStorageManager storageManager)
    {
        ContainerName = containerName;
        AccountName = accountName;
        CloudStorageProvider = provider;
        StorageManager = storageManager;
    }

    public string Name => ContainerName;
    public string ContainerName { get; }
    public string AccountName { get; }
    public CloudStorageProviderType CloudStorageProvider { get; }
    public ICloudStorageManager StorageManager { get; }
}

public enum CloudStorageProviderType
{
    Azure,
    AWS
}