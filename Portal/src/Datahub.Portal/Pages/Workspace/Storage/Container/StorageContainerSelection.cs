using Datahub.Core.Storage;

namespace Datahub.Portal.Pages.Workspace.Storage.Container
{
    internal static class StorageContainerSelection
    {
        internal static string AccountKey(CloudStorageContainer container) => container.Id is int id
            ? $"{container.CloudStorageProvider}:external:{id}"
            : $"{container.CloudStorageProvider}:workspace:{container.AccountName}";

        internal static CloudStorageContainer? SelectAfterReload(
            IEnumerable<CloudStorageContainer> containers, CloudStorageContainer? previous, CloudStorageContainer? workspaceDefault)
        {
            var enabled = containers.Where(container => container.Enabled).ToList();
            return enabled.FirstOrDefault(container => previous is not null
                    && AccountKey(container) == AccountKey(previous) && container.ContainerName == previous.ContainerName)
                ?? enabled.FirstOrDefault(container => ReferenceEquals(container, workspaceDefault))
                ?? enabled.FirstOrDefault();
        }
    }
}
