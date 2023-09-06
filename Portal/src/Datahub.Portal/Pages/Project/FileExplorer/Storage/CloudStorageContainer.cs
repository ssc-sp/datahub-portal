using Datahub.Application.Services.Storage;

namespace Datahub.Portal.Pages.Project.FileExplorer.Storage;

public class CloudStorageContainer
{
    public CloudStorageContainer(string name, ICloudStorageManager storageManager)
    {
        Name = name;
        StorageManager = storageManager;
    }

    public string Name { get; }
    public ICloudStorageManager StorageManager { get; }
}
