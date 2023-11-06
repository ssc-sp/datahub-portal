using Datahub.Core.Model.CloudStorage;

namespace Datahub.Portal.Pages.Project.FileExplorer.Storage
{
    public class CloudStorageSettings
    {
        public CloudStorageSettings()
        {
            Name = string.Empty;
            Enabled = true;
            ConnectionData = string.Empty;
        }

        public CloudStorageSettings(ProjectCloudStorage projectCloudStorage)
        {
            Name = projectCloudStorage.Name;
            Enabled = projectCloudStorage.Enabled;
            ConnectionData = projectCloudStorage.ConnectionData;
        }

        public void ApplyTo(ProjectCloudStorage projectCloudStorage)
        {
            projectCloudStorage.Enabled = Enabled;
            projectCloudStorage.Name = Name;
            projectCloudStorage.ConnectionData = ConnectionData;
        }

        public string Name { get; set; }
        public bool Enabled { get; set; }
        public string ConnectionData { get; set; }
    }
}
