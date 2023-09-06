namespace Datahub.Portal.Pages.Project.FileExplorer.Storage;

public class CloudStorage
{
    public CloudStorage(string name, string description, IEnumerable<CloudStorageContainer> containers)
    {
        Name = name;
        Description = description;
        Containers = containers.ToList();
    }

    public string Name { get; }
    public string Description { get; }
    public List<CloudStorageContainer> Containers { get; }
}
